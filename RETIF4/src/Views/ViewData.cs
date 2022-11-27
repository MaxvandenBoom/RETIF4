/**
 * ViewData class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace RETIF4.Views {

    public static class ViewData {

        private static Logger logger = LogManager.GetLogger("ViewData");                        // the logger object for the view

        private static string viewDirectory = null;

        private static FileStream dataStream = null;                                           // filestream that is fed to the binarywriter, containing the stream of events to be written to the .evt file
        private static StreamWriter dataStreamWriter = null;                                   // writer that writes values to the .evt file

        //private static Dictionary<string, string> trialData = null;
        private static string[] trialDataFields = null;
        private static string[] trialDataValues = null;


        public static void destroy() {

            // clear the trialData
            if (trialDataFields != null) {
                trialDataFields = null;
                trialDataValues = null;
            }

            // check if the writer exists
            if (dataStreamWriter != null) {

                // close the writer and stream
                dataStreamWriter.Close();
                dataStreamWriter = null;
                dataStream = null;

            }

        }

        public static void newRun(string name, string headerText, string[] fields) {

            // clear all data and stop all writers
            destroy();

            // initially viewdirectory is empty
            if (viewDirectory == null) {

                // check the working directory
                if (string.IsNullOrEmpty(MainThread.getWorkDirectory().Trim())) {

                    // message
                    logger.Error("No valid working directory was set (empty), no view data logging");
                    ViewData.viewDirectory = null;

                } else {

                    // build the view directory path
                    string viewDirectory = MainThread.getWorkDirectory() + "View";

                    // if the directory does not exists then create the directory
                    if (!Directory.Exists(viewDirectory)) Directory.CreateDirectory(viewDirectory);

                    // set the path
                    ViewData.viewDirectory = viewDirectory;

                }

            }

            // check if the view directory is set and valid
            if (ViewData.viewDirectory != null) {

                // get identifier and current time to use as filenames
                String fileName = name + "_" + DateTime.Now.ToString("yyyy_MM_dd__HH_mm_ss") + ".txt";
                string filePath = ViewData.viewDirectory + "\\" + fileName;
                try {

                    // create the data writer
                    dataStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    dataStreamWriter = new StreamWriter(dataStream);
                    dataStreamWriter.AutoFlush = true;                                         // ensures that after every write operation content of stream is flushed to file

                    // write the header text
                    dataStreamWriter.WriteLine(headerText);

                    // add empty lines
                    dataStreamWriter.WriteLine("");
                    dataStreamWriter.WriteLine("");

                    // initialize the arrays to store the trial data in
                    trialDataFields = new string[fields.Length];
                    trialDataValues = new string[fields.Length];
                    
                    // create the fields in the trial data dictionary and build the header for the log 
                    string trialHeader = "";
                    for (int i = 0; i < fields.Length; i++) {
                        if (trialHeader.Length != 0) trialHeader += "\t";
                        trialHeader += fields[i];

                        trialDataFields[i] = fields[i];
                        trialDataValues[i] = "";
                    }
                    
                    // write the header of the trials
                    dataStreamWriter.WriteLine(trialHeader);

                } catch (Exception) {

                    logger.Error("Could not open view data log file ('" + filePath + "'), logging not possible");

                }

            }
            
        }
        
        public static void setTrialValue(string key, string value) {

            // check if there is a valid trial data
            if (trialDataFields != null) {

                // check if the key exists
                int i = 0;
                for (i = 0; i < trialDataFields.Length; i++) {
                    if (string.Compare(trialDataFields[i], key) == 0)   break;
                }
                if (i == trialDataFields.Length) {
                    // error message
                    logger.Error("Field '" + key + "' does not exist");

                    // return without storing the value
                    return;

                }
                
                // store the value
                trialDataValues[i] = value;
                
            }

        }

        public static void saveTrial() {

            // check if there is a valid trial data
            if (trialDataFields != null) {

                // build the trial values string and clear the trial data
                string strTrial = "";
                for (int i = 0; i < trialDataFields.Length; i++) {
                    
                    // build the string
                    if (strTrial.Length != 0) strTrial += "\t";
                    strTrial += trialDataValues[i];

                    // clear the values
                    trialDataValues[i] = "";

                }

                // write the trial data
                dataStreamWriter.WriteLine(strTrial);
                
            }

        }


    }

}
