/**
 * AnatMniToNativeMaskTask class
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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace RETIF4.Tasks {

    /// <summary>  
	/// This task requires an anatomical scan that is exported from the scan computer (since DrinDataDumper cannot provide the correct header).
	/// After selecting the anatomical scan from a local drive using a GUI dialog. The SPM segmentation step is called which generates a transformation
	/// matrix (and it's inverse) to go from native space to MNI or vise versa. The 'FuncMniToNativeMaskTask' will provide the same but based on a functional scan.
    /// </summary>  
    public class AnatMniToNativeMaskTask : TaskBase, ITask {

        private static Logger logger = LogManager.GetLogger("AnatMniToNativeMaskTask");

        private const string STD_COPY_PREFIX = "cpy_";
        private const string STD_NATIVE_EPI_COPY_FILENAME = STD_COPY_PREFIX + "native_scan_epi";
        private const string STD_MNI_TEMPLATE_COPY_FILENAME = STD_COPY_PREFIX + "mni_template_epi";
        private const string STD_MNI_GLOBALMASK_COPY_FILENAME = STD_COPY_PREFIX + "mni_globalmask";
        private const string STD_ANATOMICAL_COPY_FILENAME = STD_COPY_PREFIX + "anat";
        
        private string epiMniTemplateFilepath = null;               // the filepath to an epi template in MNI space which is used to find the mni to native transformation (will be set by the constructor)
        private string mniGlobalmaskFilepath = null;                // the filepath to the MNI global mask that should be converted to a native mask (will be set by the constructor)

        private string currentMaskDirectory = null;                 // the working directory used for the current task (will be set while processing)
        private string currentAnatomyFile = null;                   // the anatomy file used for the current task to find the mni to native transformation (will be set while processing)
        private string currentMNIGlobalMaskFile = null;             // the mni global mask file used for the current task (will be set while processing)
        private string currentNativeGlobalMaskFile = null;          // the (resliced) native global mask file used for the current task (will be set while processing)
        private string currentNativeEpiFile = null;                 // the native api file used for the current task to reslice to (will be set while processing)
        private string currentRTNativeGlobalMaskFile = null;        // the (resliced) native global mask in RT image orientation file used for the current task (will be set while processing)
        
        private Volume currentOutputVolume = null;                  // the native mask volume as output for the current task (will be set while processing)
        private Volume currentRTOutputVolume = null;                // the RT native mask volume as output for the current task (will be set while processing)
        
        public AnatMniToNativeMaskTask(string workDirectory, string globalMaskFilepath) : base(workDirectory) {

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
                    this.mniGlobalmaskFilepath = globalMaskFilepath;

                } else {
                    
                    // message
                    logger.Error("Invalid global MNI mask filename ('" + Path.GetFileNameWithoutExtension(globalMaskFilepath) + "'). This filename is reserved for the copy that will be made, please rename the mask file");
                    this.mniGlobalmaskFilepath = null;

                }

            } else {

                // message
                logger.Error("Invalid (or empty) global MNI filepath ('" + globalMaskFilepath + "')");
                this.mniGlobalmaskFilepath = null;

            }
            
        }

        public void run() {


            //
            // file management
            //

            // build a new folder for the current task and create the folder
            currentMaskDirectory = outputDirectory + Path.DirectorySeparatorChar + "globalmask__mni_to_native__anat__" + DateHelper.getDateTime();
            Directory.CreateDirectory(currentMaskDirectory);


            //
            // Anatomical Nifti file from the scanner (dialog/usb)
            //

            // open file dialog to open file
            OpenFileDialog dlgLoadNiftiFile = new OpenFileDialog();

            // set initial directory
            //dlgLoadSessionFile.InitialDirectory = Directory.GetCurrentDirectory();
            dlgLoadNiftiFile.Filter = "Anatomical Nifti file (*.nii)|*.nii";
            dlgLoadNiftiFile.RestoreDirectory = true;

            // check if the dialog was cancelled
            if (dlgLoadNiftiFile.ShowDialog() != DialogResult.OK) {

                // message
                logger.Error("No anatomical nifti selected, returning without creating a global mask");
                return;
            }


            string extension = Path.GetExtension(dlgLoadNiftiFile.FileName);
            if (!string.Equals(extension, ".nii", StringComparison.OrdinalIgnoreCase)) {
                // not a nifti file

                // message
                logger.Error("Unknown input filetype for usage as anatomical nifti");
                return;

            }

            // copy the nifti file
            File.Copy(  dlgLoadNiftiFile.FileName,
                        currentMaskDirectory + Path.DirectorySeparatorChar + STD_ANATOMICAL_COPY_FILENAME + ".nii");

            // set the filepath to the copy
            currentAnatomyFile = currentMaskDirectory + Path.DirectorySeparatorChar + STD_ANATOMICAL_COPY_FILENAME + ".nii";


            // message
            logger.Info("Copied anatomy, using '" + currentAnatomyFile + "' as anatomy");




            // copy the MNI global mask file to the masks directory
            if (!NiftiHelper.copyNiftiFilesAndRename(mniGlobalmaskFilepath, currentMaskDirectory + Path.DirectorySeparatorChar + STD_MNI_GLOBALMASK_COPY_FILENAME)) {
                // error while creating copiy

                // message and return
                logger.Error("Could not copy the global MNI mask file ('" + epiMniTemplateFilepath + "') to the current mask directory ('" + currentMaskDirectory + "')");
                return;

            }

            // set the filepath to the copy
            currentMNIGlobalMaskFile = currentMaskDirectory + Path.DirectorySeparatorChar + STD_MNI_GLOBALMASK_COPY_FILENAME;
            currentMNIGlobalMaskFile += NiftiHelper.getNiftiSetExtension(currentMNIGlobalMaskFile);

            // message
            logger.Info("Copied global MNI mask, using '" + currentMNIGlobalMaskFile + "' as mask");





            // retrieve the correction volume
            Volume correctionEPIVolume = Session.getCorrectionVolume();
            if (correctionEPIVolume == null) {

                // message
                logger.Error("Cannot reslice native, no correction volume in session, no global mask could be created");

                // return
                return;

            }
            
            // copy an epi file (the correction volume) to the masks directory
            if (!NiftiHelper.copyNiftiFilesAndRename(correctionEPIVolume.filepath, currentMaskDirectory + Path.DirectorySeparatorChar + STD_NATIVE_EPI_COPY_FILENAME)) {
                // error while creating copy

                // message and return
                logger.Error("Could not copy the EPI file ('" + correctionEPIVolume.filepath + "') to the current mask directory ('" + currentMaskDirectory + "')");
                return;

            }

            // set the filepath to the copy
            currentNativeEpiFile = currentMaskDirectory + Path.DirectorySeparatorChar + STD_NATIVE_EPI_COPY_FILENAME;
            currentNativeEpiFile += NiftiHelper.getNiftiSetExtension(currentNativeEpiFile);

            // message
            logger.Info("Copied EPI, using '" + currentNativeEpiFile + "' as native EPI");





            //
            // matlab calls
            //
            
            // run the segmentation and normalize write
            MatlabWrapper.sendCommand("clear matlabbatch;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.channel.vols = { '" + currentAnatomyFile + ",1'};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.channel.biasreg = 0.001;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.channel.biasfwhm = 60;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.channel.write = [0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(1).tpm = { '" + MatlabWrapper.getFunctionDirectory() + "\\spm12\\tpm\\TPM.nii,1'};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(1).ngaus = 1;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(1).native = [0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(1).warped = [0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(2).tpm = { '" + MatlabWrapper.getFunctionDirectory() + "\\spm12\\tpm\\TPM.nii,2'};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(2).ngaus = 1;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(2).native = [0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(2).warped = [0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(3).tpm = { '" + MatlabWrapper.getFunctionDirectory() + "\\spm12\\tpm\\TPM.nii,3'};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(3).ngaus = 2;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(3).native = [0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(3).warped = [0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(4).tpm = { '" + MatlabWrapper.getFunctionDirectory() + "\\spm12\\tpm\\TPM.nii,4'};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(4).ngaus = 3;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(4).native = [0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(4).warped = [0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(5).tpm = { '" + MatlabWrapper.getFunctionDirectory() + "\\spm12\\tpm\\TPM.nii,5'};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(5).ngaus = 4;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(5).native = [0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(5).warped = [0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(6).tpm = { '" + MatlabWrapper.getFunctionDirectory() + "\\spm12\\tpm\\TPM.nii,6'};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(6).ngaus = 2;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(6).native = [0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.tissue(6).warped = [0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.warp.mrf = 1;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.warp.cleanup = 1;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.warp.reg = [0 0.001 0.5 0.05 0.2];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.warp.affreg = 'mni';");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.warp.fwhm = 0;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.warp.samp = 3;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.preproc.warp.write = [1 1];");
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.spatial.normalise.write.subj.def(1) = cfg_dep('Segment: Inverse Deformations', substruct('.','val', '{}',{1}, '.','val', '{}',{1}, '.','val', '{}',{1}), substruct('.','invdef', '()',{':'}));");
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.spatial.normalise.write.subj.resample = { '" + currentMNIGlobalMaskFile + ",1'};");
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.spatial.normalise.write.woptions.bb = [-90, -120, -100; 90,90,100];");
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.spatial.normalise.write.woptions.vox = [1 1 1];");
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.spatial.normalise.write.woptions.interp = 0;");
            MatlabWrapper.sendCommand("matlabbatch{2}.spm.spatial.normalise.write.woptions.prefix = 'native_';");
            MatlabWrapper.sendCommand("spm_jobman('run', matlabbatch); ");
            Thread.Sleep(100);  // allow jobman to finish writing

            // rename the MNI to Naive transformation matrix file
            File.Move(  currentMaskDirectory + Path.DirectorySeparatorChar + "iy_" + STD_ANATOMICAL_COPY_FILENAME + ".nii",
                        currentMaskDirectory + Path.DirectorySeparatorChar + "transmat_MNItoNative.nii");
            
            // update the currentGlobalMaskFile to the resulting file
            currentNativeGlobalMaskFile = currentMaskDirectory + Path.DirectorySeparatorChar + "native_" + Path.GetFileName(currentMNIGlobalMaskFile);

            // rename the resulting file (remove the copy prefix)
            string newFilepath = currentMaskDirectory + Path.DirectorySeparatorChar + Path.GetFileName(currentNativeGlobalMaskFile).Replace(STD_COPY_PREFIX, "");
            File.Move(currentNativeGlobalMaskFile, newFilepath);
            currentNativeGlobalMaskFile = newFilepath;

            // After applying the transformation matrix, the normalizer mask image does not have the same dimensions as the original image, so we do this step here
            // Also update the currentGlobalMaskFile to the resulting file
            MatlabWrapper.sendCommand("clear matlabbatch;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.coreg.write.ref = {'" + currentNativeEpiFile + ",1'};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.coreg.write.source = {'" + currentNativeGlobalMaskFile + ",1'};");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.coreg.write.roptions.interp = 0;");                   // (coreg) reslice; nearest neighbour interpolation
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.coreg.write.roptions.wrap = [0 0 0];");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.coreg.write.roptions.mask = 0;");
            MatlabWrapper.sendCommand("matlabbatch{1}.spm.spatial.coreg.write.roptions.prefix = 'resliced_';");
            MatlabWrapper.sendCommand("spm_jobman('run', matlabbatch); ");
            currentNativeGlobalMaskFile = currentMaskDirectory + Path.DirectorySeparatorChar + "resliced_" + Path.GetFileName(currentNativeGlobalMaskFile);



            // retrieve the correction volume
            Volume correctionVolume = Session.getCorrectionVolume();

            // create a copy of the input volume object to serve as output volume
            currentOutputVolume = new Volume(Volume.VolumeSource.Generated);
            currentOutputVolume.filepath = currentNativeGlobalMaskFile;
            currentOutputVolume.orientDataCorrection = correctionVolume.orientDataCorrection;

            // create a copy of the resliced native global mask (to become the RT orientated version of it)
            currentRTNativeGlobalMaskFile = currentMaskDirectory + Path.DirectorySeparatorChar + "RT_" + Path.GetFileName(currentNativeGlobalMaskFile);
            if (!NiftiHelper.copyNiftiFilesAndRename(currentNativeGlobalMaskFile, currentRTNativeGlobalMaskFile)) {
                // error while creating copy

                // message and return
                logger.Error("Could not copy the global native mask file ('" + currentNativeGlobalMaskFile + "') to '" + currentRTNativeGlobalMaskFile + "'");
                return;

            }

            // check if we need to tranform the data to make the global mask to a RT oriented global mask
            if (correctionVolume.revOrientDataCorrection != Nifti.NiftiImage.OrientationTransform.None) {

                // re-orient correct the data
                MatlabWrapper.sendCommand("volume = spm_vol('" + currentRTNativeGlobalMaskFile + "');volumeData = spm_read_vols(volume);volumeData = reorient3D(volumeData, " + (int)correctionVolume.revOrientDataCorrection + ");spm_write_vol(volume, volumeData);");
                MatlabWrapper.sendCommand("clear volumeData volume;");

            }

            // create a object to serve as RT output volume
            currentRTOutputVolume = new Volume(Volume.VolumeSource.Generated);
            currentRTOutputVolume.filepath = currentRTNativeGlobalMaskFile;
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
            currentAnatomyFile = null;
            currentMNIGlobalMaskFile = null;
            currentNativeGlobalMaskFile = null;
            currentNativeEpiFile = null;
            currentRTNativeGlobalMaskFile = null;

            // reset the current output volumes
            currentOutputVolume = null;
            
            // flag as not finished
            finished = false;

        }

    }

}
