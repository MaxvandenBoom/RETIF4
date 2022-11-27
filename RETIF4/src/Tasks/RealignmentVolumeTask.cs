/**
 * RealignmentVolumeTask class
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
using System.Diagnostics;
using System.IO;
using static RETIF4.Data.Volume;

namespace RETIF4.Tasks {

    /// <summary>  
    ///  This task takes a volume and sets it as the volume to realign all future incoming volumes to
    /// </summary>  
    public class RealignmentVolumeTask : TaskBase, ITask {

        private static Logger logger = LogManager.GetLogger("RealignmentVolumeTask");
        
        private const string STD_REALIGNMENT_VOLUME_FILENAME = "realignment_volume";                                    // the corrected version of the real-time input EPI, to be used to realign future volumes to

        private string currentRealignmentVolumeDirectory = null;    // the working directory used for the current task (will be set while processing)
        private Volume currentInputVolume = null;                   // the epi volume used as input for the current task (will be set while processing)
        private Volume currentOutputVolume = null;                  // the correction volume as output for the current task (will be set while processing)

        public RealignmentVolumeTask(string workDirectory) : base(workDirectory) {

            
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
            currentRealignmentVolumeDirectory = outputDirectory + Path.DirectorySeparatorChar + "realignment_volume__" + DateHelper.getDateTime();
            Directory.CreateDirectory(currentRealignmentVolumeDirectory);

            //
            // realtime file from the scanner (input from collector)
            //

            // copy the input volume files to the realignment directory
            if (!NiftiHelper.copyNiftiFilesAndRename(currentInputVolume.filepath, currentRealignmentVolumeDirectory + Path.DirectorySeparatorChar + STD_REALIGNMENT_VOLUME_FILENAME)) {
                // error while creating copies

                // message and return
                logger.Error("Could not copy the scan input file ('" + currentInputVolume.filepath + "') to the current realignment volume directory ('" + currentRealignmentVolumeDirectory + "')");
                return;

            }

            // create a volume object based on the input volume, update the date/time on the new object and set filepath to the resulting nifti
            currentOutputVolume = currentInputVolume.clone();
            currentOutputVolume.timeStamp = Stopwatch.GetTimestamp();
            currentOutputVolume.dateTime = DateHelper.getDateTime();
            currentOutputVolume.filepath = currentRealignmentVolumeDirectory + Path.DirectorySeparatorChar + STD_REALIGNMENT_VOLUME_FILENAME + ".hdr";

            // message
            logger.Info("Copied input volume, using '" + currentOutputVolume.filepath + "' as realignment volume input");
                        
            // message
            logger.Info("finished: " + currentRealignmentVolumeDirectory);

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
            currentRealignmentVolumeDirectory = null;

            // reset the current input and output volumes
            currentInputVolume = null;
            currentOutputVolume = null;

            // flag as not finished
            finished = false;

        }

    }

}
