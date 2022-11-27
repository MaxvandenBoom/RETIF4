/**
 * LocalizerROIMaskTask class
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

namespace RETIF4.Tasks {

    /// <summary>  
	/// Produce a mask based on the GLM output, taking either a specified number of highest voxels, or a percentage of highest voxels (after applying a global ROI mask).
	/// The output mask can then be used to perform more precise voxel selection for neurofeedback
    /// </summary> 
    public class LocalizerROIMaskTask : TaskBase, ITask {

        private static Logger logger = LogManager.GetLogger("LocalizerROIMaskTask");

        private const string STD_NATIVE_ROI_MASK_FILENAME = "native_roi_mask";

        private bool usePercentage = false;                         // flag whether to use an absolute number of voxels or a percentage
        private int numberOfHighestVoxels = 80;                     // the number of highest voxels to take from each t-map
        private double percentageOfHighestVoxels = 0.33;            // the percentage of highest voxels to take from each t-map
        

        private string currentMaskDirectory = null;                 // the working directory used for the current task (will be set while processing)
        private Volume currentOutputVolume = null;                  // the native mask volume as output for the current task (will be set while processing)


        public LocalizerROIMaskTask(string workDirectory, int numberOfHighestVoxels) : base(workDirectory) {

            // transfer the number of highest voxels
            if (numberOfHighestVoxels > 0)
                this.numberOfHighestVoxels = numberOfHighestVoxels;
            else {
                // message
                logger.Error("Invalid argument for number of highest voxels");
            }


            // use an absolute number of voxels
            usePercentage = false;

        }

        public LocalizerROIMaskTask(string workDirectory, double percentageOfHighestVoxels) : base(workDirectory) {

            // transfer the number of highest voxels
            if (percentageOfHighestVoxels > 0 && percentageOfHighestVoxels <= 1)
                this.percentageOfHighestVoxels = percentageOfHighestVoxels;
            else {
                // message
                logger.Error("Invalid argument for percentage of highest voxels");
            }

            // use a percentage
            usePercentage = true;
        }

        public void run(List<Volume> inputVolumes, Volume globalMask) {

            // flag the task as not finished
            finished = false;

            // check if there are enough volumes
            if (inputVolumes.Count == 0) {

                // message
                logger.Error("No input volumes");

                // return
                return;

            }


            //
            // file management
            //

            // build a new folder for the current task and create the folder
            currentMaskDirectory = outputDirectory + Path.DirectorySeparatorChar + "localizer__roi__" + DateHelper.getDateTime();
            Directory.CreateDirectory(currentMaskDirectory);
            
            // copy the spm input files to the current mask directory and update the inputvolumes
            for (int i = 0; i < inputVolumes.Count; i++) {
                NiftiHelper.copyNiftiFilesToDirectory(inputVolumes[i].filepath, currentMaskDirectory);
                inputVolumes[i].filepath = currentMaskDirectory + Path.DirectorySeparatorChar + Path.GetFileName(inputVolumes[i].filepath);
            }


            //
            // matlab calls
            //

            // list the t-maps to use
            MatlabWrapper.sendCommand("tmaps = {};");
            for (int i = 0; i < inputVolumes.Count; i++) {
                MatlabWrapper.sendCommand("volume = spm_vol('" + inputVolumes[i].filepath + "');");
                MatlabWrapper.sendCommand("volumeData = spm_read_vols(volume);");
                MatlabWrapper.sendCommand("tmaps{" + (i + 1) + "} = volumeData;");
                MatlabWrapper.sendCommand("clear volumeData volume;");
            }

            // check if there is a global mask that should be applied first to the t-maps
            if (globalMask != null) {
                
                // TODO: create a local copy of the global mask


                // apply the global mask to the t-maps
                MatlabWrapper.sendCommand("volume = spm_vol('" + globalMask.filepath + "');");
                MatlabWrapper.sendCommand("globalData = spm_read_vols(volume);");
                for (int i = 0; i < inputVolumes.Count; i++) {
                    MatlabWrapper.sendCommand("tmaps{" + (i + 1) + "} (globalData == 0) = 0;");
                }
                MatlabWrapper.sendCommand("clear globalData volume;");
            }

            // check if absolute number of voxels or a percentage should be used
            if (usePercentage) {
                // percentage of voxels

                // create a union mask from the ... percentage of highest voxels in each t-map 
                MatlabWrapper.sendCommand("unionMask = createUnionMask(tmaps, (" + percentageOfHighestVoxels + " * 100), 0, 1);");

            } else {
                // absolute number of voxels

                // create a union mask from the ... number of highest voxels in each t-map 
                MatlabWrapper.sendCommand("unionMask = createUnionMask(tmaps, " + numberOfHighestVoxels + ", 0, 0);");

            }

            // build the path to the future roi mask
            string roiMaskFilepath = currentMaskDirectory + Path.DirectorySeparatorChar + STD_NATIVE_ROI_MASK_FILENAME + Path.GetExtension(inputVolumes[0].filepath);

            // create a copy of the first file, this will become the roi mask
            if (!NiftiHelper.copyNiftiFilesAndRename(inputVolumes[0].filepath, roiMaskFilepath)) {
                // error while creating copies

                // message and return
                logger.Error("Could not create a copy of the first input file ('" + inputVolumes[0].filepath + "') to the current mask directory ('" + currentMaskDirectory + "')");
                return;

            }

            // store the union mask in the roi mask nifti
            MatlabWrapper.sendCommand("volume = spm_vol('" + roiMaskFilepath + "');");
            MatlabWrapper.sendCommand("spm_write_vol(volume, unionMask);");
            MatlabWrapper.sendCommand("clear volume;");

            // create a volume to serve as output volume 
            currentOutputVolume = new Volume(Volume.VolumeSource.Generated);
            currentOutputVolume.filepath = roiMaskFilepath;

            // base the correction/realignment properties
            currentOutputVolume.headerCorrection = inputVolumes[0].headerCorrection;
            currentOutputVolume.orientDataCorrection = inputVolumes[0].orientDataCorrection;
            currentOutputVolume.revOrientDataCorrection = inputVolumes[0].revOrientDataCorrection;
            currentOutputVolume.realigned = inputVolumes[0].realigned;

            // message
            logger.Info("finished " + currentMaskDirectory);

            // flag the task as finished
            finished = true;

        }

        public Volume getOutputMask() {

            // check if the task has not finished yet
            if (!finished) {

                logger.Error("Trying to retrieve the ROI mask before the task has finished, returning null");

                // return failure
                return null;

            }

            // return the native mask
            return currentOutputVolume;
        }

    }

}
