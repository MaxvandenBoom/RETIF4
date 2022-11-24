/**
 * AmygExperiment class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using NLog;
using RETIF4.Data;
using RETIF4.Events;
using RETIF4.Helpers;
using RETIF4.Matlab;
using RETIF4.Tasks;
using RETIF4.Views;
using System.Collections.Generic;
using System.IO;
using Scenes = RETIF4.Views.AmygView.Scenes;

namespace RETIF4.Experiment {
    
    public class AmygExperiment : ExperimentBase, IExperiment {
        
        private static Logger logger = LogManager.GetLogger("AmygExperiment");

        public enum PhaseIds : int {
            NoPhase = 0,
            CalibrateView = 1,
            ScannerSetup = 3,
            RealtimeCorrectionVolume = 5,
            FuncGlobalToNativeMask = 7,
            AnatGlobalToNativeMask = 9,
            RealignmentVolume = 12,
            Localizer_Pretest_TaskInstr = 20,
            Localizer_Pretest_Acquire = 22,

            Localizer_Pretest_GLM = 28,
            Localizer_Pretest_ROIMask = 30,

            Neurofeedback_Run1_TaskInstr = 40,
            Neurofeedback_Run1 = 47,

            Neurofeedback_Run2_TaskInstr = 50,
            Neurofeedback_Run2 = 57,

            Neurofeedback_Run3_TaskInstr = 60,
            Neurofeedback_Run3 = 67,

            Transfer_TaskInstr = 70,
            Transfer_Task = 77,


            Test = 100,


        };

        private new const string CONFIG_VIEW_CLASS = "RETIF4.Views.AmygView";
        private new const string CONFIG_COLLECTOR_CLASS = "RETIF4.Collectors.CollectorPhilips7TUMCU";
        private new const string CONFIG_TRIGGER_CLASS = "RETIF4.Triggers.SerialTrigger";

        //private const string CONFIG_GLOBAL_MASK_FILE = "MNI_MASK_BR_LHAND_BIN.nii";
        private const string CONFIG_GLOBAL_MASK_FILE = "MNI_MASK_AMYG_TOTAL.nii";
        private const string CONFIG_EPI_MNI_FILE = "MNI_EPI_TEMPLATE_TOTAL.nii";

        //private const int CONFIG_ROIMASK_ABS_NUM_HIGHEST_VOXELS = 120;
        private const double CONFIG_ROIMASK_PERC_HIGHEST_VOXELS = 0.33;
        
        private const double CONFIG_GLM_TR = 1.915;
        private const int CALIBRATION_PRETEST_NUMVOLUMES = 209; // task 1
        private const int NEUROFEEDBACK_NUMVOLUMES = 329;
        private const int TRANSFERTASK_NUMVOLUMES = 329;

        private string workDirectory = "C:\\P33\\";

        // tasks
        private RTCorrectionVolumeTask      rtCorrectionVolumeTask = null;
        private FuncMniToNativeMaskTask     funcMniToNativeMaskTask = null;
        private AnatMniToNativeMaskTask     anatMniToNativeMaskTask = null;
        private RealignmentVolumeTask       realignmentVolumeTask = null;
        private SimpleGLMTask               localizerGLMTask = null;
        private LocalizerROIMaskTask        localizerROIMaskTask = null;
        private AmplitudeFeedbackTask       feedbackTask = null;


        public string getWorkDirectory() {
            return workDirectory;
        }

        public void init() {

            // thread safety
            lock (configurationLock) {

                // set the view, collector and trigger class
                base.CONFIG_VIEW_CLASS = CONFIG_VIEW_CLASS;
                base.CONFIG_COLLECTOR_CLASS = CONFIG_COLLECTOR_CLASS;
                base.CONFIG_TRIGGER_CLASS = CONFIG_TRIGGER_CLASS;

                // setting the program's behaviour
                MainThread.setMatlabStartupOnStartProgram(true);        // start matlab at the start of the application (only when one or more experiment phases has the 'requiresMatlab' flag on true)
                MainThread.setViewStartupOnStartProgram(true);          // start the view at the start of the application
                MatlabWrapper.setSendCommandMethod(0);                  // set the method used to send commands to 1 = using sending text to the command window (gives feedback in the command window/console)
                MatlabWrapper.setShowCommandWindow(true);               // show the matlab instance command window

                
                //
                // create tasks
                //
                rtCorrectionVolumeTask      = new RTCorrectionVolumeTask(workDirectory);
                funcMniToNativeMaskTask     = new FuncMniToNativeMaskTask(  workDirectory, 
                                                                            (IOHelper.getProgramDirectory() + "templates" + Path.DirectorySeparatorChar + CONFIG_EPI_MNI_FILE),
                                                                            (IOHelper.getProgramDirectory() + "templates" + Path.DirectorySeparatorChar + CONFIG_GLOBAL_MASK_FILE),
                                                                            0);
                anatMniToNativeMaskTask     = new AnatMniToNativeMaskTask(  workDirectory,
                                                                            (IOHelper.getProgramDirectory() + "templates" + Path.DirectorySeparatorChar + CONFIG_GLOBAL_MASK_FILE)  );
                realignmentVolumeTask       = new RealignmentVolumeTask(workDirectory);
                localizerGLMTask            = new SimpleGLMTask(workDirectory);
                //localizerROIMaskTask        = new LocalizerROIMaskTask(workDirectory, CONFIG_ROIMASK_ABS_NUM_HIGHEST_VOXELS);
                localizerROIMaskTask        = new LocalizerROIMaskTask(workDirectory, CONFIG_ROIMASK_PERC_HIGHEST_VOXELS);
                feedbackTask                = new AmplitudeFeedbackTask(workDirectory);


                //
                // define phases
                //


                //
                // preperatory phases
                //

                Phase phase;
                phase = new Phase((int)PhaseIds.NoPhase, "No phase / Program wait / Black screen");
                phase.setSceneImmediate((int)Scenes.Black);
                phase.collect = false;
                phase.setEndTypeNone();
                phases.Add(phase);

                phase = new Phase((int)PhaseIds.CalibrateView, "Calibrate view");
                phase.setSceneImmediate((int)Scenes.CalibrateView);
                phase.collect = false;
                phase.setEndTypeNone();
                phases.Add(phase);
                
                phase = new Phase((int)PhaseIds.ScannerSetup, "Scanner setup");
                phase.setSceneImmediate((int)Scenes.ScannerSetup);
                phase.collect = false;
                phase.setEndTypeNone();
                phases.Add(phase);

                Phase phaseRtCorrectionVolume = new Phase((int)PhaseIds.RealtimeCorrectionVolume, "Real-time correction volume");
                phaseRtCorrectionVolume.setSceneImmediate((int)Scenes.ScannerSetup);
                phaseRtCorrectionVolume.collect = true;
                phaseRtCorrectionVolume.collectionName = "rt_correction_volume";
                phaseRtCorrectionVolume.requiresMatlab = true;
                phaseRtCorrectionVolume.setEndTypeTasksFinish(false);
                phases.Add(phaseRtCorrectionVolume);

                Phase phaseFuncGlobalMask = new Phase((int)PhaseIds.FuncGlobalToNativeMask, "Global mask - mni to native (use functional)");
                phaseFuncGlobalMask.setSceneImmediate((int)Scenes.ScannerSetup);
                phaseFuncGlobalMask.collect = true;
                phaseFuncGlobalMask.collectionName = "globalmask__mni_to_native__func";
                phaseFuncGlobalMask.requiresMatlab = true;
                phaseFuncGlobalMask.setEndTypeTasksFinish(false);
                phaseFuncGlobalMask.setAutoProcessType(Phase.AutoProcessType.CORRECT);
                phaseFuncGlobalMask.setAutoProcessWrite(true);
                phases.Add(phaseFuncGlobalMask);

                Phase phaseAnatGlobalMask = new Phase((int)PhaseIds.AnatGlobalToNativeMask, "Global mask - mni to native (use anatomical)");
                phaseAnatGlobalMask.setSceneImmediate((int)Scenes.ScannerSetup);
                phaseAnatGlobalMask.collect = true;
                phaseAnatGlobalMask.collectionName = "globalmask__mni_to_native__anat";
                phaseAnatGlobalMask.requiresMatlab = true;
                phaseAnatGlobalMask.setEndTypeTasksFinish(false);
                phaseAnatGlobalMask.setAutoProcessType(Phase.AutoProcessType.NONE);
                phaseAnatGlobalMask.setAutoProcessWrite(false);
                phases.Add(phaseAnatGlobalMask);

                Phase phaseRealignmentVolume = new Phase((int)PhaseIds.RealignmentVolume, "Realignment volume");
                phaseRealignmentVolume.setSceneImmediate((int)Scenes.ScannerSetup);
                phaseRealignmentVolume.collect = true;
                phaseRealignmentVolume.collectionName = "realignment_volume";
                phaseRealignmentVolume.requiresMatlab = true;
                phaseRealignmentVolume.setEndTypeTasksFinish(false);
                phaseRealignmentVolume.setAutoProcessType(Phase.AutoProcessType.CORRECT);
                phaseRealignmentVolume.setAutoProcessWrite(true);
                phases.Add(phaseRealignmentVolume);


                Phase phaseLocalizerInstr = new Phase((int)PhaseIds.Localizer_Pretest_TaskInstr, "Localizer / pretest task - Task instruction");
                phaseLocalizerInstr.setSceneImmediate((int)Scenes.LocalizerInstructions);
                phaseLocalizerInstr.collect = false;
                //phaseLocalizerInstr.collectionName = "localizer_1_instr";
                phaseLocalizerInstr.requiresMatlab = false;
                phaseLocalizerInstr.setEndTypeTime(5000);
                //phaseAnatGlobalMask.setAutoProcessType(Phase.AutoProcessType.NONE);
                //phaseAnatGlobalMask.setAutoProcessWrite(false);
                phases.Add(phaseLocalizerInstr);

                Phase phaseLocalizerAcq = new Phase((int)PhaseIds.Localizer_Pretest_Acquire, "Localizer / pretest task - Acquisition");
                //phaseLocalizerAcq.setSceneImmediate((int)Scenes.Localizer);
                phaseLocalizerAcq.setSceneAtIncomingScan((int)Scenes.Localizer, -1);
                phaseLocalizerAcq.collect = true;
                phaseLocalizerAcq.collectionName = "localizer_2_acq";
                phaseLocalizerAcq.requiresMatlab = true;
                phaseLocalizerAcq.setEndTypeScans(CALIBRATION_PRETEST_NUMVOLUMES);
                phaseLocalizerAcq.setAutoProcessType(Phase.AutoProcessType.CORRECT_REALIGN_SMOOTH);
                phaseLocalizerAcq.setAutoProcessWrite(true);
                phases.Add(phaseLocalizerAcq);

                Phase phaseLocalizerGLM = new Phase((int)PhaseIds.Localizer_Pretest_GLM, "Localizer / pretest task - Analyze (GLM)");
                phaseLocalizerGLM.setSceneImmediate((int)Scenes.Break_1);
                phaseLocalizerGLM.collect = false;
                //phaseLocalizerGLM.collectionName = "localizer_3_glm";
                phaseLocalizerGLM.requiresMatlab = true;
                phaseLocalizerGLM.setEndTypeTasksFinish(true);
                //phaseLocalizerGLM.setAutoProcessType(Phase.AutoProcessType.CORRECT_REALIGN_SMOOTH);
                //phaseLocalizerGLM.setAutoProcessWrite(true);
                phases.Add(phaseLocalizerGLM);

                Phase phaseLocalizerRoiMask = new Phase((int)PhaseIds.Localizer_Pretest_ROIMask, "Localizer / pretest task - Create ROI Mask");
                phaseLocalizerRoiMask.setSceneImmediate((int)Scenes.Break_1);
                phaseLocalizerRoiMask.collect = false;
                //phaseLocalizerRoiMask.collectionName = "localizer_4_roimask";
                phaseLocalizerRoiMask.requiresMatlab = true;
                phaseLocalizerRoiMask.setEndTypeTasksFinish(true);
                //phaseLocalizerRoiMask.setAutoProcessType(Phase.AutoProcessType.CORRECT_REALIGN_SMOOTH);
                //phaseLocalizerRoiMask.setAutoProcessWrite(true);
                phases.Add(phaseLocalizerRoiMask);




                //
                // Neurofeedback run 1
                //

                Phase phaseFeedbackRun1Instr = new Phase((int)PhaseIds.Neurofeedback_Run1_TaskInstr, "Neurofeedback Run 1 - Task instruction / break");
                phaseFeedbackRun1Instr.setSceneImmediate((int)Scenes.Feedback_Run1_Instructions);
                phaseFeedbackRun1Instr.collect = false;
                //phaseFeedbackRun1Instr.collectionName = "feedback_5_r1_instr";
                phaseFeedbackRun1Instr.requiresMatlab = false;
                phaseFeedbackRun1Instr.setEndTypeTime(5000);
                //phaseFeedbackRun1Instr.setAutoProcessType(Phase.AutoProcessType.NONE);
                //phaseFeedbackRun1Instr.setAutoProcessWrite(false);
                phases.Add(phaseFeedbackRun1Instr);

                Phase phaseFeedbackRun1 = new Phase((int)PhaseIds.Neurofeedback_Run1, "Neurofeedback - Run 1");
                phaseFeedbackRun1.setSceneAtIncomingScan((int)Scenes.Feedback_Run1, (int)Scenes.Feedback_Run1_Instructions);
                phaseFeedbackRun1.collect = true;
                phaseFeedbackRun1.collectionName = "feedback_6_r1";
                phaseFeedbackRun1.requiresMatlab = true;
                phaseFeedbackRun1.setEndTypeScans(NEUROFEEDBACK_NUMVOLUMES);
                phaseFeedbackRun1.setAutoProcessType(Phase.AutoProcessType.CORRECT_REALIGN_SMOOTH_ROI);
                phaseFeedbackRun1.setAutoProcessWrite(false);
                phases.Add(phaseFeedbackRun1);


                //
                // Neurofeedback run 2
                //

                Phase phaseFeedbackRun2Instr = new Phase((int)PhaseIds.Neurofeedback_Run2_TaskInstr, "Neurofeedback Run 2 - Task instruction / break");
                phaseFeedbackRun2Instr.setSceneImmediate((int)Scenes.Feedback_Run2_Instructions);
                phaseFeedbackRun2Instr.collect = false;
                //phaseFeedbackRun2Instr.collectionName = "feedback_7_r2_instr";
                phaseFeedbackRun2Instr.requiresMatlab = false;
                phaseFeedbackRun2Instr.setEndTypeTime(5000);
                //phaseFeedbackRun2Instr.setAutoProcessType(Phase.AutoProcessType.NONE);
                //phaseFeedbackRun2Instr.setAutoProcessWrite(false);
                phases.Add(phaseFeedbackRun2Instr);

                Phase phaseFeedbackRun2 = new Phase((int)PhaseIds.Neurofeedback_Run2, "Neurofeedback - Run 2");
                phaseFeedbackRun2.setSceneAtIncomingScan((int)Scenes.Feedback_Run2, (int)Scenes.Feedback_Run2_Instructions);
                phaseFeedbackRun2.collect = true;
                phaseFeedbackRun2.collectionName = "feedback_8_r2";
                phaseFeedbackRun2.requiresMatlab = true;
                phaseFeedbackRun2.setEndTypeScans(NEUROFEEDBACK_NUMVOLUMES);
                phaseFeedbackRun2.setAutoProcessType(Phase.AutoProcessType.CORRECT_REALIGN_SMOOTH_ROI);
                phaseFeedbackRun2.setAutoProcessWrite(false);
                phases.Add(phaseFeedbackRun2);


                //
                // Neurofeedback run 3
                //

                Phase phaseFeedbackRun3Instr = new Phase((int)PhaseIds.Neurofeedback_Run3_TaskInstr, "Neurofeedback Run 3 - Task instruction / break");
                phaseFeedbackRun3Instr.setSceneImmediate((int)Scenes.Feedback_Run3_Instructions);
                phaseFeedbackRun3Instr.collect = false;
                //phaseFeedbackRun3Instr.collectionName = "feedback_9_r3_instr";
                phaseFeedbackRun3Instr.requiresMatlab = false;
                phaseFeedbackRun3Instr.setEndTypeTime(5000);
                //phaseFeedbackRun3Instr.setAutoProcessType(Phase.AutoProcessType.NONE);
                //phaseFeedbackRun3Instr.setAutoProcessWrite(false);
                phases.Add(phaseFeedbackRun3Instr);

                Phase phaseFeedbackRun3 = new Phase((int)PhaseIds.Neurofeedback_Run3, "Neurofeedback - Run 3");
                phaseFeedbackRun3.setSceneAtIncomingScan((int)Scenes.Feedback_Run3, (int)Scenes.Feedback_Run3_Instructions);
                phaseFeedbackRun3.collect = true;
                phaseFeedbackRun3.collectionName = "feedback_10_r3";
                phaseFeedbackRun3.requiresMatlab = true;
                phaseFeedbackRun3.setEndTypeScans(NEUROFEEDBACK_NUMVOLUMES);
                phaseFeedbackRun3.setAutoProcessType(Phase.AutoProcessType.CORRECT_REALIGN_SMOOTH_ROI);
                phaseFeedbackRun3.setAutoProcessWrite(false);
                phases.Add(phaseFeedbackRun3);


                //
                // Transfer task
                //

                Phase phaseTransferTaskInstr = new Phase((int)PhaseIds.Transfer_TaskInstr, "Transfer task - Task instruction / break");
                phaseTransferTaskInstr.setSceneImmediate((int)Scenes.Transfer_Instructions);
                phaseTransferTaskInstr.collect = false;
                //phaseTransferTaskInstr.collectionName = "transfer_11_instr";
                phaseTransferTaskInstr.requiresMatlab = false;
                phaseTransferTaskInstr.setEndTypeTime(5000);
                //phaseTransferTaskInstr.setAutoProcessType(Phase.AutoProcessType.NONE);
                //phaseTransferTaskInstr.setAutoProcessWrite(false);
                phases.Add(phaseTransferTaskInstr);

                Phase phaseTransferTask = new Phase((int)PhaseIds.Transfer_Task, "Transfer task");
                phaseTransferTask.setSceneAtIncomingScan((int)Scenes.Transfer, (int)Scenes.Transfer_Instructions);
                phaseTransferTask.collect = true;
                phaseTransferTask.collectionName = "transfer_12";
                phaseTransferTask.requiresMatlab = true;
                phaseTransferTask.setEndTypeScans(TRANSFERTASK_NUMVOLUMES);
                phaseTransferTask.setAutoProcessType(Phase.AutoProcessType.CORRECT_REALIGN_SMOOTH_ROI);
                phaseTransferTask.setAutoProcessWrite(false);
                phases.Add(phaseTransferTask);



                Phase phaseTest = new Phase((int)PhaseIds.Test, "Test");
                phaseTest.setSceneImmediate((int)Scenes.Feedback_Run1);
                phaseTest.collect = false;
                phaseTest.requiresMatlab = false;
                phaseTest.setEndTypeNone();
                phases.Add(phaseTest);




                //
                // phase forwarding
                //


                // Localizer / pretest task - Task instruction               ->     Localizer / pretest task - Acquisition
                phaseLocalizerInstr.nextPhase = phaseLocalizerAcq;

                // Localizer / pretest task - Acquisition                    ->     Localizer / pretest task - GLM
                phaseLocalizerAcq.nextPhase = phaseLocalizerGLM;

                // Localizer / pretest task - GLM                            ->     Localizer / pretest task - ROI mask
                phaseLocalizerGLM.nextPhase = phaseLocalizerRoiMask;

                // Localizer / pretest task - ROI mask                       ->     Neurofeedback Run 1 - Task instruction / break
                //phaseLocalizerRoiMask.nextPhase = phaseFeedbackRun1Instr;
            
                

                // Neurofeedback Run 1 - Task instruction / break              ->     Neurofeedback Run 1
                phaseFeedbackRun1Instr.nextPhase = phaseFeedbackRun1;

                // Neurofeedback Run 2 - Task instruction / break              ->     Neurofeedback Run 2
                phaseFeedbackRun2Instr.nextPhase = phaseFeedbackRun2;

                // Neurofeedback Run 3 - Task instruction / break              ->     Neurofeedback Run 3
                phaseFeedbackRun3Instr.nextPhase = phaseFeedbackRun3;


                // Tranfer task - Task instruction / break                      ->     Transfer task
                phaseTransferTaskInstr.nextPhase = phaseTransferTask;


            }

        }


        /**
         * Callback function, called at the start of an experimental phase by Mainthread
         * 
         * Note that this is the last thing called. At this
         * point, the view, the volume collection and trigger catching have already started
         **/
        public void startPhase(ref Phase phase) {

            // write the session data to a file at the beginning of each phase
            Session.saveSession(workDirectory + "SessionData___StartPhase_" + phase.phaseID + "___" + DateHelper.getDateTime() + ".txt");
            Session.saveSessionBinary(workDirectory + "SessionData___StartPhase_" + phase.phaseID + "___" + DateHelper.getDateTime() + ".dat");

            // Note, delibaretely not using switch-case here. As switch-case
            // does not allow the same variable names in each case block
            // (considered a declaration space rather than a scope)

            if (phase.phaseID == (int)PhaseIds.RealtimeCorrectionVolume) {
                // real-time volume correction

                // reset the task
                rtCorrectionVolumeTask.reset();

            } else if (phase.phaseID == (int)PhaseIds.FuncGlobalToNativeMask) {
                // Global to native mask (functional)

                // reset the task
                funcMniToNativeMaskTask.reset();

            } else if (phase.phaseID == (int)PhaseIds.AnatGlobalToNativeMask) {
                // Global to native mask (anatomical)

                // reset the task
                anatMniToNativeMaskTask.reset();

                // run the task
                anatMniToNativeMaskTask.run();

                // check if the phase is finished
                if (anatMniToNativeMaskTask.isTaskFinished()) {

                    // store the resulting native global mask in the session (create a clone of the volume object so nothing in the task gets adjusted)
                    Session.setGlobalMask(anatMniToNativeMaskTask.getOutputMask().clone());

                    // store the resulting RT native global mask in the session (create a clone of the volume object so nothing in the task gets adjusted)
                    Session.setGlobalRTMask(anatMniToNativeMaskTask.getRTOutputMask().clone());

                    // flag the phase as finished
                    phase.tasksFinished = true;

                }

            } else if (phase.phaseID == (int)PhaseIds.RealignmentVolume) {
                // realign volume

                // reset the task
                realignmentVolumeTask.reset();


            } else if (phase.phaseID == (int)PhaseIds.Localizer_Pretest_Acquire) {
                // Localizer / pretest task - Acquisition    

                // clear the feature localizer volumes and trial information from the session
                Session.clearTaskVolumes("LocalizerAcq");
                Session.clearTaskTrials("LocalizerAcqTrials");


            } else if (phase.phaseID == (int)PhaseIds.Localizer_Pretest_GLM) {
                // Localizer / pretest task - Analyze (GLM)
                
                // retrieve the volumes for the task
                List<Volume> locVolumes = Session.getTaskVolumes("LocalizerAcq");
                if (locVolumes == null || locVolumes.Count == 0) {

                    // message and return
                    logger.Error("No (Localizer / pretest task) acquired volumes found, task cannot run");
                    return;

                }

                // create clones of the volumes in the volume list (to prevent any adjustments in the original list)
                bool allRealigned = true;
                List<Volume> lstClone = new List<Volume>(locVolumes.Count);
                for (int i = 0; i < locVolumes.Count; i++) {
                    lstClone.Add(locVolumes[i].clone());
                    if (!locVolumes[i].realigned) {
                        allRealigned = false;
                        break;
                    }
                }
                locVolumes = lstClone;

                // check if the images are realigned
                if (!allRealigned) {

                    // message and return
                    logger.Error("One, more or all of the (Localizer / pretest task) acquired volumes are not realigned, task cannot run");
                    return;

                }

                // correct the volume sequence if needed
                // see if they are sequential. If not sequential: warn; then fill the spot with a clone of the volume before or after
                // TODO: 


                // retrieve the trialList
                List<Block> locTrials = Session.getTaskTrials("LocalizerAcqTrials");
                if (locTrials == null || locTrials.Count == 0) {

                    // message and return
                    logger.Error("No (Localizer / pretest task) trial information found, task cannot run");
                    return;

                }

                // clear the localizer GLM output volumes from the session
                Session.clearTaskVolumes("LocalizerGLM");

                // run the task with the volumes
                localizerGLMTask.run(locVolumes, locTrials, CONFIG_GLM_TR);

                // check if the phase is finished
                if (localizerGLMTask.isTaskFinished()) {

                    // store the resulting glm volumes in the session (create a clone of the volume object so nothing in the task gets adjusted)
                    List<Volume> volumes = localizerGLMTask.getOutputTMapVolumes();
                    for (int i = 0; i < volumes.Count; i++) {
                        Session.addTaskVolume("LocalizerGLM", volumes[i].clone());
                    }

                    // flag the phase as finished
                    phase.tasksFinished = true;

                }


            } else if (phase.phaseID == (int)PhaseIds.Localizer_Pretest_ROIMask) {
                // Localizer / pretest task - ROI mask

                // retrieve the GLM output volumes for the task
                List<Volume> locVolumes = Session.getTaskVolumes("LocalizerGLM");
                if (locVolumes == null || locVolumes.Count == 0) {

                    // message and return
                    logger.Error("No (Localizer / pretest task) glm volumes found, task cannot run");
                    return;

                }

                // create clones of the volumes in the volume list (to prevent any adjustments in the original list)
                List<Volume> lstClone = new List<Volume>(locVolumes.Count);
                for (int i = 0; i < locVolumes.Count; i++)
                    lstClone.Add(locVolumes[i].clone());
                locVolumes = lstClone;

                // retrieve the global mask for the task (optional, can be null)
                Volume globalMaskVolume = Session.getGlobalMask().clone();

                // run the task with the volumes
                localizerROIMaskTask.run(locVolumes, globalMaskVolume);

                // flag the phase as finished
                if (localizerROIMaskTask.isTaskFinished()) {

                    // store the resulting roi mask in the session (create a clone of the volume object so nothing in the task gets adjusted)
                    Session.setRoiMask(localizerROIMaskTask.getOutputMask().clone());

                    // flag the phase as finished
                    phase.tasksFinished = true;

                }

            } else if ( phase.phaseID == (int)PhaseIds.Neurofeedback_Run1 ||
                        phase.phaseID == (int)PhaseIds.Neurofeedback_Run2 ||
                        phase.phaseID == (int)PhaseIds.Neurofeedback_Run3 || 
                        phase.phaseID == (int)PhaseIds.Transfer_Task) {
                // Neurofeedback runs and transfer task

                // clear any of the runs at the start of the phase
                if (phase.phaseID == (int)PhaseIds.Neurofeedback_Run1)      Session.clearTaskVolumes("Neurofeedback1");
                if (phase.phaseID == (int)PhaseIds.Neurofeedback_Run2)      Session.clearTaskVolumes("Neurofeedback2");
                if (phase.phaseID == (int)PhaseIds.Neurofeedback_Run3)      Session.clearTaskVolumes("Neurofeedback3");
                if (phase.phaseID == (int)PhaseIds.Transfer_Task)           Session.clearTaskVolumes("Transfer");

                // run the task with the volumes
                feedbackTask.initialize();
                
            }

        }

        /**
         * Callback function, called when the end of an experimental phase (as defined per configuration) was reached
         * 
         * This function is called first before the task's nextPhase (as defined by the configuration) is
         * initiated and a next phase/view task is started
         **/
        public void stopPhase(ref Phase phase) {

            // Note, delibaretely not using switch-case here. As switch-case
            // does not allow the same variable names in each case block
            // (considered a declaration space rather than a scope)

            if (phase.phaseID == (int)PhaseIds.Localizer_Pretest_Acquire) {
                // Localizer / pretest task - Acquisition

                // try to retrieve the trial sequence
                IViewRF view = MainThread.getView();
                if (view != null) {
                    Block[] trials = view.getTrialSequence();
                    if (trials != null) {
                        List<Block> trialList = new List<Block>(trials);
                        for (int i = 0; i < trialList.Count; i++) trialList[i] = trialList[i].clone();
                        Session.setTaskTrials("LocalizerAcqTrials", trialList);
                    }
                }

            }

            // write the session data to a file at the ending of each phase
            Session.saveSession(workDirectory + "SessionData___StopPhase_" + phase.phaseID + "___" + DateHelper.getDateTime() + ".txt");
            Session.saveSessionBinary(workDirectory + "SessionData___StopPhase_" + phase.phaseID + "___" + DateHelper.getDateTime() + ".dat");

        }

        /**
         * Callback function, called for every new incoming volume
         * 
         * (after the Mainthread has finished auto-processing with it)
         **/
        public void processVolume(ref Phase phase, Volume volume) {

            // Note, delibaretely not using switch-case here. As switch-case
            // does not allow the same variable names in each case block
            // (considered a declaration space rather than a scope)


            if (phase.phaseID == (int)PhaseIds.RealtimeCorrectionVolume) {
                // real-time correction volume

                // process and handle
                rtCorrectionVolumeTask.process(volume);
                if (rtCorrectionVolumeTask.isTaskFinished()) {
                    // task finished

                    // store the resulting correction volume in the session (create a clone of the volume object so nothing in the task gets adjusted)
                    // the NiftiHelper class will use this volume (from the session) to correct the headers on real-time collected images
                    Session.setCorrectionVolume(rtCorrectionVolumeTask.getOutputVolume().clone());

                    // flag the phase as finished
                    phase.tasksFinished = true;

                }


            } else if (phase.phaseID == (int)PhaseIds.FuncGlobalToNativeMask) {
                // Global to native mask (functional)

                // process and handle
                funcMniToNativeMaskTask.process(volume);
                if (funcMniToNativeMaskTask.isTaskFinished()) {
                    // task finished

                    // store the resulting native global mask in the session (create a clone of the volume object so nothing in the task gets adjusted)
                    Session.setGlobalMask(funcMniToNativeMaskTask.getOutputMask().clone());

                    // store the resulting RT native global mask in the session (create a clone of the volume object so nothing in the task gets adjusted)
                    Session.setGlobalRTMask(funcMniToNativeMaskTask.getRTOutputMask().clone());

                    // flag the phase as finished
                    phase.tasksFinished = true;

                }


            } else if (phase.phaseID == (int)PhaseIds.RealignmentVolume) {
                // realignment volume

                // process and handle
                realignmentVolumeTask.process(volume);
                if (realignmentVolumeTask.isTaskFinished()) {
                    // task finished

                    // store the realign volume in the session (create a clone of the volume object so nothing in the task gets adjusted)
                    // the NiftiHelper class will use this volume (from the session) to realign the real-time collected images to
                    Session.setRealignmentVolume(realignmentVolumeTask.getOutputVolume().clone());

                    // flag the phase as finished
                    phase.tasksFinished = true;

                }


            } else if (phase.phaseID == (int)PhaseIds.Localizer_Pretest_Acquire) {
                // Localizer / pretest task - Acquisition

                Session.addTaskVolume("LocalizerAcq", volume);
                

            } else if ( phase.phaseID == (int)PhaseIds.Neurofeedback_Run1 ||
                        phase.phaseID == (int)PhaseIds.Neurofeedback_Run2 ||
                        phase.phaseID == (int)PhaseIds.Neurofeedback_Run3 ||
                        phase.phaseID == (int)PhaseIds.Transfer_Task) {
                // Neurofeedback runs and transfer task

                // store the volume in the session
                if (phase.phaseID == (int)PhaseIds.Neurofeedback_Run1)      Session.addTaskVolume("Neurofeedback1", volume);
                if (phase.phaseID == (int)PhaseIds.Neurofeedback_Run2)      Session.addTaskVolume("Neurofeedback2", volume);
                if (phase.phaseID == (int)PhaseIds.Neurofeedback_Run3)      Session.addTaskVolume("Neurofeedback3", volume);
                if (phase.phaseID == (int)PhaseIds.Transfer_Task)           Session.addTaskVolume("Transfer", volume);

                // process the scan
                feedbackTask.processVolume(volume);
                
            }


        }


        /**
         * Callback function, called for every incoming trigger
         * 
         * (after the Mainthread is finished with it)
         **/
        public void processTrigger(ref Phase phase, TriggerEventArgs e) {
            //logger.Debug("Experiment processTrigger {0} {1}", e.value, e.datetime);


        }

        /**
         * Callback function, called at the start of each trial in the view
         * 
         * Initiated by the View, forwarded to the MainThread, which forwards it here
         **/
        public void processViewTrialStart(ref Phase phase, double condition) {
            feedbackTask.processTrialStart(condition);
        }

        /**
         * Get the most recent feedback value from the feedback task
         * 
         * Most likely to be requested by the view from the mainthread, which in it's turn
         * requests it from the experiment (here).
         **/
        public double getFeedbackValue(ref Phase phase) {
            
            // select which phase
            if (phase.phaseID == (int)PhaseIds.Neurofeedback_Run1 ||
                phase.phaseID == (int)PhaseIds.Neurofeedback_Run2 ||
                phase.phaseID == (int)PhaseIds.Neurofeedback_Run3 ||
                phase.phaseID == (int)PhaseIds.Transfer_Task) {
                // Neurofeedback runs and transfer task

                // check if the neurofeedback task exists and is initialized
                if (feedbackTask != null && feedbackTask.isInitialized()) {
                    
                    // retrieve the trial classification value
                    return feedbackTask.getFeedbackValue();
                    
                }

            }
            
            return 0;

        }

        /**
         * Get the most recent feedback values from the feedback task
         * 
         * Most likely to be requested by the view from the mainthread, which in it's turn
         * requests it from the experiment (here).
         **/
        public double[] getFeedbackValues(ref Phase phase) {

            // select which phase
            if (phase.phaseID == (int)PhaseIds.Neurofeedback_Run1 ||
                phase.phaseID == (int)PhaseIds.Neurofeedback_Run2 ||
                phase.phaseID == (int)PhaseIds.Neurofeedback_Run3 ||
                phase.phaseID == (int)PhaseIds.Transfer_Task) {
                // Neurofeedback runs and transfer task

                // check if the neurofeedback task exists and is initialized
                if (feedbackTask != null && feedbackTask.isInitialized()) {

                    // retrieve the trial classification value
                    return feedbackTask.getFeedbackValues();

                }

            }

            return new double[0];

        }


    }

}
