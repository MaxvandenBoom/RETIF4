/**
 * SimpleGLMTask class
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
using RETIF4.Helpers;
using RETIF4.Matlab;
using System;
using System.Collections.Generic;
using System.IO;

namespace RETIF4.Tasks {

    public class SimpleGLMTask : TaskBase, ITask {

        private static Logger logger = LogManager.GetLogger("SimpleGLMTask");

        private string currentGLMDirectory = null;                          // the working directory used for the current task (will be set while processing)
        private List<Volume> currentOutputTMapVolumes = null;               // the native mask volume as output for the current task (will be set while processing)


        public SimpleGLMTask(string workDirectory) : base(workDirectory) {
            
        }

        public void run(List<Volume> inputVolumes, List<Block> inputTrials, double inputTR) {

            // flag the task as not finished
            finished = false;

            // check if there are volumes
            if (inputVolumes.Count == 0) {

                // message
                logger.Error("No input volumes");

                // return
                return;

            }

            // check if there are trials
            if (inputTrials.Count == 0) {

                // message
                logger.Error("No input trial information");

                // return
                return;

            }


            // 
            System.Globalization.CultureInfo us = new System.Globalization.CultureInfo("en-US");
            string symbol_0_onsets = "[";
            string symbol_1_onsets = "[";
            string symbol_2_onsets = "[";
            for (int i = 0; i < inputTrials.Count; i++) {
                if (inputTrials[i].condition == 0) {
                    if (symbol_0_onsets.Length != 1) {
                        symbol_0_onsets += ",";
                    }
                    symbol_0_onsets += String.Format(us, "{0:0.000}", inputTrials[i].onset);
                }
                if (inputTrials[i].condition == 1) {
                    if (symbol_1_onsets.Length != 1) {
                        symbol_1_onsets += ",";
                    }
                    symbol_1_onsets += String.Format(us, "{0:0.000}", inputTrials[i].onset);
                }
                if (inputTrials[i].condition == 2) {
                    if (symbol_2_onsets.Length != 1) {
                        symbol_2_onsets += ",";
                    }
                    symbol_2_onsets += String.Format(us, "{0:0.000}", inputTrials[i].onset);
                }
            }
            symbol_0_onsets += "]";
            symbol_1_onsets += "]";
            symbol_2_onsets += "]";
            logger.Error("symbol_0_onsets: " + symbol_0_onsets);
            logger.Error("symbol_1_onsets: " + symbol_1_onsets);
            logger.Error("symbol_2_onsets: " + symbol_2_onsets);




            //
            // file management
            //

            // build a new folder for the current task and create the folder
            currentGLMDirectory = outputDirectory + Path.DirectorySeparatorChar + "localizer__glm__" + DateHelper.getDateTime();
            Directory.CreateDirectory(currentGLMDirectory);



            //
            // matlab calls
            //
            
            // transfer the volumes filenames to matlab
            NiftiHelper.transferFileListToMatlab(inputVolumes);

            MatlabWrapper.sendCommand("clear matlabbatch;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.dir = {'" + currentGLMDirectory + "'};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.timing.units = 'secs';");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.timing.RT = " + String.Format(us, "{0:0.000}", inputTR) + ";");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.timing.fmri_t = 16;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.timing.fmri_t0 = 8;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.scans = files;");

            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).name = 'act';");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).onset = " + symbol_2_onsets + ";");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).duration = [16];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).tmod = 0;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).pmod = struct('name', {}, 'param', {}, 'poly', {});");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).orth = 1;");
            
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.multi = {''};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.regress = struct('name', {}, 'val', {});");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.multi_reg = {''};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.hpf = 128;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.fact = struct('name', {}, 'levels', {});");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.bases.hrf.derivs = [0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.volt = 1;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.global = 'None';");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.mthresh = 0.3;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.mask = {''};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.cvi = 'AR(1)';");
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.stats.fmri_est.spmmat(1) = cfg_dep('fMRI model specification: SPM.mat File', substruct('.','val', '{}',{ 1}, '.','val', '{}',{1}, '.','val', '{}',{1}), substruct('.','spmmat'));");
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.stats.fmri_est.write_residuals = 0;");
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.stats.fmri_est.method.Classical = 1;");
            MatlabWrapper.sendCommand("matlabbatch{3}.spm.stats.con.spmmat(1) = cfg_dep('Model estimation: SPM.mat File', substruct('.','val', '{}',{ 2}, '.','val', '{}',{1}, '.','val', '{}',{1}), substruct('.','spmmat'));");
            MatlabWrapper.sendCommand("matlabbatch{3}.spm.stats.con.consess{1}.tcon.name = 'act';");
            MatlabWrapper.sendCommand("matlabbatch{3}.spm.stats.con.consess{1}.tcon.weights = [1 0];");
            MatlabWrapper.sendCommand("matlabbatch{3}.spm.stats.con.consess{1}.tcon.sessrep = 'none';");
            MatlabWrapper.sendCommand("matlabbatch{3}.spm.stats.con.delete = 0;");

            // run the (est+write) align
            MatlabWrapper.sendCommand("spm_jobman('run', matlabbatch); ");

            // store the resulting contrast t-maps
            try {

                string[] tmaps = Directory.GetFiles(currentGLMDirectory, "spmT*.nii");
                currentOutputTMapVolumes = new List<Volume>(tmaps.Length);
                for (int i = 0; i < tmaps.Length; i++) {
                    Volume volume = new Volume(Volume.VolumeSource.Generated);
                    volume.filepath = tmaps[i];

                    // base the correction/realignment properties
                    volume.headerCorrection = inputVolumes[0].headerCorrection;
                    volume.orientDataCorrection = inputVolumes[0].orientDataCorrection;
                    volume.revOrientDataCorrection = inputVolumes[0].revOrientDataCorrection;
                    volume.realigned = inputVolumes[0].realigned;

                    currentOutputTMapVolumes.Add(volume);
                }

            } catch (Exception) { }

            // message
            logger.Info("finished: " + currentGLMDirectory);

            // flag the task as finished
            finished = true;
            
        }

        public List<Volume> getOutputTMapVolumes() {

            // check if the task has not finished yet
            if (!finished) {

                logger.Error("Trying to retrieve the GLM t-maps before the task has finished, returning null");

                // return failure
                return null;

            }

            // return the tmap output volumes
            return currentOutputTMapVolumes;
        }
        

    }

}
