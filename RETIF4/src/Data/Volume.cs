/**
 * Volume class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using RETIF4.Helpers;
using System;
using System.Diagnostics;
using static RETIF4.Nifti.NiftiImage;

namespace RETIF4.Data {

    [Serializable]
    public class Volume {
        
        public enum VolumeSource : int {
            Collected = 0,
            Generated = 1
        }

        public enum HeaderCorrection : int {
            None = 0,
            Standard_Header = 1,
            Correction_Volume = 2
        }


        // base properties
        public VolumeSource volumeSource = VolumeSource.Generated;
        public string filepath = null;                  //
        public long timeStamp = 0;                      // timestamp
        public string dateTime = "";                    // dateTime

        // processing properties
        public int volumeId = -1;                       // id (combination of the subdirectory * 10000 plus filename sequential number)
        public int volumeNr = -1;                       // filename sequential number
        public int subDirNr = -1;                       // subdirectory sequential number
        public int volumeIndexInSession = -1;
        
        public bool discarded = false;                  // whether the volume has been processed or discarded at the collector
        public bool matched = false;                    // variable to hold whether the volume in the trigger array was matched with an actual incoming file
        public int predictiveQuality = -1;              // quality of the scan information [-1 = bad, was not expected, no timing/condition information available; 0 = ad hoc created (and retrieved) timing/condition information; 1 = predicted (without any matching issues); 2 = predicted (with directory naming correction)]

        public int condition = -1;                      // experiment condition


		// auto-process properties
        public HeaderCorrection headerCorrection = HeaderCorrection.None;                   // whether the header was corrected (0 = not; 1 = to standard header; 2 = to header correction volume)
        public OrientationTransform orientDataCorrection = OrientationTransform.None;       // which orientation correction is applied to the data to bring it to the same orientation as it would have in the PAR/REC
        public OrientationTransform revOrientDataCorrection = OrientationTransform.None;    // which orientation correction is applied to the data to bring it to the orientation that came out of the scanner
        public bool realigned = false;                                                      // whether the volume is spatially realigned
        public int smoothed = 0;                                                            // whether the volume is spatially smoothed (0 = not smoothed, > 0 = FWHM applied)

        public double[] roiValues = null;                                                   // the extracted values from the ROI (not detrended)
        public double[] detrendedRoiValues = null;                                          // the extracted values from the ROI (detrended)
		
		public Phase.AutoProcessType autoProcessType = Phase.AutoProcessType.NONE;          // how the volume was auto-processed (defined by the phase the volume processing occurred in)
		public bool autoProcessSuccess = false;                                             // whether the processing was a success or failed
		public double autoProcessTime = 0.0;                                                // 
		


        // 

        public bool isGlobalMasked = false;             // whether the volume is global masked before using it's data
        public bool isROIMasked = false;                // whether the volume is ROI masked before using it's data

        //public bool isLocalizerData = false;    // 
        //public bool isBaselineData = false;     // 
        //public bool is
        //dataLocalizer       % array holding which time point are or are not localizer data
        //dataBaseline        % array holding which time points should or should not be considered in baseline calculation
        //dataPause           % array holding which time points are or are not pause data
        //dataNeurofeedback   % array holding which time points are or are not neurofeedback


        public Volume(VolumeSource volumeSource) {

            // store the volume source
            this.volumeSource = volumeSource;

            // set date/time upon initialization
            timeStamp = Stopwatch.GetTimestamp();
            dateTime = DateHelper.getDateTime();

        }

        public Volume(VolumeSource volumeSource, string filePath) {

            // store the volume source
            this.volumeSource = volumeSource;

            // set date/time upon initialization
            timeStamp = Stopwatch.GetTimestamp();
            dateTime = DateHelper.getDateTime();

            // store the given filepath
            this.filepath = filePath;

        }

        public Volume clone() {

            // create a new object
            Volume volume = new Volume(this.volumeSource);

            // clone the properties
            volume.volumeId = this.volumeId;
            volume.volumeNr = this.volumeNr;
            volume.subDirNr = this.subDirNr;
            volume.volumeIndexInSession = this.volumeIndexInSession;

            volume.filepath = this.filepath;
            volume.discarded = this.discarded;

            volume.matched = this.matched;
            volume.predictiveQuality = this.predictiveQuality;

            volume.condition = this.condition;
            volume.timeStamp = this.timeStamp;
            volume.dateTime = this.dateTime;

            volume.headerCorrection = this.headerCorrection;
            volume.orientDataCorrection = this.orientDataCorrection;
            volume.revOrientDataCorrection = this.revOrientDataCorrection;
            volume.realigned = this.realigned;
			volume.smoothed = this.smoothed;
			
            volume.roiValues = this.roiValues;
            volume.detrendedRoiValues = this.detrendedRoiValues;

			volume.autoProcessType = this.autoProcessType;
			volume.autoProcessSuccess = this.autoProcessSuccess;
			volume.autoProcessTime = this.autoProcessTime;

            volume.isGlobalMasked = this.isGlobalMasked;
            volume.isROIMasked = this.isROIMasked;

            return volume;
        }

        public static string getLineAsLineHeaders() {
            string str = "";

            str += "volumeSource" + "\t";

            str += "volumeId" + "\t";
            
            str += "volumeNr" + "\t";
            str += "subDirNr" + "\t";
            str += "volumeIndexInSession" + "\t";

            str += "timeStamp" + "\t";
            str += "dateTime" + "\t";
            str += "condition" + "\t";

            str += "filePath" + "\t";
            str += "discarded" + "\t";
            str += "matched" + "\t";
            str += "predictiveQuality" + "\t";

            str += "headerCorrection" + "\t";
            str += "orientDataCorrection" + "\t";
            str += "revOrientDataCorrection" + "\t";
			
            str += "realigned" +"\t";
            str += "smoothed" +"\t";
			
            str += "autoProcessType" + "\t";
            str += "autoProcessSuccess" + "\t";
            str += "autoProcessTime";

            return str;
			
        }

        public static Volume getVolumeFromLine(string line) {
            try {
                string[] volumeText = line.Split('\t');
                if (volumeText.Length != 20)    return null;
				//if (volumeText.Length != 17)    return null;

                Volume volume = new Volume((VolumeSource)int.Parse(volumeText[0]));
                
                volume.volumeId = int.Parse(volumeText[1]);
                volume.volumeNr = int.Parse(volumeText[2]);
                volume.subDirNr = int.Parse(volumeText[3]);
                volume.volumeIndexInSession = int.Parse(volumeText[4]);

                volume.timeStamp = long.Parse(volumeText[5]);
                volume.dateTime = volumeText[6];
                volume.condition = int.Parse(volumeText[7]);

                volume.filepath = volumeText[8];
                volume.discarded = (string.Compare(volumeText[9], "true", true) == 0 || string.Compare(volumeText[9], "1", true) == 0);
                volume.matched = (string.Compare(volumeText[10], "true", true) == 0 || string.Compare(volumeText[10], "1", true) == 0);
                volume.predictiveQuality = int.Parse(volumeText[11]);

                volume.headerCorrection = (HeaderCorrection)int.Parse(volumeText[12]);
                volume.orientDataCorrection = (OrientationTransform)int.Parse(volumeText[13]);
                volume.revOrientDataCorrection = (OrientationTransform)int.Parse(volumeText[14]);

                volume.realigned = (string.Compare(volumeText[15], "true", true) == 0 || string.Compare(volumeText[15], "1", true) == 0);
                volume.smoothed = int.Parse(volumeText[16]);

				volume.autoProcessType = (Phase.AutoProcessType)int.Parse(volumeText[17]);
				volume.autoProcessSuccess = (string.Compare(volumeText[18], "true", true) == 0 || string.Compare(volumeText[18], "1", true) == 0);
				volume.autoProcessTime = double.Parse(volumeText[19]);
				
                // return volume
                return volume;

            } catch (Exception) {   }

            // return failure
            return null;
        }

        public string getAsLine() {
            string str = "";

            str += (int)volumeSource + "\t";

            str += volumeId + "\t";
            str += volumeNr + "\t";
            str += subDirNr + "\t";
            str += volumeIndexInSession + "\t";

            str += timeStamp + "\t";
            str += dateTime + "\t";
            str += condition + "\t";

            str += filepath + "\t";
            str += discarded + "\t";
            str += matched + "\t";
            str += predictiveQuality + "\t";

            str += (int)headerCorrection + "\t";
            str += (int)orientDataCorrection + "\t";
            str += (int)revOrientDataCorrection + "\t";

            str += realigned + "\t";
            str += smoothed + "\t";

            str += (int)autoProcessType + "\t";
            str += autoProcessSuccess + "\t";
            str += autoProcessTime;

            return str;

        }

        public void clear() {

            this.volumeSource = VolumeSource.Generated;
            this.timeStamp = Stopwatch.GetTimestamp();
            this.dateTime = DateHelper.getDateTime();
            filepath = null;

            // TODO: clear the rest

        }

        public static string getVolumeGUIDisplayString(Volume volume) {
            return volume.filepath;
        }

    }

}
