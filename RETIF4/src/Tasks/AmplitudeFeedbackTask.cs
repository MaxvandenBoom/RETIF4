/**
 * AmplitudeFeedbackTask class
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
using System.Threading;

namespace RETIF4.Tasks {

    /**
     * Feedback task where an on-the-fly baseline is established (using the rest before each trial) to give feedback against
     **/
    public class AmplitudeFeedbackTask : TaskBase, ITask {

        private static Logger logger = LogManager.GetLogger("AmplitudeFeedbackTask");
        
        private Object lockMatlab = new Object();
        private string currentfeedbackDirectory = null;
        private bool initialized = false;

        private RingBuffer samples = null;
        private double latestSmoothedAverage = 0;

        public AmplitudeFeedbackTask(string workDirectory) : base(workDirectory) {

        }

        public void initialize() {

            // flag as not initialized
            initialized = false;
            
            //
            // file management
            //

            // build a new folder for the current task and create the folder
            currentfeedbackDirectory = outputDirectory + Path.DirectorySeparatorChar + "feedback__" + DateHelper.getDateTime();
            Directory.CreateDirectory(currentfeedbackDirectory);

            
            
            
            // initialize the sample buffer
            samples = new RingBuffer(4);

            // reset the average
            latestSmoothedAverage = 0;

            // flag as initialized
            initialized = true;
            
        }


        public bool isInitialized() {
            return initialized;
        }
        
        public void processVolume(Volume volume) {

            
            // copy the input volume files to the feedback directory
            if (!NiftiHelper.copyNiftiFilesToDirectory(volume.filepath, currentfeedbackDirectory)) {
                // error while creating copies

                // message and return
                logger.Error("Could not copy the scan input file ('" + volume.filepath + "') to the current feedback directory ('" + currentfeedbackDirectory + "')");
                return;

            }
            
            double meanVal = volume.roiValues.Average();
            //Console.WriteLine("processVolume " + volume.volumeId + "    - " + meanVal);

            // add the volume mean
            samples.Put(meanVal);
            //Console.WriteLine("- added: " + meanVal);

            // calculate the average
            if (samples.IsFull()) {

                // smooth over last part (x samples) of the buffer
                double[] sampleBufferSequential = samples.DataSequential();
                //Console.WriteLine("- sequential: " + String.Join(" - ", sampleBufferSequential));
                double[] sampleBufferSequentialSubset = new double[3];      // take the last 3 samples
                Array.Copy(sampleBufferSequential, sampleBufferSequential.Length - sampleBufferSequentialSubset.Length, sampleBufferSequentialSubset, 0, sampleBufferSequentialSubset.Length);
                //Console.WriteLine("- sequential subset: " + String.Join(" - ", sampleBufferSequentialSubset));
                latestSmoothedAverage = sampleBufferSequentialSubset.Average();
                
                // smooth over full buffer (4?)
                //latestSmoothedAverage = samples.Data().Average();

            } else {
                latestSmoothedAverage = samples.DataSequential().Average();
            }

            // message the end of processing
            Console.WriteLine("Processed volume " + volume.volumeId + ", feedback value " + meanVal);

        }


        /**
         * Callback function, called at the start of each trial in the view
         * 
         * Initiated by the View, forwarded to the MainThread, forwarded to the experiment, which forwards it here
         **/
        public void processTrialStart(double condition) {
            
        }

        public double getFeedbackValue() {

            // return the smoothed average
            return latestSmoothedAverage;
            
        }


        public double[] getFeedbackValues() {

            // return the buffer
            if (samples.IsFull()) {
                return samples.Data();
            } else {
                return new double[] { 0 };
            }

        }


    }

}
