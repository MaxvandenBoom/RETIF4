/**
 * MainThread class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using NLog;
using RETIF4.Collectors;
using RETIF4.Data;
using RETIF4.Events;
using RETIF4.Experiment;
using RETIF4.Helpers;
using RETIF4.Matlab;
using RETIF4.Triggers;
using RETIF4.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace RETIF4 {

    public enum Status : int {
        STATUS_STARTING,                // starting phase
        STATUS_STARTED_STANDBY,         // phase is started but waiting for incoming scans to start running
        STATUS_RUNNING,                 // running until phase endtype is met or indefinitely running
        STATUS_STOPPING,                // in the process of stopping a phase
        STATUS_STOPPED                  // no phase started, phase is finished
    };

    public class MainThread {

        private const int threadLoopDelay = 50;		                        // thread loop delay (1000ms / 20 run times per second = rest 50ms)

        private static Logger logger = LogManager.GetLogger("MainThread");  // 
        private static IExperiment experiment = null;                       // the experiment to run

        private static bool running = false;                                 // flag to define if the experiment thread is still running (setting to false will stop the experiment thread) 
        private static bool matlabStartupOnStartProgram = false;            // start matlab at the start of the application. This occures only when one or more experiment phases has the 'requiresMatlab' flag on true
        private bool matlabRequired = false;                                // flag to hold whether matlab is required for the experiment (and thus might be running)

        private static IViewRF view = null;                                 // reference to the view, used to pull information from and push commands to
        private static bool viewStartupOnStartProgram = false;              // start the view at the start of the application

        private static ITrigger trigger = null;                             // 
        private static ICollector collector = null;                         // 

        private static Object phaseLock = new Object();                         // lock for phase manipulations
        private static Phase currentPhase = null;                               // the current phase of the experiment
        private static Phase.EndType currentPhaseEndType = Phase.EndType.NONE;  // the current phase it's endtype (so we do not have to retrieve it each time in the mainloop)
        private static int currentPhaseScanCounter = 0;                         // when the endtype of the phase is set to scans, this holds the number of scans left until the phase should stop
        private static long currentPhaseEndTime = 0;                            // when the endtype of the phase is set to time, this holds the time at which the phase should stop
        private static bool currentPhaseFinished = false;                       // track whether the phase was finished (when endtype is set to scans, time or tasksfinish)
        private static bool currentPhaseSceneStarted = false;                   // track whether the scene of the phase has been started

        private static Status currentStatus = Status.STATUS_STOPPED;            // the current status of the program

        private static Phase phaseToStart = null;                               // the phase that should be started (will be started in the mainloop)
        private static Phase.StartType phaseToStartType = 0;                    // the starttype of the phase that should be started (will be started in the mainloop)

        private Object volumeQueLock = new Object();                            // lock object to lock the volume que
        private List<Volume> volumeProcessingQue = new List<Volume>();          // array of volumes that require processing (first in first out)


        /**
	     * Experiment constructor
	     */
        public MainThread(IExperiment experiment) {

            // store a reference to the experiment
            MainThread.experiment = experiment;

        }

        // returns the view (partipants view oject)
        public static IViewRF getView() {
            return view;
        }

        // returns the view condition
        public static int getViewCondition() {
            if (view != null)   return view.getCondition();
            return -1;
        }

        // returns the trigger
        public static ITrigger getTrigger() {
            return trigger;
        }

        public static List<Phase> getPhases() {
            return experiment.getPhases();
        }

        public static string getWorkDirectory() {
            return experiment.getWorkDirectory();
        }

        // Retreive the current phase of the experiment
        public static Phase getCurrentPhase() {
            lock (phaseLock) {
                return currentPhase;
            }
        }

        /// Retrieve the current status
        public static Status getCurrentStatus() {
            return currentStatus;
        }

        // 
        public static void setMatlabStartupOnStartProgram(bool onStart) {
            matlabStartupOnStartProgram = onStart;
        }

        // 
        public static void setViewStartupOnStartProgram(bool onStart) {
            viewStartupOnStartProgram = onStart;
        }

        private void init() {

            // initialize the experiment
            experiment.init();

            ///
            /// View
            ///

            // create the view object (if specified)
            string viewClass = experiment.getViewClass();
            if (!String.IsNullOrEmpty(viewClass)) {
                try {
                    view = (IViewRF)Activator.CreateInstance(Type.GetType(viewClass));
                    logger.Info("Created a view instance of the class '" + viewClass + "'");
                } catch (Exception) {
                    logger.Error("Unable to create a view instance of the class '" + viewClass + "', in the experiment check the 'CONFIG_VIEW_CLASS' setting");
                }
            } else {
                logger.Error("No view class is defined for the experiment");
            }

            ///
            /// Phases
            ///


            // check if there is at least one phase, else give a warning
            if (experiment.getPhases() == null || experiment.getPhases().Count < 1) {
                logger.Error("No phases defined for the experiment");
            }



            ///
            /// Trigger
            ///

            // create the trigger object (if specified)
            string triggerClass = experiment.getTriggerClass();
            if (!String.IsNullOrEmpty(triggerClass)) {
                try {

                    // create a trigger object and add a listener (callback to processTrigger on trigger)
                    trigger = (ITrigger)Activator.CreateInstance(Type.GetType(triggerClass));
                    trigger.triggerHandler += processTrigger;       // add the method to the event handle

                    logger.Info("Created a trigger instance of the class '" + triggerClass + "'");

                } catch (Exception) {
                    logger.Error("Unable to create a trigger instance of the class '" + triggerClass + "', in the experiment check the 'CONFIG_TRIGGER_CLASS' setting");
                }
            } else {
                logger.Error("No trigger class is defined for the experiment");
            }


            ///
            /// Collector
            ///

            // create the view object (if specified)
            string collectorClass = experiment.getCollectorClass();
            if (!String.IsNullOrEmpty(collectorClass)) {
                try {

                    // create a collector object and a listener (callback to processImage on a new incoming image from the scanner)
                    collector = (ICollector)Activator.CreateInstance(Type.GetType(collectorClass));
                    collector.newVolumeHandler += newVolume;       // add the method to the event handle

                    logger.Info("Created a collector instance of the class '" + collectorClass + "'");

                } catch (Exception) {
                    logger.Error("Unable to create a collector instance of the class '" + collectorClass + "', in the experiment check the 'CONFIG_COLLECTOR_CLASS' setting");
                }
            } else {
                logger.Error("No collector class is defined for the experiment");
            }


            ///
            /// Matlab
            ///

            // check if matlab is required in at least one of the phases
            matlabRequired = false;
            string matlabPhases = "";
            List<Phase> phases = experiment.getPhases();
            for (int i = 0; i < phases.Count; i++) {
                if (phases[i].requiresMatlab) {
                    matlabRequired = true;
                    if (matlabPhases.Length != 0)
                        matlabPhases += ", ";
                    matlabPhases += phases[i].phaseID;
                }
            }
            if (matlabRequired) {
                // matlab is required

                // message
                logger.Info("Matlab is required for at least one of the phases (ID: " + matlabPhases + "), checking for matlab");

                if (MatlabWrapper.isMatlabAvailable()) {
                    // Matlab is available

                    logger.Info("Matlab installation was found");

                    // check if matlab should be started in the beginning
                    if (matlabStartupOnStartProgram) {

                        // message
                        logger.Info("Matlab is set to startup on start of application");

                        // start matlab
                        MatlabWrapper.startMatlab();

                    } else {

                        // message
                        logger.Warn("Starting Matlab during start of application is disabled, this might cause an unwanted delay at the beginning of the phase first to use matlab");

                    }


                } else {
                    // Matlab is not available

                    // message
                    logger.Error("Matlab is not available while some phase(s) (ID: " + matlabPhases + ") require it to run. These phases cannot be executed and could cause the experiment to fail during runtime");

                }

            }

            ///
            /// View
            ///

            // check if the view should be started automatically
            if (viewStartupOnStartProgram) {

                // message
                logger.Info("View is set to start on the start of application");

                // start the view
                view.start();

                // wait till the view is loaded or a maximum amount of 30 seconds (30.000 / 50 = 600)
                int waitCounter = 600;
                while (!view.isStarted() && waitCounter > 0) {
                    Thread.Sleep(50);
                    waitCounter--;
                }

            }


            // debug
            startPhase(phases[0], Phase.StartType.IMMEDIATE___SCENE_IMMEDIATE);


        }

        public static bool isRunning() {
            return running;
        }


        public void run() {

            // log message
            logger.Debug("Thread started");

            // initialize the thread and experiment
            init();

            /*
            List<Volume> corrVolume = Session.readSessionVolumeVariableFromFile("C:\\P33\\SessionData___StartPhase_47___20190430_151216.txt", "corrVolume");
            List<Volume> globalVolume = Session.readSessionVolumeVariableFromFile("C:\\P33\\SessionData___StartPhase_47___20190430_151216.txt", "globalMask");
            List<Volume> realignVolume = Session.readSessionVolumeVariableFromFile("C:\\P33\\SessionData___StartPhase_47___20190430_151216.txt", "realignVolume");
            List<Volume> volumesAcq = Session.readSessionVolumeVariableFromFile("C:\\P33\\SessionData___StartPhase_47___20190430_151216.txt", "CalibrationAcq");
            List<Volume> volumeRoi = Session.readSessionVolumeVariableFromFile("C:\\P33\\SessionData___StartPhase_47___20190430_151216.txt", "roiMask");
            Session.setVolumes("corrVolume", corrVolume);
            Session.setVolumes("globalMask", globalVolume);
            Session.setVolumes("realignVolume", realignVolume);
            Session.setVolumes("CalibrationAcq", volumesAcq);
            Session.setVolumes("roiMask", volumeRoi);
            Session.saveSessionBinary("C:\\P33\\session.dat");
            */

            /*
            // todo: debug
            List<Volume> corrVolume = Session.readSessionVolumeVariableFromFile("C:\\P33\\SessionData___test.txt", "corrVolume");
            List<Volume> globalVolume = Session.readSessionVolumeVariableFromFile("C:\\P33\\SessionData___test.txt", "globalMask");
            List<Volume> globalRTVolume = Session.readSessionVolumeVariableFromFile("C:\\P33\\SessionData___test.txt", "globalRTMask");
            List<Volume> volumesAcq = Session.readSessionVolumeVariableFromFile("C:\\P33\\SessionData___test.txt", "<CalibrationAcq>");
            List<Volume> realignVolume = Session.readSessionVolumeVariableFromFile("C:\\P33\\SessionData___test.txt", "realignVolume");
            List<Volume> volumesGlm = Session.readSessionVolumeVariableFromFile("C:\\P33\\SessionData___test.txt", "FeatureLocGLM");
            List<Volume> volumeRoi = Session.readSessionVolumeVariableFromFile("C:\\P33\\SessionData___test.txt", "roiMask");
            Session.setVolumes("corrVolume", corrVolume);
            Session.setVolumes("globalMask", globalVolume);
            Session.setVolumes("globalRTMask", globalRTVolume);
            Session.setVolumes("CalibrationAcq", volumesAcq);
            Session.setVolumes("realignVolume", realignVolume);
            Session.setVolumes("FeatureLocGLM", volumesGlm);
            Session.setVolumes("roiMask", volumeRoi);
            */



            // flag as running
            running = true;

            // loop while running
            while (running) {

                // thread safety for phase manipulations
                lock (phaseLock) {

                    // check if there is a current phase
                    if (currentPhase != null && !currentPhaseFinished) {

                        // check if the phase is finished
                        if (currentPhaseEndType == Phase.EndType.TASKSFINISH && currentPhase.tasksFinished) {

                            // message
                            logger.Info("Current phase (" + currentPhase.phaseID + ") reached end, task(s) finished");

                            // flag as finished
                            currentPhaseFinished = true;

                        } else if (currentPhaseEndType == Phase.EndType.TASKSFINISH_NOSCANS && currentPhase.tasksFinished) {

                            // check if there are no scans left in the buffer
                            if (getScansInBuffer() == 0) {
                                
                                // message
                                logger.Info("Current phase (" + currentPhase.phaseID + ") reached end, task(s) finished and no scans left in the buffer");

                                // flag as finished
                                currentPhaseFinished = true;

                            }

                        } else if (currentPhaseEndType == Phase.EndType.SCANS && currentPhaseScanCounter == 0) {

                            // message
                            logger.Info("Current phase (" + currentPhase.phaseID + ") reached end, " + currentPhase.getEndTypeNumScans() + " scans reached");

                            // flag as finished
                            currentPhaseFinished = true;

                        } else if (currentPhaseEndType == Phase.EndType.TIME && Stopwatch.GetTimestamp() > currentPhaseEndTime) {

                            // message
                            logger.Info("Current phase (" + currentPhase.phaseID + ") reached end, time limit " + (currentPhase.getEndTypeTimeMs() / 1000.0) + "s reached");

                            // flag as finished
                            currentPhaseFinished = true;

                        }


                        // check if the phase is finished
                        if (currentPhaseFinished) {

                            // stop the phase (collection and triggers)
                            // (note: later startPhaseLocked will also call this, but in case
                            // there is no nextphase, make sure the collection and triggers are already stopped here)
                            stopPhaseLocked();

                            // check if there is a next phase
                            if (currentPhase.nextPhase != null) {

                                // check if another phase to start was already set (most probably through the GUI)
                                if (phaseToStart == null) {
                                    // no phase to start set yet

                                    // set the next phase to start, taken from the currentphase
                                    phaseToStart = currentPhase.nextPhase;
                                    phaseToStartType = currentPhase.nextPhase.getStartType();

                                } else {
                                    // a phase to start is already set (from the GUI)

                                    // message
                                    logger.Warn("The task has finished but another phase was already set to be started (most probably through the GUI, the nextPhase setting from the current phase is ignored and instead phase already set to be started will be started.");

                                }

                            }

                        }

                    }


                    // check if there is a phase to start
                    if (phaseToStart != null) {

                        // start the phase
                        startPhaseLocked(phaseToStart, phaseToStartType);

                        // reset the phase to start (only start it once)
                        phaseToStart = null;
                        phaseToStartType = 0;

                    }


                    // volume processing here, no volume should be processed while phase manipulation are taking place
                    // lock the volume processing que
                    Volume volume = null;
                    lock (volumeQueLock) {

                        // retrieve the volume to be processed and remove it from the que
                        // (then directly the que it's lock can be lifted so new volumes can be added while processing)
                        if (volumeProcessingQue.Count > 0) {
                            volume = volumeProcessingQue[0];
                            volumeProcessingQue.RemoveAt(0);
                        }

                    }
                    if (volume != null) {
                        processVolume(volume);
                    }


                }

                // if still running then sleep to allow other processes
                if (running) {
                    Thread.Sleep(threadLoopDelay);
                }

            }

            // stop and close the view
            if (view != null) {
                view.stop();
                view = null;
            }

            // stop the phase
            stopPhaseLocked();

            // stop and destroy the trigger
            if (trigger != null) {
                trigger.destroy();
                trigger = null;
            }

            // stop and destroy the collector
            if (collector != null) {
                collector.destroy();
                collector = null;
            }

            // check if matlab was required and could have been started
            if (matlabRequired) {

                // stop matlab (this won't do anything if matlab is unavailable or already stopped)
                MatlabWrapper.closeMatlab();

            }

            // log message
            logger.Debug("Thread stopped");

        }


        /**
         * Function called by collector then a new scane volume is collected,
         * places the volume in the que for processing
         * 
         * @param e
         */
        public void newVolume(object sender, CollectorEventArgs e) {

            // check if there is a volume to process
            if (e.volume != null) {

                // lock the volume processing que
                lock (volumeQueLock) {

                    // add the new volume to the volume que for processing (by the experiment thread)
                    volumeProcessingQue.Add(e.volume);

                    // message
                    logger.Info("Experiment: new volume with ID " + e.volume.volumeId + ", placed in que for processing");

                }

            }

        }

        /**
         * Returns how many scans there are still in the buffer
         * 
         */
        private int getScansInBuffer() {
            lock (volumeQueLock) {
                return volumeProcessingQue.Count;
            }
        }

        /**
         * Function which processes a new volume
         * 
         * @param volume
         */
        public void processVolume(Volume volume) {

            // message
            logger.Info("Processing volume with ID " + volume.volumeId);

            // check if we need to count volumes (in regard to the phase ending)
            if (currentPhaseEndType == Phase.EndType.SCANS && !currentPhaseFinished)
                currentPhaseScanCounter--;

            // check if the scene is not started yet and should start at the first incoming scan
            if (view != null && !currentPhaseSceneStarted && currentPhase.getStartType() == Phase.StartType.IMMEDIATE___SCENE_AT_INCOMING_SCAN) {

                // message
                logger.Info("First scan of the phase, starting scene");

                // start the view scene
                view.startScene(currentPhase.getScene());

                // flag the scene for the current phase as started
                currentPhaseSceneStarted = true;

            }

            // store the auto process type of the phase in the volume
            volume.autoProcessType = currentPhase.getAutoProcessType();

            // start a stopwatch object to measure the auto-process time
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // add the volume to the session data
            Session.addVolume(volume);

            // check if the current phase requires auto-processing
            if (currentPhase.getAutoProcessType() != Phase.AutoProcessType.NONE) {
                // perform some form of auto processing

                // create a copy of the volume object (so it does not modify the orgininal stored in the session)
                Volume autoProcVolume = volume.clone();

                // build the copy filename prefix
                string cpyFilenamePrefix = "c";
                if (currentPhase.getAutoProcessWrite()) {
                    if (currentPhase.getAutoProcessType() == Phase.AutoProcessType.CORRECT_REALIGN ||
                        currentPhase.getAutoProcessType() == Phase.AutoProcessType.CORRECT_REALIGN_ROI ||
                        currentPhase.getAutoProcessType() == Phase.AutoProcessType.CORRECT_REALIGN_ROI_DETREND) cpyFilenamePrefix += "r";
                    if (currentPhase.getAutoProcessType() == Phase.AutoProcessType.CORRECT_REALIGN_SMOOTH ||
                        currentPhase.getAutoProcessType() == Phase.AutoProcessType.CORRECT_REALIGN_SMOOTH_ROI ||
                        currentPhase.getAutoProcessType() == Phase.AutoProcessType.CORRECT_REALIGN_SMOOTH_ROI_DETREND) cpyFilenamePrefix += "s";
                }
                cpyFilenamePrefix += "_";

                // create a new filepath for the corrected copy of the file
                autoProcVolume.filepath = Path.GetDirectoryName(autoProcVolume.filepath) + Path.DirectorySeparatorChar + cpyFilenamePrefix + Path.GetFileName(autoProcVolume.filepath);

                // copy the original
                if (NiftiHelper.copyNiftiFilesAndRename(volume.filepath, autoProcVolume.filepath)) {
                    // succesfull copy

                    // choose and apply the autocorrect process
                    if (currentPhase.getAutoProcessType() == Phase.AutoProcessType.CORRECT) {

                        // only correct the volume
                        if (!NiftiHelper.correctRealtimeNifti(autoProcVolume, true, true, false, 0, false, false, currentPhase.getAutoProcessWrite())) {

                            // message and empty the volume object
                            logger.Error("AutoProcess error, could RT correct the incoming volume");
                            autoProcVolume = null;

                        }

                    } else if (currentPhase.getAutoProcessType() == Phase.AutoProcessType.CORRECT_REALIGN) {

                        // correct the volume and realign
                        if (!NiftiHelper.correctRealtimeNifti(autoProcVolume, true, true, true, 0, false, false, currentPhase.getAutoProcessWrite())) {

                            // message and empty the volume object
                            logger.Error("AutoProcess error, could RT correct and realign the incoming volume");
                            autoProcVolume = null;

                        }

                    } else if (currentPhase.getAutoProcessType() == Phase.AutoProcessType.CORRECT_REALIGN_ROI) {

                        // correct the volume and realign
                        if (!NiftiHelper.correctRealtimeNifti(autoProcVolume, true, true, true, 0, true, false, currentPhase.getAutoProcessWrite())) {

                            // message and empty the volume object
                            logger.Error("AutoProcess error, could RT correct, realign and extract the ROI data from the incoming volume");
                            autoProcVolume = null;

                        }

                    } else if (currentPhase.getAutoProcessType() == Phase.AutoProcessType.CORRECT_REALIGN_ROI_DETREND) {

                        // correct the volume and realign
                        if (!NiftiHelper.correctRealtimeNifti(autoProcVolume, true, true, true, 0, true, true, currentPhase.getAutoProcessWrite())) {

                            // message and empty the volume object
                            logger.Error("AutoProcess error, could RT correct, realign and extract the ROI data from the incoming volume");
                            autoProcVolume = null;

                        }

                    } else if (currentPhase.getAutoProcessType() == Phase.AutoProcessType.CORRECT_REALIGN_SMOOTH) {

                        // correct the volume and realign
                        if (!NiftiHelper.correctRealtimeNifti(autoProcVolume, true, true, true, currentPhase.getAutoProcessSmoothingFWHM(), false, false, currentPhase.getAutoProcessWrite())) {

                            // message and empty the volume object
                            logger.Error("AutoProcess error, could RT correct and realign the incoming volume");
                            autoProcVolume = null;

                        }

                    } else if (currentPhase.getAutoProcessType() == Phase.AutoProcessType.CORRECT_REALIGN_SMOOTH_ROI) {

                        // correct the volume and realign
                        if (!NiftiHelper.correctRealtimeNifti(autoProcVolume, true, true, true, currentPhase.getAutoProcessSmoothingFWHM(), true, false, currentPhase.getAutoProcessWrite())) {

                            // message and empty the volume object
                            logger.Error("AutoProcess error, could RT correct, realign and extract the ROI data from the incoming volume");
                            autoProcVolume = null;

                        }

                    } else if (currentPhase.getAutoProcessType() == Phase.AutoProcessType.CORRECT_REALIGN_SMOOTH_ROI_DETREND) {

                        // correct the volume and realign
                        if (!NiftiHelper.correctRealtimeNifti(autoProcVolume, true, true, true, currentPhase.getAutoProcessSmoothingFWHM(), true, true, currentPhase.getAutoProcessWrite())) {

                            // message and empty the volume object
                            logger.Error("AutoProcess error, could RT correct, realign and extract the ROI data from the incoming volume");
                            autoProcVolume = null;

                        }

                    }

                    // check if the auto processing steps on the incoming volume were completed succesfully
                    if (autoProcVolume == null) {

                        // return immediately and don't pass any volume to experiment for processing
                        return;

                    }

                    // flag the auto processing in the volume as a success
                    autoProcVolume.autoProcessSuccess = true;

                    // replace the incoming volume with the auto-processed volume
                    volume = autoProcVolume;

                } else {
                    // error while creating copies

                    // message and empty the volume object
                    logger.Error("AutoProcess error, could not create a copy of the incoming volume");
                    autoProcVolume = null;

                    // return immediately and don't pass any volume to experiment for processing
                    return;

                }

            } else {
                // auto processing set to none

                // flag the auto processing as a success
                volume.autoProcessSuccess = true;

            }

            // stop the processing stopwatch and store the elapsed processing time in the volume
            sw.Stop();
            volume.autoProcessTime = sw.Elapsed.TotalMilliseconds;

            // pass (auto-processed) volume to the experiment to handle
            experiment.processVolume(ref currentPhase, volume);

        }

        static void processTrigger(object sender, TriggerEventArgs e) {

            // pass to the experiment to handle
            experiment.processTrigger(ref currentPhase, e);

        }

        /**
         * event called when the GUI is dispatched and closed
         */
        public static void eventGUIClosed() {

            // stop the program from running
            running = false;

        }


        // start (or plan to start) a certain phase in the experiment
        public static void startPhase(Phase phase, Phase.StartType starttype) {

            // threadsafety for phase manipulations
            lock (phaseLock) {

                phaseToStart = phase;
                phaseToStartType = starttype;

            }   // end lock

        }

        private static void startPhaseLocked(Phase phase, Phase.StartType starttype) {

            // TODO: check if the phase need matlab to run, cancel start if not available
            // TODO: if matlab is needed and not started, then start it here


            // stopped = no task started or a task was cancelled
            // standby and waiting for scans = no task is running, next phase is set and program is waiting for a scan to trigger the start
            // running = either waiting (on a timer to callback) or processing scans (neurofeedback)
            // running and next phase standby = either waiting (on a timer to callback) or processing scans (neurofeedback)
            // done = finished the task at hand and ready to continue to the next with no instructions on what should happen next

            // stop the phase if it is still running
            stopPhaseLocked();

            // set the status to starting
            currentStatus = Status.STATUS_STARTING;

            // message
            string msgText = "Starting phase " + phase.phaseID;
            if (phase.getEndType() == Phase.EndType.NONE)           msgText += ", phase will run without ending";
            if (phase.getEndType() == Phase.EndType.SCANS)          msgText += ", phase will run for " + phase.getEndTypeNumScans() + " scans";
            if (phase.getEndType() == Phase.EndType.TASKSFINISH)    msgText += ", phase will run until task(s) finish";
            if (phase.getEndType() == Phase.EndType.TIME)           msgText += ", phase will run for " + (phase.getEndTypeTimeMs() / 1000.0) + " seconds";
            logger.Info(msgText);

            // set the phase locally
            currentPhase = phase;

            // check if the scene should change
            if (view != null && phase.getScene() != -1 && phase.getStartType() == Phase.StartType.IMMEDIATE___SCENE_IMMEDIATE) {

                // message
                logger.Info("Starting scene immediately");

                // start the view scene
                view.startScene(phase.getScene());

                // flag the scene for the current phase as started
                currentPhaseSceneStarted = true;

            } else {

                // check if there is a pre-scene
                if (phase.getPreScene() != -1) {

                    // message
                    logger.Info("Scene will be started at incoming scan, now starting pre-scene");

                    // start the pre-scene
                    view.startPreScene(phase.getPreScene());

                } else {

                    // message
                    logger.Info("Scene will be started at incoming scan");

                }


                // flag the scene for the current phase as not started
                currentPhaseSceneStarted = false;

            }

            // start listening to triggers
            if (trigger != null) {
                trigger.start();
            }

            // check if data should be collected for this task
            if (phase.collect) {

                // start collection
                collector.start(phase.collectionName);

            }

            // check if matlab is required and available
            if (phase.requiresMatlab && MatlabWrapper.getMatlabState() != MatlabWrapper.MATLAB_UNAVAILABLE) {

                // wait for matlab to stop
                if (MatlabWrapper.getMatlabState() == MatlabWrapper.MATLAB_STOPPING) {
                    // todo
                }

                // if matlab is stopped then start it
                if (MatlabWrapper.getMatlabState() == MatlabWrapper.MATLAB_STOPPED) {

                    // message
                    logger.Warn("Matlab is not started for this step and will be started now, consider setting the configuration to have Matlab started at the beginning of the application");

                    // start matlab
                    MatlabWrapper.startMatlab();

                }

            }


            // set the status to starting
            if (phase.getStartType() == Phase.StartType.IMMEDIATE___SCENE_IMMEDIATE)
                currentStatus = Status.STATUS_RUNNING;
            else
                currentStatus = Status.STATUS_STARTED_STANDBY;

            // set or reset the task end variables
            phase.tasksFinished = false;
            currentPhaseEndType = phase.getEndType();
            currentPhaseScanCounter = 0;
            currentPhaseEndTime = 0;
            if (currentPhaseEndType == Phase.EndType.TIME)  currentPhaseEndTime = Stopwatch.GetTimestamp() + (int)(Stopwatch.Frequency * (phase.getEndTypeTimeMs() / 1000.0));
            if (currentPhaseEndType == Phase.EndType.SCANS) currentPhaseScanCounter = phase.getEndTypeNumScans();
            currentPhaseFinished = false;

            // pass to the experiment to handle
            experiment.startPhase(ref currentPhase);

        }

        private static void stopPhaseLocked() {
            if (currentStatus != Status.STATUS_STARTED_STANDBY && currentStatus != Status.STATUS_RUNNING)     return;
            
            // TODO: check if a phase is being started and wait till it is finished starting
            //Status.STATUS_STARTING

            // set the status to stopping
            currentStatus = Status.STATUS_STOPPING;

            // stop the collector and trigger if they are running
            if (collector != null && collector.isCollecting()) collector.stop();

            // stop listening to triggers
            if (trigger != null) {
                trigger.stop();
            }

            // pass to the experiment to handle
            experiment.stopPhase(ref currentPhase);

            // set the status to stopped
            currentStatus = Status.STATUS_STOPPED;

        }
        

        /**
         * Callback function, called at the start of each trial in the view
         * 
         * Initiated by the View, forwarded to here
         **/
        public static void processViewTrialStart(double condition) {
            experiment.processViewTrialStart(ref currentPhase, condition);
        }

        /**
         * Get the most recent feedback value from the experiment
         * 
         * Most likely to be called from the view
         **/
        public static double getFeedbackValue() {
            return experiment.getFeedbackValue(ref currentPhase);
        }

        /**
         * Get the most recent feedback values from the experiment
         * 
         * Most likely to be called from the view
         **/
        public static double[] getFeedbackValues() {
            return experiment.getFeedbackValues(ref currentPhase);
        }

    }

}
