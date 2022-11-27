/**
 * RTCorrectionVolumeTask class
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
using System.Windows.Forms;
using static RETIF4.Data.Volume;
using static RETIF4.Nifti.NiftiImage;

namespace RETIF4.Tasks {

    /// <summary>  
    ///  This task takes a real-time volume and an exported/coverted PAR/REC or Nifi volume, and learn the application how to correct both the header and (optionally) the data orientation of RT images
    /// </summary>  
    public class RTCorrectionVolumeTask : TaskBase, ITask {

        private static Logger logger = LogManager.GetLogger("RTCorrectionVolumeTask");

        private const string STD_COPY_PREFIX = "cpy_";
        private const string STD_REALTIME_INPUT_COPY_FILENAME = STD_COPY_PREFIX + "realtime_scan_epi";                  // the uncorrected real-time input EPI
        private const string STD_CORRECTION_VOLUME_FILENAME = "correction_volume";                                      // the external incoming EPI which's header and orientation is used to correct other volumes
        private const string STD_REALTIME_CORRECTED_FILENAME = "corrected_realtime_scan_epi";                           // the corrected version of the real-time input EPI
        

        private string currentCorrectionVolumeDirectory = null;     // the working directory used for the current task (will be set while processing)
        private Volume currentInputVolume = null;                   // the epi volume used as input for the current task (will be set while processing)
        private Volume currentOutputVolume = null;                  // the correction volume as output for the current task (will be set while processing)
        
        public RTCorrectionVolumeTask(string workDirectory) : base(workDirectory) {

            
        }


        public void process(Volume volume) {

            // check if the task is not yet finished and we do not have a volume yet
            if (!finished && currentInputVolume == null) {

                // store the volume we will use for input
                currentInputVolume = volume.clone();

                // execute the task on this volume
                run();

            }

        }

        private void run() {


            //
            // file management
            //


            // build a new folder for the current task and create the folder
            currentCorrectionVolumeDirectory = outputDirectory + Path.DirectorySeparatorChar + "correction_volume__" + DateHelper.getDateTime();
            Directory.CreateDirectory(currentCorrectionVolumeDirectory);




            //
            // realtime file from the scanner (input from collector)
            //


            // copy the input volume files to the masks directory
            if (!NiftiHelper.copyNiftiFilesAndRename(currentInputVolume.filepath, currentCorrectionVolumeDirectory + Path.DirectorySeparatorChar + STD_REALTIME_INPUT_COPY_FILENAME)) {
                // error while creating copies

                // message and return
                logger.Error("Could not copy the scan input file ('" + currentInputVolume.filepath + "') to the current correction volume directory ('" + currentCorrectionVolumeDirectory + "')");
                return;

            }

            // set the filepath to the copy
            currentInputVolume.filepath = currentCorrectionVolumeDirectory + Path.DirectorySeparatorChar + STD_REALTIME_INPUT_COPY_FILENAME;
            currentInputVolume.filepath += NiftiHelper.getNiftiSetExtension(currentInputVolume.filepath);

            // message
            logger.Info("Copied input volume, using '" + currentInputVolume.filepath + "' as real-time volume input");


            //
            // PAR/REC file from the scanner (dialog/usb)
            //

            // open file dialog to open file
            OpenFileDialog dlgLoadExternalFile = new OpenFileDialog();

            // set initial directory
            //dlgLoadSessionFile.InitialDirectory = Directory.GetCurrentDirectory();
            dlgLoadExternalFile.Filter = "PAR file (*.par)|*.par|Rec file (*.rec)|*.rec|Nifti file (*.nii)|*.nii|All files (*.*)|*.*";
            dlgLoadExternalFile.RestoreDirectory = true;

            // check if the dialog was cancelled
            if (dlgLoadExternalFile.ShowDialog() != DialogResult.OK) {

                // message
                logger.Error("No correction volume selected, returning without updating correction volume");

                return;
            }
            
            string extension = Path.GetExtension(dlgLoadExternalFile.FileName);
            if (string.Equals(extension, ".par", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".rec", StringComparison.OrdinalIgnoreCase)) {
                // par/rec file

                // get the filepath parts
                string parRecFilename = Path.GetFileNameWithoutExtension(dlgLoadExternalFile.FileName);
                string parRecDirectory = Path.GetDirectoryName(dlgLoadExternalFile.FileName);
                parRecDirectory = parRecDirectory.TrimEnd('\\').TrimEnd('/') + Path.DirectorySeparatorChar;

                // check if both the .par and .rec files are present
                if (!File.Exists(parRecDirectory + parRecFilename + ".par") || !File.Exists(parRecDirectory + parRecFilename + ".rec")) {

                    // message
                    logger.Error("Could not find both the .par and .rec file for '" + parRecDirectory + parRecFilename + "', returning without updating correction volume");

                    // return failure
                    return;

                }

                // copy both files
                File.Copy(parRecDirectory + parRecFilename + ".par", currentCorrectionVolumeDirectory + Path.DirectorySeparatorChar + STD_COPY_PREFIX + parRecFilename + ".par");
                File.Copy(parRecDirectory + parRecFilename + ".rec", currentCorrectionVolumeDirectory + Path.DirectorySeparatorChar + STD_COPY_PREFIX + parRecFilename + ".rec");


                //
                // matlab calls
                //

                //
                // use r2agui in matlab to convert the par/rec to nifti
                //

                // set options
                MatlabWrapper.sendCommand("clear options;");
                MatlabWrapper.sendCommand("options.angulation = 1;");
                MatlabWrapper.sendCommand("options.rescale = 1;");
                MatlabWrapper.sendCommand("options.outputformat = 1;");         //  1 = nifti, 2 = analyze
                MatlabWrapper.sendCommand("options.usedirtydtihack = 0;");
                MatlabWrapper.sendCommand("options.pathpar = ['" + currentCorrectionVolumeDirectory + "', filesep];");         // set complete path containing PAR files(with trailing /)
                MatlabWrapper.sendCommand("options.usefullprefix = 0;");
                MatlabWrapper.sendCommand("options.subaan = 0;");
                MatlabWrapper.sendCommand("options.usealtfolder = 0;");
                MatlabWrapper.sendCommand("options.prefix = '';");

                // set the par/rec to convert
                MatlabWrapper.sendCommand("clear rawFilepath;");
                MatlabWrapper.sendCommand("rawFilepath{1} = '" + STD_COPY_PREFIX + parRecFilename + ".par" + "';");

                // convert par/rec to nifti
                MatlabWrapper.sendCommand("convert_r2a(rawFilepath, options);");

                // try to find 
                string[] resultFiles = Directory.GetFiles(currentCorrectionVolumeDirectory, "*-0001.nii");

                // check if there are no files, return failure
                if (resultFiles.Length == 0) {

                    // message
                    logger.Error("Error during conversion, no nifti found in output folder after r2agui '" + currentCorrectionVolumeDirectory + "', returning without updating correction volume");

                    // return failure
                    return;

                }

                // copy the resulting nifti file
                File.Copy(  resultFiles[0],
                            currentCorrectionVolumeDirectory + Path.DirectorySeparatorChar + STD_CORRECTION_VOLUME_FILENAME + ".nii");


            } else if (string.Equals(extension, ".nii", StringComparison.OrdinalIgnoreCase)) {
                // nii file

                // copy the nifti file
                File.Copy(  dlgLoadExternalFile.FileName,
                            currentCorrectionVolumeDirectory + Path.DirectorySeparatorChar + STD_CORRECTION_VOLUME_FILENAME + ".nii");

            } else {

                // message
                logger.Error("Unknown input filetype for usage as correction volume");

                // return failure
                return;

            }

            // create an hdr/img version of the file (using a .hdr file makes it easier to quickly replace the header of a real-time volume)
            MatlabWrapper.sendCommand("inputFile = '" + currentCorrectionVolumeDirectory + Path.DirectorySeparatorChar + STD_CORRECTION_VOLUME_FILENAME + ".nii" + "';");
            MatlabWrapper.sendCommand("[pathstr, fname, ext] = fileparts(inputFile);");
            MatlabWrapper.sendCommand("outputFile = strrep(inputFile,'.nii','.img');");
            MatlabWrapper.sendCommand("V = spm_vol(inputFile);");
            MatlabWrapper.sendCommand("ima = spm_read_vols(V);");
            MatlabWrapper.sendCommand("V.fname = outputFile;");
            MatlabWrapper.sendCommand("spm_write_vol(V, ima);");

            // create a volume object, store filepath to the resulting nifti and set as output volume
            Volume correctionVolume = new Volume(VolumeSource.Generated);
            correctionVolume.filepath = currentCorrectionVolumeDirectory + Path.DirectorySeparatorChar + STD_CORRECTION_VOLUME_FILENAME + ".hdr";
            correctionVolume.headerCorrection = HeaderCorrection.Correction_Volume;

            // determine the re-orientation transformation in order to get the data the same as the par/rec
            MatlabWrapper.sendCommand("[trIndex, revTrIndex] = findOrientTransIndex('" + correctionVolume.filepath + "', '" + currentInputVolume.filepath + "');");

            // store the orientation transformation in the volume
            int transIndex = MatlabWrapper.getIntVariable("trIndex");
            int revTransIndex = MatlabWrapper.getIntVariable("revTrIndex");
            correctionVolume.orientDataCorrection = (OrientationTransform)transIndex;
            correctionVolume.revOrientDataCorrection = (OrientationTransform)revTransIndex;

            // create a copy of the real-time nifti
            if (!NiftiHelper.copyNiftiFilesAndRename(currentInputVolume.filepath, currentCorrectionVolumeDirectory + Path.DirectorySeparatorChar + STD_REALTIME_CORRECTED_FILENAME)) {
                // error while creating copies

                // message and return
                logger.Error("Could not create a copy of the file ('" + currentInputVolume.filepath + "') in the current correction volume directory ('" + currentCorrectionVolumeDirectory + "')");
                return;

            }

            // correct the real-time nifti
            if (!NiftiHelper.correctRealtimeNifti(currentCorrectionVolumeDirectory + Path.DirectorySeparatorChar + STD_REALTIME_CORRECTED_FILENAME + ".img", correctionVolume, true, true, false, 0, false, false, true)) {

                logger.Error("Could not correct '" + currentCorrectionVolumeDirectory + Path.DirectorySeparatorChar + STD_REALTIME_CORRECTED_FILENAME + ".img" + "'");
                return;

            }
            
            // set as output volume
            currentOutputVolume = correctionVolume;

            // message
            logger.Info("finished: " + currentCorrectionVolumeDirectory);

            // flag the task as finished
            finished = true;
            
        }

        public Volume getOutputVolume() {

            // check if the task has not finished yet
            if (!finished) {

                logger.Error("Trying to retrieve the correction volume before the task has finished, returning null");

                // return failure
                return null;

            }

            // return the native mask
            return currentOutputVolume;
        }

        public void reset() {

            // reset the path
            currentCorrectionVolumeDirectory = null;

            // reset the current input and output volumes
            currentInputVolume = null;
            currentOutputVolume = null;

            // flag as not finished
            finished = false;

        }

    }

}
