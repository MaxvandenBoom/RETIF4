/**
 * ApplePickExperiment class
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
using System.IO;
using Scenes = RETIF4.Views.ApplePickView.Scenes;

namespace RETIF4.Experiment {
    
    public class ApplePickExperiment : ExperimentBase, IExperiment {
        
        private static Logger logger = LogManager.GetLogger("ApplePickExperiment");

        private new const string CONFIG_VIEW_CLASS = "RETIF4.Views.ApplePickView";
        private new const string CONFIG_COLLECTOR_CLASS = "RETIF4.Collectors.CollectorPhilips7TUMCU";
        private new const string CONFIG_TRIGGER_CLASS = "RETIF4.Triggers.SerialTrigger";

        private const string CONFIG_GLOBAL_MASK_FILE = "MNI_MASK_DLPFC.nii";
        private const string CONFIG_EPI_MNI_FILE = "MNI_EPI_TEMPLATE_TOTAL.nii";
        
        private string workDirectory = "C:\\P33\\";

        // tasks
        private FuncMniToNativeMaskTask mniToNativeMaskTask = null;


        public void init() {
            
            // thread safety
            lock (configurationLock) {

                // set the view, collector and trigger class
                base.CONFIG_VIEW_CLASS = CONFIG_VIEW_CLASS;
                base.CONFIG_COLLECTOR_CLASS = CONFIG_COLLECTOR_CLASS;
                base.CONFIG_TRIGGER_CLASS = CONFIG_TRIGGER_CLASS;
                
                // settings the program's behaviour
                MainThread.setMatlabStartupOnStartProgram(false);       // star t view at the start of the application
                MatlabWrapper.setSendCommandMethod(1);                  // set the methmatlab at the start of the application (only when one or more experiment phases has the 'requiresMatlab' flag on true)
                MainThread.setViewStartupOnStartProgram(false);         // start theod nstance command window

                //used to send commands to 1 = using sending text to the command window (gives feedback in the command window/console)
                MatlabWrapper.setShowCommandWindow(true);               // show the matlab i
                // create tasks
                //
                mniToNativeMaskTask = new FuncMniToNativeMaskTask(  workDirectory, 
                                                                (IOHelper.getProgramDirectory() + "templates" + Path.DirectorySeparatorChar + CONFIG_EPI_MNI_FILE),
                                                                (IOHelper.getProgramDirectory() + "templates" + Path.DirectorySeparatorChar + CONFIG_GLOBAL_MASK_FILE),
                                                                0);

                // 
                // define phases
                // 
                /*
                 * 
                    '0. No phase / Program wait / Black screen', ...
                    '3. Scanner setup *', ...
                    '7. Localizer mask (scanner) *', ...
                    '10. Localizer ROI (scanner) / Network activation task (pre)', ...
                    '13. Localizer ROI (process) *', ...
                    '15. Pre-training rest (distractor first) *', ...
                    '16. Training task with distractor (first)', ...
                    '18. Anatomy *', ...
                    '23. Threshold instruction *', ...
                    '25. Threshold task', ...
                    '28. Pre-training rest (run 1) *', ...
                    '30. Training task (run 1)', ...
                    '32. Pre-training rest (run 2) *', ...
                    '35. Training task (run 2)', ...
                    '38. Pre-training rest (run 3) *', ...
                    '40. Training task (run 3)', ...
                    '42. Pre-training rest (run 4) *', ...
                    '45. Training task (run 4)', ...
                    '48. Pre-training rest (run 5) *', ...
                    '50. Training task (run 5)', ...
                    '52. Pre-training rest (run 6) *', ...
                    '55. Training task (run 6)', ...
                    '58. Pre-training rest (run 7) *', ...
                    '60. Training task (run 7)', ...
                    '70. Long rest *', ...
                    '73. Network activation task instruction *', ...
                    '75. Network activation task (post)', ...
                    '80. Threshold pre-data switch *', ...
                    '85. Pre-training rest (distractor post) *', ...
                    '90. Training task with distractor (post)', ...
                    '99. Session done'});
                 * 
                        if phase == 16, obj.collector.start('training_distfirst'); end;
                        if phase == 30, obj.collector.start('training1'); end;
                        if phase == 35, obj.collector.start('training2'); end;
                        if phase == 40, obj.collector.start('training3'); end;
                        if phase == 45, obj.collector.start('training4'); end;
                        if phase == 50, obj.collector.start('training5'); end;
                        if phase == 55, obj.collector.start('training6'); end;
                        if phase == 60, obj.collector.start('training7'); end;
                        if phase == 90, obj.collector.start('training_distpost'); end;
                 * 
                        if phase == 15, obj.collector.start('pretrainingrest_distrfirst'); end
                        if phase == 28, obj.collector.start('pretrainingrest1'); end;
                        if phase == 32, obj.collector.start('pretrainingrest2'); end;
                        if phase == 38, obj.collector.start('pretrainingrest3'); end;
                        if phase == 42, obj.collector.start('pretrainingrest4'); end;
                        if phase == 48, obj.collector.start('pretrainingrest5'); end;
                        if phase == 52, obj.collector.start('pretrainingrest6'); end;
                        if phase == 58, obj.collector.start('pretrainingrest7'); end;
                        if phase == 85, obj.collector.start('pretrainingrest_distrpost'); end;
                 * 
                 * 25 threshold
                 * 13 t('localizer_process');
                 * 10 obj.collector.start('localizer_acq');
                 * 7 obj.collector.start('localizer_mask');
                 */

                Phase phase;
                phase = new Phase(0, "No phase / Program wait / Black screen");
                phase.setSceneImmediate((int)Scenes.BlackScreen);
                phase.collect = false;
                phases.Add(phase);

                phase = new Phase(1, "Calibrate view / background only");
                phase.setSceneImmediate((int)Scenes.BackgroundOnly);
                phase.collect = false;
                phases.Add(phase);

                phase = new Phase(3, "Scanner setup *");
                phase.setSceneImmediate((int)Scenes.BlackScreen);
                phase.collect = false;
                phases.Add(phase);

                phase = new Phase(7, "Global to native mask (scanner) *");
                phase.setSceneImmediate((int)Scenes.BlackScreen);
                phase.collect = true;
                phase.collectionName = "global_to_native_mask";
                phase.requiresMatlab = true;
                phases.Add(phase);

                phase = new Phase(10, "Feature localizer / Classifier trainer / VMI task (pre) (scanner) *");
                phase.setSceneAtIncomingScan((int)Scenes.BackgroundOnly, -1);
                phase.collect = true;
                phase.collectionName = "feature_localizer";
                phase.requiresMatlab = true;
                phases.Add(phase);






                phase = new Phase(30, "Training task (run 1)");
                phase.setSceneImmediate((int)Scenes.Feedback);
                phase.collect = true;
                phase.collectionName = "training1";
                phases.Add(phase);

                phase = new Phase(34, "Matlab Test");
                phase.setSceneImmediate((int)Scenes.BlackScreen);
                phase.collect = false;
                phase.collectionName = "matlabtest";
                phase.requiresMatlab = true;
                phases.Add(phase);




                getPhaseById(7).nextPhase = getPhaseById(10);

            }

        }

        public string getWorkDirectory() {
            return workDirectory;
        }

        // Called with every start of a phase (after the Mainthread has finished starting it)
        // (use 'processPhase' if the processing does not rely on volumes coming in at the current phase)
        public void startPhase(ref Phase phase) {

        }

        public void stopPhase(ref Phase phase) {

        }

        // called with every new volume (after the Mainthread has finished with it)
        public void processVolume(ref Phase phase, Volume volume) {

            // Localizer mask
            if (phase.phaseID == 7) {

                // process and handle
                mniToNativeMaskTask.process(volume);
                if (mniToNativeMaskTask.isTaskFinished()) {
                    // task finished

                    // store the resulting native global mask in the session (create a clone of the volume object so nothing in the task gets adjusted)
                    Session.setGlobalMask(mniToNativeMaskTask.getOutputMask().clone());

                    if (phase.nextPhase != null) {

                        // 
                        //MainThread.start()
                        //getPhaseById(9);


                    }



                }

            }


            //NiftiImage a = NiftiDLL.n_ReadNifti_Safe(volume.filePath);
            //logger.Error("read");

        }

        // called with every trigger (after the Mainthread has finished with it)
        public void processTrigger(ref Phase phase, TriggerEventArgs e) {
            //logger.Debug("Experiment processTrigger {0} {1}", e.value, e.datetime);



        }

        /**
         * Callback function, called at the start of each trial in the view
         * 
         * Initiated by the View, forwarded to the MainThread, which forwards it here
         **/
        public void processViewTrialStart(ref Phase phase, double condition) {

        }

        /**
         * Get the most recent feedback value from the feedback task
         * 
         * Most likely to be requested by the view from the mainthread, which in it's turn
         * requests it from the experiment (here).
         **/
        public double getFeedbackValue(ref Phase phase) {
            return 1;
        }

        /**
         * Get the most recent feedback values from the feedback task
         * 
         * Most likely to be requested by the view from the mainthread, which in it's turn
         * requests it from the experiment (here).
         **/
        public double[] getFeedbackValues(ref Phase phase) {
            return new double[0];
        }

    }

}
