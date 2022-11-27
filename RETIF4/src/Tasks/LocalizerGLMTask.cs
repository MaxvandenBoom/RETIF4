/**
 * LocalizerGLMTask class
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
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace RETIF4.Tasks {

    /// <summary>  
	/// This task collects a number of input volumes and performs a GLM. The output can be used later - for example in the 'LocalizerROIMaskTask' task 
	/// to make a more precise voxels selecion (within the ROI) for feedback. Corrected, reoriented, realigned and smoothed images are expected to come in. 
    /// </summary> 
    public class LocalizerGLMTask : TaskBase, ITask {

        private static Logger logger = LogManager.GetLogger("LocalizerGLMTask");

        private string currentGLMDirectory = null;                          // the working directory used for the current task (will be set while processing)
        private List<Volume> currentOutputTMapVolumes = null;               // the native mask volume as output for the current task (will be set while processing)


        public LocalizerGLMTask(string workDirectory) : base(workDirectory) {
            
        }

        public void run(List<Volume> inputVolumes, List<Block> inputTrials) {

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
            //logger.Debug("symbol_0_onsets: " + symbol_0_onsets);
            //logger.Debug("symbol_1_onsets: " + symbol_1_onsets);
            //logger.Debug("symbol_2_onsets: " + symbol_2_onsets);




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
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.timing.RT = 1.5;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.timing.fmri_t = 16;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.timing.fmri_t0 = 8;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.scans = files;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).name = 'x';");

            // production fixed
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).onset = [12,29.1,148.2,233.5,250.5,301.6,386.8,437.9,472,489,506,540.1,625.3,659.4,761.6];");

            // debug: carlijn testdata
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).onset = [0,75.1,90.1,165.2,180.3,210.3,240.3,255.3,405.5,480.6];");
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).onset = [0.0480000000000000,75.0970000000000,90.1140000000000,165.196000000000,180.213000000000,210.246000000000,240.279000000000,255.296000000000,405.461000000000,480.543000000000];");

            // debug: P09 testdata perc
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).onset = [30.10,58.10,107.2,215.5,248.5,317.7,335.7,368.7,456,471,484,525.1,571.2];");
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).onset = [26.034,50.067,93.117,187.233,216.266,277.332,293.348,322.381,397.480,410.497,421.514,456.563,496.613,574.712,587.728];");

            // debug: P09 testdata imag1
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).onset = [19.025,191.189,227.222,278.272,297.288,312.305,378.371,480.47,499.486,514.489];");

            // production
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).onset = " + symbol_0_onsets + ";");

            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).duration = [0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).tmod = 0;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).pmod = struct('name', {}, 'param', {}, 'poly', {});");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(1).orth = 1;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(2).name = '+';");

            // production fixed
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(2).onset = [46.1,63.1,97.2,114.2,165.3,216.4,318.7,335.7,369.8,523,557.1,591.2,676.4,693.5,710.5];");

            // debug: carlijn testdata
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(2).onset = [15,45.1,120.2,135.2,150.2,195.3,285.4,315.4,375.5,390.5,420.5,450.6];");
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(2).onset = [15.0310000000000,45.0640000000000,120.147000000000,135.163000000000,150.180000000000,195.229000000000,285.329000000000,315.362000000000,375.428000000000,390.444000000000,420.477000000000,450.510000000000];");

            // debug: P09 testdata perc
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(2).onset = [15,94.20,158.3,202.4,266.6,284.6,353.7,381.8,399.8,414.9,429.9,442.9,510.1,538.2,584.2];");
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(2).onset = [13.018,82.10,138.167,176.216,232.282,248.299,309.365,333.398,349.414,362.431,375.447,386.464,443.547,467.580,507.629];");

            // debug: P09 testdata imag1
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(2).onset = [0.025,34.041,70.074,87.091,123.124,140.14,155.157,208.206,261.255,416.404,431.42];");

            // production
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(2).onset = " + symbol_1_onsets + ";");

            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(2).duration = [0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(2).tmod = 0;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(2).pmod = struct('name', {}, 'param', {}, 'poly', {});");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(2).orth = 1;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(3).name = 'o';");

            // production fixed
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(3).onset = [80.2,131.2,182.3,199.4,267.6,284.6,352.7,403.8,420.9,454.9,574.2,608.3,642.3,727.6,744.6];");

            // debug: carlijn testdata
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(3).onset = [30.1,60.1,105.2,225.3,270.4,300.4,330.4,345.5,360.5,435.6,465.6,495.6];");
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(3).onset = [30.0480000000000,60.0810000000000,105.130000000000,225.262000000000,270.312000000000,300.345000000000,330.378000000000,345.395000000000,360.411000000000,435.494000000000,465.527000000000,495.560000000000];");

            // debug: P09 testdata perc
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(3).onset = [0,45.10,76.20,125.3,143.3,171.4,189.4,230.5,302.6,497.1,556.2];");
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(3).onset = [0.011,39.0510,66.084,109.134,125.150,149.183,165.2,200.249,264.315,432.530,483.596,523.646,536.662,552.679,563.695];");

            // debug: P09 testdata imag1
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(3).onset = [51.058,106.107,174.173,244.239,329.321,344.338,359.354,397.387,448.437,465.453,606.783];");
            //MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(3).onset = [51.058,106.107,174.173,244.239,329.321,344.338,359.354,397.387,448.437,465.453];");

            // production
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(3).onset = " + symbol_2_onsets + ";");


            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(3).duration = [0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(3).tmod = 0;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(3).pmod = struct('name', {}, 'param', {}, 'poly', {});");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.stats.fmri_spec.sess.cond(3).orth = 1;");
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
            MatlabWrapper.sendCommand("matlabbatch{3}.spm.stats.con.consess{1}.tcon.name = 'x';");
            MatlabWrapper.sendCommand("matlabbatch{3}.spm.stats.con.consess{1}.tcon.weights = [1 0 0];");
            MatlabWrapper.sendCommand("matlabbatch{3}.spm.stats.con.consess{1}.tcon.sessrep = 'none';");
            MatlabWrapper.sendCommand("matlabbatch{3}.spm.stats.con.consess{2}.tcon.name = '+';");
            MatlabWrapper.sendCommand("matlabbatch{3}.spm.stats.con.consess{2}.tcon.weights = [0 1 0];");
            MatlabWrapper.sendCommand("matlabbatch{3}.spm.stats.con.consess{2}.tcon.sessrep = 'none';");
            MatlabWrapper.sendCommand("matlabbatch{3}.spm.stats.con.consess{3}.tcon.name = 'o';");
            MatlabWrapper.sendCommand("matlabbatch{3}.spm.stats.con.consess{3}.tcon.weights = [0 0 1];");
            MatlabWrapper.sendCommand("matlabbatch{3}.spm.stats.con.consess{3}.tcon.sessrep = 'none';");
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
