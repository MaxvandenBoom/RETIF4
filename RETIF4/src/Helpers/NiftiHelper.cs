/**
 * NiftiHelper class
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
using RETIF4.Matlab;
using RETIF4.Nifti;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace RETIF4.Helpers {

    public static class NiftiHelper {

        private static Logger logger = LogManager.GetLogger("NiftiHelper");
        private static Volume realignVolume = null;
        private static Volume roiVolume = null;

        //
        // note: correctRealtimeNifti functions require matlab to be started
        //

        public static bool correctRealtimeNifti(string filepath, bool correctHeader, bool reorientData, bool realignData, int smoothData, bool extractROIData, bool detrendROIData, bool writeData) {
            return correctRealtimeNifti(new Volume(Volume.VolumeSource.Generated, filepath), correctHeader, reorientData, realignData, smoothData, extractROIData, detrendROIData, writeData);
        }
        public static bool correctRealtimeNifti(Volume volume, bool correctHeader, bool reorientData, bool realignData, int smoothData, bool extractROIData, bool detrendROIData, bool writeData) {

            // check if we have a correction volume in the session to use
            Volume correctionVolume = Session.getCorrectionVolume();
            if (reorientData && correctionVolume == null) {

                // message
                logger.Error("RT image data should be corrected but no correction volume in session, returning failure");

                // return failure
                return false;
            }

            return correctRealtimeNifti(volume, correctionVolume, correctHeader, reorientData, realignData, smoothData, extractROIData, detrendROIData, writeData);
        }
        

        public static bool correctRealtimeNifti(string filepath, Volume correctionVolume, bool correctHeader, bool reorientData, bool realignData, int smoothData, bool extractROIData, bool detrendROIData, bool writeData) {
            return correctRealtimeNifti(new Volume(Volume.VolumeSource.Generated, filepath), correctionVolume, correctHeader, reorientData, realignData, smoothData, extractROIData, detrendROIData, writeData);
        }

        /// <summary> 
        /// Method to Display Geeksforgeeks Message 
        /// </summary> 
        /// <param name="volume">The (incoming) volume object to be real-time corrected (header, reorientation, realignment)</param> 
        /// <param name="correctionVolume">The (correction) volume object which is used to correct the header and orientation of the incoming volume. Note: to allow correction to an alternative volume that this function is overloaded; if no correction volume is given, it will be taken from the session.</param> 
        /// <param name="correctHeader">Flag whether the header of the (incoming) volume object should be corrected</param> 
        /// <param name="reorientData">Reorient the data of the (incoming) volume object to the (correction) volume's orientation</param> 
        /// <param name="realignData">Realign the data of the (incoming) volume object to the session's realignment volume</param> 
        /// <param name="smoothData">Smooth the data of the (incoming) volume object. 0 to skip smoothing, > 0 = fwhm parameter</param> 
        /// <param name="extractROIData">Extract the ROI data from the (incoming) volume object</param> 
        /// <param name="writeData">Flag whether the reoriented and realigned data should be written to the disk. Note that not writing will spare time, but might break steps which load the volume from file and expect data manipulation to be applied (e.g. SPM functions)</param> 
        public static bool correctRealtimeNifti(Volume volume, Volume correctionVolume, bool correctHeader, bool reorientData, bool realignData, int smoothData, bool extractROIData, bool detrendROIData, bool writeData) {

            // check if matlab is started
            if (MatlabWrapper.getMatlabState() != MatlabWrapper.MATLAB_STARTED) {

                // message
                logger.Error("RT image data should be corrected but matlab is not started, returning failure");

                // return failure
                return false;

            }

            // check if the input file exists
            if (!File.Exists(volume.filepath)) {

                // message
                logger.Error("RT image data should be corrected but file ('" + volume.filepath + "') does not exist, returning failure");

                // return failure
                return false;

            }

            Volume sessRealignVolume = null;
            // check if the data should be realigned 
            if (realignData) {

                // check if we have a realign volume in the session to use
                sessRealignVolume = Session.getRealignmentVolume();
                if (sessRealignVolume == null) {

                    // message
                    logger.Error("RT image data for file ('" + volume.filepath + "') should be realigned, but no realignment volume in session, not performing re-alignment");

                    // flag not to realign the data (later in this function)
                    realignData = false;

                } else {

                    // check if the realigmentVolume from the session is the same as the realignmentVolume in the helper
                    // (this is the first time realignment is needed, or a new realignment image could have been loaded into the session)
                    if (realignVolume != sessRealignVolume) {

                        // read the re-alignment volume for future use
                        MatlabWrapper.sendCommand("h_raVolPath = '" + sessRealignVolume.filepath + "';");
                        MatlabWrapper.sendCommand("h_raVol = spm_vol(h_raVolPath);");
                        MatlabWrapper.sendCommand("h_raVolAO = RT_spm_realign_mem_createAlignObject(h_raVol);");

                        // store the session volume as the current
                        realignVolume = sessRealignVolume;

                    }


                }

            }

            // check if the ROI data should be extracted
            if (extractROIData) {

                // check if we have a ROI maskin the session to use
                Volume sessROIVolume = Session.getRoiMask();
                if (sessROIVolume == null) {

                    // message
                    logger.Error("The ROI data should be extracted from RT image ('" + volume.filepath + "'), but no ROI mask in session, not performing ROI extraction");

                    // flag not to extract the data (later in this function)
                    extractROIData = false;

                } else {

                    // check if the roiVolume from the session is the same as the roiVolume in the helper
                    // (this is the first time ROI data extraction is needed, or a new ROI mask image could have been loaded into the session)
                    if (roiVolume != sessROIVolume) {

                        // read the ROI mask volume for future use
                        MatlabWrapper.sendCommand("h_roiVolPath = '" + sessROIVolume.filepath + "';");
                        MatlabWrapper.sendCommand("h_roiVol = readVolume('" + sessROIVolume.filepath + "');");
                        MatlabWrapper.sendCommand("h_roiVolL = logical(h_roiVol);");

                        // store the session volume as the current
                        roiVolume = sessROIVolume;

                    }


                }

            }


            // check if the header should be corrected
            if (correctHeader) {

                // check the correction volume
                bool headerCorrected = false;
                if (correctionVolume != null && correctionVolume.filepath.EndsWith(".hdr")) {
                    // correction volume to use and in .hdr format

                    // get the filepath parts
                    string hdrFilename = Path.GetFileNameWithoutExtension(volume.filepath);
                    string hdrDirectory = Path.GetDirectoryName(volume.filepath);

                    // make sure the dstDirectory has a trailing slash
                    hdrDirectory = hdrDirectory.TrimEnd('\\').TrimEnd('/') + Path.DirectorySeparatorChar;

                    // check if a .hdr file exists for the input file to correct on
                    if (File.Exists(hdrDirectory + hdrFilename + ".hdr")) {

                        // remove the original header file
                        File.Delete(hdrDirectory + hdrFilename + ".hdr");

                        // copy the header correction volume .hdr to resplace the original header file
                        File.Copy(correctionVolume.filepath,
                                    hdrDirectory + hdrFilename + ".hdr");

                        // flag the header as corrected in the volume object
                        volume.headerCorrection = Volume.HeaderCorrection.Correction_Volume;

                        // flag as corrected
                        headerCorrected = true;

                    } else {

                        // message
                        logger.Warn("The input file is not in the hdr/img format, fast correction of the header requires overwriting the input .hdr file");

                    }

                } else {

                    // message
                    logger.Warn("The correction volume is not in the hdr/img format, fast correction of the header requires the .hdr file from the correction volume");

                }

                // check if the header was not corrected using the correction volume
                if (!headerCorrected) {
                    // correct using a standard header
                    
                    // message
                    logger.Warn("Using standard header to correct the real-time volume header. It is recommended to use a correction volume.");

                    // correct the file
                    MatlabWrapper.sendCommand("correctVol = spm_vol('" + volume.filepath + "');");
                    MatlabWrapper.sendCommand("correctVol.mat = [-2.1904, -0.0098, 0.0032, 121.3716; -0.0098, -2.1905, -0.0017, 124.8064; 0.0032, 0.0017, 2.2000, 6.6184; 0, 0, 0, 1];");
                    MatlabWrapper.sendCommand("spm_create_vol(correctVol);");

                    // flag the header as to standard header in the volume object
                    volume.headerCorrection = Volume.HeaderCorrection.Standard_Header;

                }

            }



            // check if the data should be reoriented, realigned, ROI extracted or ROI detrended
            if ((reorientData && correctionVolume != null && correctionVolume.orientDataCorrection != Nifti.NiftiImage.OrientationTransform.None) || realignData || smoothData > 0 || extractROIData || detrendROIData) {

                // check if the volume is not header-corrected
                if (volume.headerCorrection == Volume.HeaderCorrection.None) {

                    // message
                    logger.Error("Non-corrected header. Reorientation, realignment, smoothing, extraction of ROI data and/or detrending of ROI data currently require the header on the input volume to be corrected (elsewise spm read routines will not work)");

                    // return failure
                    return false;

                }

                // (matlab) read the volume
                string matlabCommand = "h_Vol = spm_vol('" + volume.filepath + "');";
                matlabCommand += "h_VolData = spm_read_vols(h_Vol);";


                // check if the data should be reoriented
                if (reorientData && correctionVolume != null) {

                    // check if the data is already corrected
                    if (volume.orientDataCorrection != Nifti.NiftiImage.OrientationTransform.None) {
                        // input volume was already reoriented

                        // message
                        logger.Warn("The input volume has already been re-oriented, skipping re-orientation of the data");

                    } else {
                        // input volume is not reoriented

                        // check if the data needs a orientation transformation
                        if (correctionVolume.orientDataCorrection != Nifti.NiftiImage.OrientationTransform.None) {

                            // (matlab) apply data oriententation correction
                            matlabCommand += "h_VolData = reorient3D(h_VolData, " + (int)correctionVolume.orientDataCorrection + ");";
                            
                            // store the orientation correction in the volume object
                            volume.orientDataCorrection = correctionVolume.orientDataCorrection;

                        }

                    }

                }

                // check if data realignment should be applied
                if (realignData) {

                    // check if the data is already realigned
                    if (volume.realigned) {
                        // already realigned

                        // message
                        logger.Warn("The input volume has already been re-aligned, skipping re-alignment of the data");

                    } else {
                        // not realigned

                        // check if the realignment volume and the input volume do not have the same orientation
                        if (realignVolume.orientDataCorrection != volume.orientDataCorrection) {

                            // message
                            logger.Error("The realignment volume and input volume have different data orientations");

                            // return failure
                            return false;

                        }


                        /*
                        // TODO: write before realigning
                        matlabCommand += "spm_write_vol(h_Vol, h_VolData);";

                        // TODO: come up with a new filename for the realigned image
                        string newFile = volume.filepath;
                        int index = newFile.LastIndexOf("\\");
                        newFile = newFile.Insert(index + 1, "b_");
                        volume.filepath = newFile;

                        // TODO: update the filename
                        matlabCommand += "h_Vol.fname = '" + newFile + "';";
                        */


                        // (matlab) SPM realignment done in memory
                        matlabCommand += "[h_VolData, est] = SPMAlignImageMem2(h_raVolAO, h_Vol, h_VolData);";
                        
                        // flag the realignment in the volume object
                        volume.realigned = true;
                    }
                }


                // check if data smoothing should be applied
                if (smoothData > 0) {

                    // check if the data is already smoothed
                    if (volume.smoothed > 0) {
                        // already smoothed

                        // message
                        logger.Warn("The input volume has already been smoothed (FWHM: " + volume.smoothed + "), skipping smoothing of the data");

                    } else {
                        // not smoothed

                        /*
                        // TODO: write before smoothing
                        matlabCommand += "spm_write_vol(h_Vol, h_VolData);";

                        // TODO: come up with a new filename for the smoothed image
                        string newFile = volume.filepath;
                        int index = newFile.LastIndexOf("\\");
                        newFile = newFile.Insert(index + 1, "b_");
                        volume.filepath = newFile;

                        // TODO: update the filename
                        matlabCommand += "h_Vol.fname = '" + newFile + "';";
                        */


                        // (matlab) SPM smoothing done in memory
                        matlabCommand += "h_VolData = RT_spm_smooth_mem(h_Vol.mat, h_VolData, " + smoothData + ");";

                        // flag the smoothing in the volume object
                        volume.smoothed = smoothData;
                    }

                }
                
                // check and (matlab) write the result
                if (writeData)
                    matlabCommand += "spm_write_vol(h_Vol, h_VolData);";

                // check if ROI data should be extracted
                if (extractROIData) {
                    
                    // check if the ROI mask and the input volume do not have the same orientation
                    if (roiVolume.orientDataCorrection != volume.orientDataCorrection) {

                        // message
                        logger.Error("The ROI mask and input volume have different data orientations");

                        // return failure
                        return false;

                    }

                    // (matlab) extract the roi data
                    matlabCommand += "h_roiV = double(h_VolData(h_roiVolL == 1));";

                }

                // (matlab) execute the correction and/or realignment
                MatlabWrapper.sendCommand(matlabCommand);


                // check if ROI data should be extracted
                if (extractROIData) {

                    // retrieve the data from matlab
                    double[] h_roiV = Matlab.MatlabWrapper.getDoubleArray("h_roiV");

                    // store the data in the volume object
                    volume.roiValues = h_roiV;

                }

                // (matlab) clear the volume data
                MatlabWrapper.sendCommand("clear h_VolData h_Vol;");

                // check if ROI data should be detrenden
                if (detrendROIData) {




                }

            }

            // return success
            return true;

        }
        

        public static void transferFileListToMatlab(List<Volume> volumes) {
            string[] strVolumes = new string[volumes.Count];
            for (int i = 0; i < volumes.Count; i++) strVolumes[i] = volumes[i].filepath;
            transferFileListToMatlab(strVolumes);
        }
        public static void transferFileListToMatlab(string[] volumes) {
            if (MatlabWrapper.getMatlabState() != MatlabWrapper.MATLAB_STARTED)     return;
            if (volumes.Length < 1)                                                 return;

            // get the directory of the first file
            string firstDirectory = Path.GetDirectoryName(volumes[0]);
            firstDirectory = firstDirectory.TrimEnd('\\').TrimEnd('/') + Path.DirectorySeparatorChar;

            // write the files to a file
            string filesFilename = firstDirectory + "files" + DateHelper.getDateTime();
            File.WriteAllLines(filesFilename, volumes);

            // read the filenames into matlab
            MatlabWrapper.sendCommand("clear files;fileID = fopen('" + filesFilename + "', 'r');files = textscan(fileID, '%s', 'delimiter', '\\n');files = files{1};fclose(fileID);clear fileID;");

            // delete the file
            //Thread.Sleep(50);
            //File.Delete(filesFilename);

        }

        // copies (a set of) nifti files to a folder
        public static bool copyNiftiFilesToDirectory(string sourceFilename, string dstDirectory) {

            // check if source file and destination directory exist, return failure if not
            if (!File.Exists(sourceFilename) || !Directory.Exists(dstDirectory)) return false;

            // get the filepath parts
            string srcFilename = Path.GetFileNameWithoutExtension(sourceFilename);
            string srcDirectory = Path.GetDirectoryName(sourceFilename);

            // make sure the dstDirectory has a trailing slash
            dstDirectory = dstDirectory.TrimEnd('\\').TrimEnd('/') + Path.DirectorySeparatorChar;

            // list the files
            string[] inputFiles = Directory.GetFiles(srcDirectory, srcFilename + "*.*");

            // check if there are no files, return failure
            if (inputFiles.Length == 0) return false;

            // loop through the files
            for (int i = 0; i < inputFiles.Length; i++) {

                // copy each file
                File.Copy(inputFiles[i], dstDirectory + Path.GetFileName(inputFiles[i]));

            }

            // return success
            return true;

        }

        // moves (a set of) nifti files to a filepath while renaming the filename
        public static bool copyNiftiFilesAndRename(string sourceFilename, string dstFilepath) {

            // check if source file exist, return failure if not
            if (!File.Exists(sourceFilename)) return false;

            // get the filepath parts
            string srcFilename = Path.GetFileNameWithoutExtension(sourceFilename);
            string srcDirectory = Path.GetDirectoryName(sourceFilename);
            string dstDirectory = Path.GetDirectoryName(dstFilepath);
            string dstFilename = Path.GetFileNameWithoutExtension(dstFilepath);

            // check if the dest directory exist, return failure if not
            if (!Directory.Exists(dstDirectory)) return false;

            // make sure the dstDirectory has a trailing slash
            dstDirectory = dstDirectory.TrimEnd('\\').TrimEnd('/') + Path.DirectorySeparatorChar;

            // list the files
            string[] inputFiles = Directory.GetFiles(srcDirectory, srcFilename + "*.*");

            // check if there are no files, return failure
            if (inputFiles.Length == 0) return false;

            // loop through the files
            for (int i = 0; i < inputFiles.Length; i++) {

                // copy each file
                File.Copy(inputFiles[i], dstDirectory + dstFilename + Path.GetExtension(inputFiles[i]));

            }

            // return success
            return true;

        }

        // find an image extension for a (set of) nifti file(s)
        public static string getNiftiSetExtension(string filepath) {

            // get the filepath parts
            string filename = Path.GetFileNameWithoutExtension(filepath);
            string directory = Path.GetDirectoryName(filepath);

            // check if the dest directory exist, return empty string if not
            if (!Directory.Exists(directory)) return "";

            // make sure the dstDirectory has a trailing slash
            directory = directory.TrimEnd('\\').TrimEnd('/') + Path.DirectorySeparatorChar;

            // list the files
            string[] inputFiles = Directory.GetFiles(directory, filename + "*.*");

            // check if there are no files, return empty string
            if (inputFiles.Length == 0) return "";

            // loop through the files and determine which extensions exist
            int extNII = -1;
            int extIMG = -1;
            for (int i = 0; i < inputFiles.Length; i++) {
                string extension = Path.GetExtension(inputFiles[i]);
                if (string.Compare(extension, ".nii", true) == 0) extNII = i;
                if (string.Compare(extension, ".img", true) == 0) extIMG = i;
            }

            // return nii or img
            if (extNII != -1) return Path.GetExtension(inputFiles[extNII]);
            if (extIMG != -1) return Path.GetExtension(inputFiles[extIMG]);

            // return empty string
            return "";

        }

    }

}
