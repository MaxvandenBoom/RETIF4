/**
 * FuncMniToNativeMaskTask class
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
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace RETIF4.Tasks {

    /// <summary>  
	/// This task will wait for one functional scan volume from the scanner (of which the header and orientation should already be corrected due
	/// to the RTVolumeCorrectionTask). Then, by calling the SPM normalization step, the incoming function volume (in native space) will 
	/// be "coregistered" to a functional MNI brain (in MNI space). The result is a transformation matrix (and it's inverse), which from this point
    /// on can be used to go from native space to MNI or vise versa. This step can be used to subsequently quickly transform a ROI mask to the
	///	subject native space. The 'AnatMniToNativeMaskTask' will provide the same but based on the anatomical scan.
    /// </summary>  
    public class FuncMniToNativeMaskTask : TaskBase, ITask {

        private static Logger logger = LogManager.GetLogger("FuncMniToNativeMaskTask");

        private const string STD_COPY_PREFIX = "cpy_";
        private const string STD_NATIVE_INPUT_COPY_FILENAME = STD_COPY_PREFIX + "native_scan_epi";
        private const string STD_MNI_TEMPLATE_COPY_FILENAME = STD_COPY_PREFIX + "mni_template_epi";
        private const string STD_MNI_GLOBALMASK_COPY_FILENAME = STD_COPY_PREFIX + "mni_globalmask";

        private int numVolToDiscardAtStartup = 0;                   // the number of volumes to discard before picking the input volume
        private string epiMniTemplateFilepath = null;               // the filepath to an epi template in MNI space which is used to find the mni to native transformation (will be set by the constructor)
        private string globalMaskFilepath = null;                   // the filepath to the global MNI mask that should be converted to a native mask (will be set by the constructor)

        private string currentMaskDirectory = null;                 // the working directory used for the current task (will be set while processing)
        private string currentEpiMniTemplateFile = null;            // the mni epi file used for the current task to find the mni to native transformation (will be set while processing)
        private string currentMNIGlobalMaskFile = null;             // the mni global mask file used for the current task (will be set while processing)
        private string currentNativeGlobalMaskFile = null;          // the (resliced) native global mask file used for the current task (will be set while processing)
        private string currentRTNativeGlobalMaskFile = null;        // the (resliced) native global mask in RT image orientation file used for the current task (will be set while processing)

        private Volume currentInputVolume = null;                   // the native epi volume used as input for the current task (will be set while processing)
        private Volume currentOutputVolume = null;                  // the native mask volume as output for the current task (will be set while processing)
        private Volume currentRTOutputVolume = null;                // the RT native mask volume as output for the current task (will be set while processing)

        private int volumeCounter = 0;
        
        public FuncMniToNativeMaskTask(string workDirectory, string epiMniTemplateFilepath, string globalMaskFilepath, int numVolToDiscardAtStartup) : base(workDirectory) {


            //
            // epi MNI template
            //

            // check if the EPI MNI template file exists
            if (File.Exists(epiMniTemplateFilepath)) {

                // check if the EPI MNI template file does not have the same filename as the copy would have (reserved filename)
                if (string.Compare(Path.GetFileNameWithoutExtension(epiMniTemplateFilepath), STD_MNI_TEMPLATE_COPY_FILENAME) != 0) {

                    // message
                    logger.Info("EPI MNI template filepath set to \'" + epiMniTemplateFilepath + "\'");

                    // set the path
                    this.epiMniTemplateFilepath = epiMniTemplateFilepath;

                } else {

                    // message
                    logger.Error("Invalid EPI MNI template filename ('" + Path.GetFileNameWithoutExtension(epiMniTemplateFilepath) + "'). This filename is reserved for the copy that will be made, please rename the template file");
                    this.epiMniTemplateFilepath = null;

                }

            } else {

                // message
                logger.Error("Invalid (or empty) EPI MNI template filepath ('" + epiMniTemplateFilepath + "')");
                this.epiMniTemplateFilepath = null;

            }


            //
            // global MNI mask
            //

            // check if the global MNI mask file exists
            if (File.Exists(globalMaskFilepath)) {
                
                // check if the global mni mask file does not have the same filename as the copy would have (reserved filename)
                if (string.Compare(Path.GetFileNameWithoutExtension(globalMaskFilepath), STD_MNI_GLOBALMASK_COPY_FILENAME, true) != 0) {

                    // message
                    logger.Info("Global MNI mask filepath set to \'" + globalMaskFilepath + "\'");

                    // set the path
                    this.globalMaskFilepath = globalMaskFilepath;

                } else {
                    
                    // message
                    logger.Error("Invalid global MNI mask filename ('" + Path.GetFileNameWithoutExtension(globalMaskFilepath) + "'). This filename is reserved for the copy that will be made, please rename the mask file");
                    this.globalMaskFilepath = null;

                }

            } else {

                // message
                logger.Error("Invalid (or empty) global MNI filepath ('" + globalMaskFilepath + "')");
                this.globalMaskFilepath = null;

            }

            // store the number of volumes to discard before picking the input volume to use
            this.numVolToDiscardAtStartup = numVolToDiscardAtStartup;

        }

        public void process(Volume volume) {

            // check if the task is not yet finished and we do not have a volume yet
            if (!finished && currentInputVolume == null) {

                // check if this is the volume we want to use
                if (volumeCounter == numVolToDiscardAtStartup) {

                    // store the volume we will use for input
                    currentInputVolume = volume.clone();

                    // execute the task on this volume
                    run();

                } else {
                    // discarding volumes

                    // message
                    logger.Info("Discarding first volumes (" + (volumeCounter + 1) + " of " + numVolToDiscardAtStartup + ")");

                }

            }

            // add another volume
            volumeCounter++;

        }

        private void run() {


            //
            // file management
            //

            // build a new folder for the current task and create the folder
            currentMaskDirectory = outputDirectory + Path.DirectorySeparatorChar + "globalmask__mni_to_native__func__" + DateHelper.getDateTime();
            Directory.CreateDirectory(currentMaskDirectory);

            // copy the input volume files to the masks directory
            if (!NiftiHelper.copyNiftiFilesAndRename(currentInputVolume.filepath, currentMaskDirectory + Path.DirectorySeparatorChar + STD_NATIVE_INPUT_COPY_FILENAME)) {
                // error while creating copies

                // message and return
                logger.Error("Could not copy the scan input file ('" + currentInputVolume.filepath + "') to the current mask directory ('" + currentMaskDirectory + "')");
                return;

            }

            // set the filepath to the copy
            currentInputVolume.filepath = currentMaskDirectory + Path.DirectorySeparatorChar + STD_NATIVE_INPUT_COPY_FILENAME;
            currentInputVolume.filepath += NiftiHelper.getNiftiSetExtension(currentInputVolume.filepath);

            // message
            logger.Info("Copied input volume, using '" + currentInputVolume.filepath + "' as native input");

            // copy the epi MNI template file to the masks directory
            if (!NiftiHelper.copyNiftiFilesAndRename(epiMniTemplateFilepath, currentMaskDirectory + Path.DirectorySeparatorChar + STD_MNI_TEMPLATE_COPY_FILENAME)) {
                // error while creating copies

                // message and return
                logger.Error("Could not copy the epi MNI template file ('" + epiMniTemplateFilepath + "') to the current mask directory ('" + currentMaskDirectory + "')");
                return;

            }

            // set the filepath to the copy
            currentEpiMniTemplateFile = currentMaskDirectory + Path.DirectorySeparatorChar + STD_MNI_TEMPLATE_COPY_FILENAME;
            currentEpiMniTemplateFile += NiftiHelper.getNiftiSetExtension(currentEpiMniTemplateFile);

            // message
            logger.Info("Copied epi MNI template, using '" + currentEpiMniTemplateFile + "' as template");




            // copy the MNI global mask file to the masks directory
            if (!NiftiHelper.copyNiftiFilesAndRename(globalMaskFilepath, currentMaskDirectory + Path.DirectorySeparatorChar + STD_MNI_GLOBALMASK_COPY_FILENAME)) {
                // error while creating copies

                // message and return
                logger.Error("Could not copy the global MNI mask file ('" + epiMniTemplateFilepath + "') to the current mask directory ('" + currentMaskDirectory + "')");
                return;

            }

            // set the filepath to the copy
            currentMNIGlobalMaskFile = currentMaskDirectory + Path.DirectorySeparatorChar + STD_MNI_GLOBALMASK_COPY_FILENAME;
            currentMNIGlobalMaskFile += NiftiHelper.getNiftiSetExtension(currentMNIGlobalMaskFile);

            // message
            logger.Info("Copied global MNI mask, using '" + currentMNIGlobalMaskFile + "' as mask");



            //
            // matlab calls
            //

            MatlabWrapper.sendCommand("spm('defaults', 'fmri');");
            MatlabWrapper.sendCommand("spm_jobman('initcfg');");
            
            MatlabWrapper.sendCommand("clear matlabbatch;");

            // Estimate normalize the MNI EPI (source) to the native EPI (template) and retrieve the transformation matrix as a variable
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.tools.oldnorm.est.subj.source = {'" + currentEpiMniTemplateFile + ",1'};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.tools.oldnorm.est.subj.wtsrc = '';");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.tools.oldnorm.est.eoptions.template = {'" + currentInputVolume.filepath + ",1'};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.tools.oldnorm.est.eoptions.weight = '';");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.tools.oldnorm.est.eoptions.smosrc = 0;");                     // normalize est; source image smoothing (set to 0, this is here set to the epi template image, which is already smoothed with 8)
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.tools.oldnorm.est.eoptions.smoref = 8;");                     // normalize est; reference image smoothing (set to 8, this is the input epi that will be smoothed. the SPM EPI MNI template image is also smoothed with 8)
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.tools.oldnorm.est.eoptions.regtype = 'mni';");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.tools.oldnorm.est.eoptions.cutoff = 25;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.tools.oldnorm.est.eoptions.nits = 16;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.tools.oldnorm.est.eoptions.reg = 1;");

            // Write normalize, apply the "MNI to native" transformation matrix to the MNI global mask to convert the DLPFC mask into native space
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.tools.oldnorm.write.subj.matname(1) = cfg_dep('Old Normalise: Estimate: Norm Params File (Subj 1)', substruct('.', 'val', '{}',{ 1}, '.','val', '{}',{ 1}, '.','val', '{}',{ 1}, '.','val', '{}',{ 1}), substruct('()',{ 1}, '.','params'));");
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.tools.oldnorm.write.subj.resample = {'" + currentMNIGlobalMaskFile + ",1'};");
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.tools.oldnorm.write.roptions.preserve = 0;");                  // normalize write; preserve concentrations
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.tools.oldnorm.write.roptions.bb = [-100,-140,-100;110,120,110];");
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.tools.oldnorm.write.roptions.vox = [2 2 2];");
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.tools.oldnorm.write.roptions.interp = 0;");                    // normalize write; nearest neighbour interpolation
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.tools.oldnorm.write.roptions.wrap = [0 0 0];");
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.tools.oldnorm.write.roptions.prefix = 'native_';");

            // run the (est+write) normalize
            MatlabWrapper.sendCommand("spm_jobman('run', matlabbatch); ");
            Thread.Sleep(100);  // allow jobman to finish writing
            
            // rename the MNI to Naive transformation matrix file
            File.Move(  currentMaskDirectory + Path.DirectorySeparatorChar + STD_MNI_TEMPLATE_COPY_FILENAME + "_sn.mat",
                        currentMaskDirectory + Path.DirectorySeparatorChar + "transmat_MNItoNative.mat");

            // update the currentGlobalMaskFile to the resulting file
            currentNativeGlobalMaskFile = currentMaskDirectory + Path.DirectorySeparatorChar + "native_" + Path.GetFileName(currentMNIGlobalMaskFile);

            // rename the resulting file (remove the copy prefix)
            string newFilepath = currentMaskDirectory + Path.DirectorySeparatorChar + Path.GetFileName(currentNativeGlobalMaskFile).Replace(STD_COPY_PREFIX, "");
            File.Move(currentNativeGlobalMaskFile, newFilepath);
            currentNativeGlobalMaskFile = newFilepath;

            // After applying the transformation matrix, the normalizer mask image does not have the same dimensions as the original image, so we do this step here
            // Also update the currentGlobalMaskFile to the resulting file
            MatlabWrapper.sendCommand("clear matlabbatch;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.coreg.write.ref = {'" + currentInputVolume.filepath + ",1'};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.coreg.write.source = {'" + currentNativeGlobalMaskFile + ",1'};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.coreg.write.roptions.interp = 0;");                   // (coreg) reslice; nearest neighbour interpolation
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.coreg.write.roptions.wrap = [0 0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.coreg.write.roptions.mask = 0;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.coreg.write.roptions.prefix = 'resliced_';");
            MatlabWrapper.sendCommand("spm_jobman('run', matlabbatch); ");
            currentNativeGlobalMaskFile = currentMaskDirectory + Path.DirectorySeparatorChar + "resliced_" + Path.GetFileName(currentNativeGlobalMaskFile);

            // create a an output volume, since it is based on the input volume, reuse some of it's properties
            currentOutputVolume = new Volume(Volume.VolumeSource.Generated);
            currentOutputVolume.filepath = currentNativeGlobalMaskFile;
            currentOutputVolume.headerCorrection = currentInputVolume.headerCorrection;
            currentOutputVolume.orientDataCorrection = currentInputVolume.orientDataCorrection;
            

            // create a copy of the resliced native global mask (to become the RT orientated version of it)
            currentRTNativeGlobalMaskFile = currentMaskDirectory + Path.DirectorySeparatorChar + "RT_" + Path.GetFileName(currentNativeGlobalMaskFile);
            if (!NiftiHelper.copyNiftiFilesAndRename(currentNativeGlobalMaskFile, currentRTNativeGlobalMaskFile)) {
                // error while creating copy

                // message and return
                logger.Error("Could not copy the global native mask file ('" + currentNativeGlobalMaskFile + "') to '" + currentRTNativeGlobalMaskFile + "'");
                return;

            }

            // retrieve the correction volume
            Volume correctionVolume = Session.getCorrectionVolume();

            // check if we need to tranform the data to make the global mask to a RT oriented global mask
            if (correctionVolume.revOrientDataCorrection != Nifti.NiftiImage.OrientationTransform.None) {

                // re-orient correct the data
                MatlabWrapper.sendCommand("volume = spm_vol('" + currentRTNativeGlobalMaskFile + "');volumeData = spm_read_vols(volume);volumeData = reorient3D(volumeData, " + (int)correctionVolume.revOrientDataCorrection + ");spm_write_vol(volume, volumeData);");
                MatlabWrapper.sendCommand("clear volumeData volume;");

            }

            // create a copy of the input volume object to serve as output volume
            currentRTOutputVolume = new Volume(Volume.VolumeSource.Generated);
            currentRTOutputVolume.filepath = currentRTNativeGlobalMaskFile;
            currentRTOutputVolume.orientDataCorrection = Nifti.NiftiImage.OrientationTransform.None;
            currentRTOutputVolume.revOrientDataCorrection = correctionVolume.revOrientDataCorrection;

            // message
            logger.Info("finished: " + currentMaskDirectory);

            // flag the task as finished
            finished = true;

        }

        public Volume getOutputMask() {

            // check if the task has not finished yet
            if (!finished) {

                logger.Error("Trying to retrieve the native global mask before the task has finished, returning null");

                // return failure
                return null;

            }

            // return the native mask
            return currentOutputVolume;
        }

        public Volume getRTOutputMask() {

            // check if the task has not finished yet
            if (!finished) {

                logger.Error("Trying to retrieve the RT native global mask before the task has finished, returning null");

                // return failure
                return null;

            }

            // return the RT native mask
            return currentRTOutputVolume;
        }

        public void reset() {

            // reset the paths
            currentMaskDirectory = null;
            currentMNIGlobalMaskFile = null;
            currentNativeGlobalMaskFile = null;
            currentRTNativeGlobalMaskFile = null;
            currentEpiMniTemplateFile = null;

            // reset the current input and output volumes
            currentInputVolume = null;
            currentOutputVolume = null;

            // reset the volume counter
            volumeCounter = 0;

            // flag as not finished
            finished = false;

        }

    }

}
