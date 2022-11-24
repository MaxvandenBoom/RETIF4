/**
 * Session class
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
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RETIF4.Data {

    public static class Session {
        private static Logger logger = LogManager.GetLogger("Session");  // 

        private static Object sessionLock = new Object();                   // lock object for thread safety
        private static List<Volume> allVolumes = new List<Volume>();        // all the volumes that were ever processed (so exclusive of discarded volumes)

        // stores volumes belonging to a task (for online analysis)
        private static Dictionary<string, List<Volume>> taskVolumes = new Dictionary<string, List<Volume>>();

        // stores trials belonging to a task (for online analysis)
        private static Dictionary<string, List<Block>> taskTrials = new Dictionary<string, List<Block>>();

        // reserved volumes
        private static Volume correctionVolume = null;
        private static Volume realignmentVolume = null;
        private static Volume globalMask = null;
        private static Volume globalRTMask = null;
        private static Volume roiMask = null;
        



        //////
        /// Sessions
        //////

        public static void newSession() {
            
            // thread safety
            lock (sessionLock) {

                // clear the task volumes
                foreach (KeyValuePair<string, List<Volume>> entry in taskVolumes) {
                    for (int i = 0; i < entry.Value.Count; i++) entry.Value[i].clear();
                    entry.Value.Clear();
                }
                taskVolumes.Clear();

                // clear all volumes
                for (int i = 0; i < allVolumes.Count; i++) allVolumes[i].clear();
                Session.allVolumes.Clear();

                // clear correction volume
                if (Session.correctionVolume != null) {
                    Session.correctionVolume.clear();
                    Session.correctionVolume = null;
                }

                // clear realign volume
                if (Session.realignmentVolume != null) {
                    Session.realignmentVolume.clear();
                    Session.realignmentVolume = null;
                }

                // clear global mask
                if (Session.globalMask != null) {
                    Session.globalMask.clear();
                    Session.globalMask = null;
                }

                // clear global RT mask
                if (Session.globalRTMask != null) {
                    Session.globalRTMask.clear();
                    Session.globalRTMask = null;
                }

                // clear roi mask
                if (Session.roiMask != null) {
                    Session.roiMask.clear();
                    Session.roiMask = null;
                }

            }

        }

        /**
         *  List the variable names in a session file
         *   
         *  filepath    - the file to list the variables from. The file can be either binary (.dat) or text (any other file extention)
         *  listVolumes - list the volume variables
         *  listBlocks  - list the block/trial variables
         **/
        public static string[] listSessionVariableNamesFromFile(string filepath, bool listVolumes, bool listBlocks) {

            StreamReader stream = null;
            try {

                string line = "";
                bool readTaskVolumes = false;
                bool readTaskTrials = false;
                List<string> sessionVariables = new List<string>();
                BinaryFormatter b = new BinaryFormatter();

                // check if it is a binary file
                bool fileBinary = String.Compare(filepath.Substring(filepath.Length - 4).ToLower(), ".dat") == 0;

                // open the file
                stream = new StreamReader(filepath);

                // trace through the file
                while ( (fileBinary && stream.BaseStream.Position < stream.BaseStream.Length) ||
                        (!fileBinary && (line = stream.ReadLine()) != null)                      ) {
                    if (fileBinary)     line = (String)b.Deserialize(stream.BaseStream);

                    // check if the taskVolumes node is starting or ending
                    if (string.Compare(line, "<taskVolumes>") == 0)     { readTaskVolumes = true; continue; }
                    if (string.Compare(line, "<\\taskVolumes>") == 0)   { readTaskVolumes = false; continue; }

                    // check if the taskVolumes node is starting or ending
                    if (string.Compare(line, "<taskTrials>") == 0)      { readTaskTrials = true; continue; }
                    if (string.Compare(line, "<\\taskTrials>") == 0)    { readTaskTrials = false; continue; }

                    // check if we are reading task volume (within the taskVolumes node)
                    if (readTaskVolumes) {
                        // within taskVolumes node

                        // check for a start node and add
                        if (line.StartsWith("<") && !line.StartsWith("<\\")) {

                            // check whether to list volume variables
                            if (listVolumes) {

                                // strip the name
                                string varName = line.Substring(1, line.Length - 2);
                                if (varName.EndsWith(" type=\"volumes\"")) {
                                    varName = varName.Substring(0, varName.Length - 15);
                                } else if (varName.EndsWith(" type=\"blocks\"")) {
                                    varName = varName.Substring(0, varName.Length - 14);
                                }
                                
                                // list the name
                                sessionVariables.Add(varName);

                            }
                            if (fileBinary) {
                                List<Volume> volumes = (List<Volume>)b.Deserialize(stream.BaseStream);
                            }
                        }

                    } else if (readTaskTrials) {
                        // within taskTrials node

                        // check for a start node and add
                        if (line.StartsWith("<") && !line.StartsWith("<\\")) {

                            // check whether to list block variables
                            if (listBlocks) {

                                // strip the name
                                string varName = line.Substring(1, line.Length - 2);
                                if (varName.EndsWith(" type=\"volumes\"")) {
                                    varName = varName.Substring(0, varName.Length - 15);
                                } else if (varName.EndsWith(" type=\"blocks\"")) {
                                    varName = varName.Substring(0, varName.Length - 14);
                                }

                                // list the name
                                sessionVariables.Add(varName);

                            }
                            if (fileBinary) {
                                List<Block> blocks = (List<Block>)b.Deserialize(stream.BaseStream);
                            }
                        }

                    } else {
                        // not reading task volumes (outside of taskVolumes node)

                        if (string.Compare(line, "<allVolumes>") == 0 || string.Compare(line, "<allVolumes type=\"volumes\">") == 0) {
                            if (listVolumes)    sessionVariables.Add("allVolumes");
                            if (fileBinary) {
                                List<Volume> volume = (List<Volume>)b.Deserialize(stream.BaseStream);
                            }
                        }
                        if (string.Compare(line, "<corrVolume>") == 0 || string.Compare(line, "<corrVolume type=\"volumes\">") == 0) {
                            if (listVolumes)    sessionVariables.Add("corrVolume");
                            if (fileBinary) {
                                Volume volume = (Volume)b.Deserialize(stream.BaseStream);
                            }
                        }
                        if (string.Compare(line, "<realignVolume>") == 0 || string.Compare(line, "<realignVolume type=\"volumes\">") == 0) {
                            if (listVolumes)    sessionVariables.Add("realignVolume");
                            if (fileBinary) {
                                Volume volume = (Volume)b.Deserialize(stream.BaseStream);
                            }
                        }
                        if (string.Compare(line, "<globalMask>") == 0 || string.Compare(line, "<globalMask type=\"volumes\">") == 0) {
                            if (listVolumes)    sessionVariables.Add("globalMask");
                            if (fileBinary) {
                                Volume volume = (Volume)b.Deserialize(stream.BaseStream);
                            }
                        }
                        if (string.Compare(line, "<globalRTMask>") == 0 || string.Compare(line, "<globalRTMask type=\"volumes\">") == 0) {
                            if (listVolumes)    sessionVariables.Add("globalRTMask");
                            if (fileBinary) {
                                Volume volume = (Volume)b.Deserialize(stream.BaseStream);
                            }
                        }
                        if (string.Compare(line, "<roiMask>") == 0 || string.Compare(line, "<roiMask type=\"volumes\">") == 0) {
                            if (listVolumes)    sessionVariables.Add("roiMask");
                            if (fileBinary) {
                                Volume volume = (Volume)b.Deserialize(stream.BaseStream);
                            }
                        }
                        
                    }

                }
    
                // close the file
                stream.Close();
                stream = null;

                // return the variables as an array
                return sessionVariables.ToArray();

            } catch (Exception) {
                
                // close the file
                if (stream != null) {
                    stream.Close();
                    stream = null;
                }

                // return failure
                return null;
            }
        }

        /**
         *  Read a session volume variable from the file
         *  (does not store it in the session, just returns a list of volumes)
         *  
         **/
        public static Dictionary<string, List<Volume>> readSessionVolumeVariableFromFile(string filepath, string[] variableNames) {
            if (variableNames == null || variableNames.Length == 0 || String.IsNullOrEmpty(filepath))
                return new Dictionary<string, List<Volume>>();

            // thread safety
            lock (sessionLock) {

                // create a list the return lists of volumes in
                Dictionary<string, List<Volume>> arrList = new Dictionary<string, List<Volume>>();
                for (int i = 0; i < variableNames.Length; i++)
                    arrList.Add(variableNames[i], null);

                // 
                StreamReader stream = null;
                try {

                    // 
                    string line = "";
                    bool readingVariable = false;
                    bool readingVariableTypeVolumes = false;
                    bool readingVariableTypeBlocks = false;
                    string readingVariableName = "";
                    bool firstLine = false;
                    BinaryFormatter b = new BinaryFormatter();

                    // check if it is a binary file
                    bool fileBinary = String.Compare(filepath.Substring(filepath.Length - 4).ToLower(), ".dat") == 0;
                    
                    // open the file
                    stream = new StreamReader(filepath);

                    // trace through the file
                    while ((fileBinary && stream.BaseStream.Position < stream.BaseStream.Length) ||
                            (!fileBinary && (line = stream.ReadLine()) != null)) {
                        if (fileBinary) line = (String)b.Deserialize(stream.BaseStream);

                        // check if we are trying to read lines
                        if (readingVariable) {
                            // interpret the lines after the start of a variable
                            
                            // try to find the end of the variable
                            if (line.StartsWith("<\\")) {
                                readingVariable = false;
                                continue;
                            }

                            // 
                            if (!fileBinary) {

                                // skip the first line of headers
                                if (!firstLine) {
                                    firstLine = true;
                                } else {

                                    // try to create a volume and add
                                    Volume volume = Volume.getVolumeFromLine(line);
                                    if (volume == null) {
                                        logger.Error("Unable to convert line '" + line + "' from the variable '" + readingVariableName + "' (file: '" + filepath + "') to a volume object, skipping");
                                    } else {
                                        arrList[readingVariableName].Add(volume);
                                    }

                                }
                            }
                            

                        } else {
                            // just interpret the line (not reading a variable yet)

                            // check for a start node
                            if (line.StartsWith("<") && !line.StartsWith("<\\")) {

                                // determine the variable name
                                readingVariableName = line.Substring(1, line.Length - 2);

                                // if these are reserved tags, then continue
                                if (readingVariableName.Equals("taskVolumes") || readingVariableName.Equals("taskTrials"))
                                    continue;

                                
                                // determine whether these are volumes (or blocks)
                                readingVariableTypeVolumes = true;
                                if (readingVariableName.EndsWith(" type=\"volumes\"")) {
                                    readingVariableName = readingVariableName.Substring(0, readingVariableName.Length - 15);
                                }
                                if (readingVariableName.EndsWith(" type=\"blocks\"")) {
                                    readingVariableTypeVolumes = false;
                                    readingVariableTypeBlocks = true;
                                    readingVariableName = readingVariableName.Substring(0, readingVariableName.Length - 14);
                                }
                                
                                // retrieve the volume information if binary
                                List<Volume> volumes = null;
                                if (fileBinary) {

                                    // check if the variable holds blocks or volumes
                                    if (readingVariableTypeBlocks) {
                                        // blocks

                                        // read and discard
                                        List<Block> blocks = (List<Block>)b.Deserialize(stream.BaseStream);

                                    } else {
                                        // volumes
                                    
                                        // check if these are reserved volumes
                                        if (readingVariableName.Equals("corrVolume") || readingVariableName.Equals("realignVolume") || readingVariableName.Equals("globalMask") ||
                                            readingVariableName.Equals("globalRTMask") || readingVariableName.Equals("roiMask") ) {
                                            // single volume

                                            Volume volume = (Volume)b.Deserialize(stream.BaseStream);
                                            volumes = new List<Volume>();
                                            volumes.Add(volume);
                                                                                
                                        } else {
                                            // list of volumes

                                            volumes = (List<Volume>)b.Deserialize(stream.BaseStream);

                                        }

                                    }

                                }

                                // determine whether the variable needs to be returned (requested and volumes)
                                int varIndex = Array.IndexOf(variableNames, readingVariableName);
                                if (readingVariableTypeVolumes && varIndex != -1) {

                                    // start to read a new volume range
                                    readingVariable = true;
                                    firstLine = false;

                                    // store the (already retrieved) information if binary
                                    if (fileBinary) {
                                        arrList[readingVariableName] = volumes;
                                    } else {

                                        arrList[readingVariableName] = new List<Volume>();

                                    }
                                    
                                }

                            }

                        }
                        
                    }

                    // close the file
                    stream.Close();
                    stream = null;

                    // return the volumes
                    return arrList;

                } catch (Exception) {

                    // close the file
                    if (stream != null) {
                        stream.Close();
                        stream = null;
                    }

                    // return failure
                    return null;

                }

            }   // end lock

        }

        public static Dictionary<string, List<Block>> readSessionTrialsVariableFromFile(string filepath, string[] variableNames) {
            if (variableNames == null || variableNames.Length == 0 || String.IsNullOrEmpty(filepath))
                return new Dictionary<string, List<Block>>();

            // thread safety
            lock (sessionLock) {

                // create a list the return lists of blocks in
                Dictionary<string, List<Block>> arrList = new Dictionary<string, List<Block>>();
                for (int i = 0; i < variableNames.Length; i++)
                    arrList.Add(variableNames[i], null);

                // 
                StreamReader stream = null;
                try {

                    // 
                    string line = "";
                    bool readingVariable = false;
                    bool readingVariableTypeBlocks = false;
                    string readingVariableName = "";
                    bool firstLine = false;
                    BinaryFormatter b = new BinaryFormatter();

                    // check if it is a binary file
                    bool fileBinary = String.Compare(filepath.Substring(filepath.Length - 4).ToLower(), ".dat") == 0;

                    // open the file
                    stream = new StreamReader(filepath);

                    // trace through the file
                    while ((fileBinary && stream.BaseStream.Position < stream.BaseStream.Length) ||
                            (!fileBinary && (line = stream.ReadLine()) != null)) {
                        if (fileBinary) line = (String)b.Deserialize(stream.BaseStream);

                        // check if we are trying to read lines
                        if (readingVariable) {
                            // interpret the lines after the start of a variable

                            // try to find the end of the variable
                            if (line.StartsWith("<\\")) {
                                readingVariable = false;
                                continue;
                            }

                            // 
                            if (!fileBinary) {

                                // skip the first line of headers
                                if (!firstLine) {
                                    firstLine = true;
                                } else {

                                    // try to create a block and add
                                    Block block = Block.getBlockFromLine(line);
                                    if (block == null) {
                                        logger.Error("Unable to convert line '" + line + "' from the variable '" + readingVariableName + "' (file: '" + filepath + "') to a block object, skipping");
                                    } else {
                                        arrList[readingVariableName].Add(block);
                                    }

                                }
                            }


                        } else {
                            // just interpret the line (not reading a variable yet)

                            // check for a start node
                            if (line.StartsWith("<") && !line.StartsWith("<\\")) {

                                // determine the variable name
                                readingVariableName = line.Substring(1, line.Length - 2);

                                // if these are reserved tags, then continue
                                if (readingVariableName.Equals("taskVolumes") || readingVariableName.Equals("taskTrials"))
                                    continue;


                                // determine whether these are volumes (or blocks)
                                if (readingVariableName.EndsWith(" type=\"volumes\"")) {
                                    readingVariableName = readingVariableName.Substring(0, readingVariableName.Length - 15);
                                }
                                if (readingVariableName.EndsWith(" type=\"blocks\"")) {
                                    readingVariableTypeBlocks = true;
                                    readingVariableName = readingVariableName.Substring(0, readingVariableName.Length - 14);
                                }

                                // retrieve the volume information if binary
                                List<Block> blocks = null;
                                if (fileBinary) {

                                    // check if the variable holds blocks or volumes
                                    if (readingVariableTypeBlocks) {
                                        // blocks

                                        blocks = (List<Block>)b.Deserialize(stream.BaseStream);

                                    } else {
                                        // volumes

                                        // check if these are reserved volumes
                                        if (readingVariableName.Equals("corrVolume") || readingVariableName.Equals("realignVolume") || readingVariableName.Equals("globalMask") ||
                                            readingVariableName.Equals("globalRTMask") || readingVariableName.Equals("roiMask")) {
                                            // single volume

                                            // read and discard
                                            Volume volume = (Volume)b.Deserialize(stream.BaseStream);
                                            
                                        } else {
                                            // list of volumes

                                            // read and discard
                                            List<Volume> volumes = (List<Volume>)b.Deserialize(stream.BaseStream);

                                        }

                                    }

                                }

                                // determine whether the variable needs to be returned (requested and blocks)
                                int varIndex = Array.IndexOf(variableNames, readingVariableName);
                                if (readingVariableTypeBlocks && varIndex != -1) {

                                    // start to read a new block range
                                    readingVariable = true;
                                    firstLine = false;

                                    // store the (already retrieved) information if binary
                                    if (fileBinary) {
                                        arrList[readingVariableName] = blocks;
                                    } else {

                                        arrList[readingVariableName] = new List<Block>();

                                    }

                                }

                            }

                        }

                    }

                    // close the file
                    stream.Close();
                    stream = null;

                    // return the volumes
                    return arrList;

                } catch (Exception) {

                    // close the file
                    if (stream != null) {
                        stream.Close();
                        stream = null;
                    }

                    // return failure
                    return null;

                }

            }   // end lock

        }

        public static void loadSession(string filePath) {

            // create a new session, basically clearing out the old session
            newSession();

            // thread safety
            lock (sessionLock) {


            }

        }


        public static void saveSessionBinary(string filePath) {

            // thread safety
            lock (sessionLock) {
                
                // open a stream to write a binary file
                StreamWriter stream = new StreamWriter(filePath);
                BinaryFormatter binFormatter = new BinaryFormatter();

                // check if there are volumes in the allVolumes variable
                if (allVolumes.Count != 0) {
                    binFormatter.Serialize(stream.BaseStream, "<allVolumes type=\"volumes\">");
                    binFormatter.Serialize(stream.BaseStream, allVolumes);
                    binFormatter.Serialize(stream.BaseStream, "<\\allVolumes>");
                }

                // check if there are taskVolume sets
                if (taskVolumes.Count != 0) {

                    // loop through the lists with taskVolumes
                    binFormatter.Serialize(stream.BaseStream, "<taskVolumes>");
                    foreach (KeyValuePair<string, List<Volume>> entry in taskVolumes) {
                        if (entry.Value != null) {
                            binFormatter.Serialize(stream.BaseStream, "<" + entry.Key + " type=\"volumes\">");
                            binFormatter.Serialize(stream.BaseStream, entry.Value);
                            binFormatter.Serialize(stream.BaseStream, "<\\" + entry.Key + ">");
                        }
                    }
                    binFormatter.Serialize(stream.BaseStream, "<\\taskVolumes>");

                }

                //
                if (correctionVolume != null) {
                    binFormatter.Serialize(stream.BaseStream, "<corrVolume type=\"volumes\">");
                    binFormatter.Serialize(stream.BaseStream, correctionVolume);
                    binFormatter.Serialize(stream.BaseStream, "<\\corrVolume>");
                }

                //
                if (realignmentVolume != null) {
                    binFormatter.Serialize(stream.BaseStream, "<realignVolume type=\"volumes\">");
                    binFormatter.Serialize(stream.BaseStream, realignmentVolume);
                    binFormatter.Serialize(stream.BaseStream, "<\\realignVolume>");
                }

                //
                if (globalMask != null) {
                    binFormatter.Serialize(stream.BaseStream, "<globalMask type=\"volumes\">");
                    binFormatter.Serialize(stream.BaseStream, globalMask);
                    binFormatter.Serialize(stream.BaseStream, "<\\globalMask>");
                }

                //
                if (globalRTMask != null) {
                    binFormatter.Serialize(stream.BaseStream, "<globalRTMask type=\"volumes\">");
                    binFormatter.Serialize(stream.BaseStream, globalRTMask);
                    binFormatter.Serialize(stream.BaseStream, "<\\globalRTMask>");
                }

                //
                if (roiMask != null) {
                    binFormatter.Serialize(stream.BaseStream, "<roiMask type=\"volumes\">");
                    binFormatter.Serialize(stream.BaseStream, roiMask);
                    binFormatter.Serialize(stream.BaseStream, "<\\roiMask>");
                }

                // check if there are taskTrial sets
                if (taskTrials.Count != 0) {

                    // loop through the lists with taskTrials
                    binFormatter.Serialize(stream.BaseStream, "<taskTrials>");
                    foreach (KeyValuePair<string, List<Block>> entry in taskTrials) {
                        if (entry.Value != null) {
                            binFormatter.Serialize(stream.BaseStream, "<" + entry.Key + " type=\"blocks\">");
                            binFormatter.Serialize(stream.BaseStream, entry.Value);
                            binFormatter.Serialize(stream.BaseStream, "<\\" + entry.Key + ">");
                        }
                    }
                    binFormatter.Serialize(stream.BaseStream, "<\\taskTrials>");

                }

                // close the stream
                stream.Close();
                

            }

        }

        public static void saveSession(string filePath) {

            // thread safety
            lock (sessionLock) {

                StreamWriter stream = null;
                try {

                    // Write the string to a file.
                    stream = new StreamWriter(filePath);

                    // check if there are volumes in the allVolumes variable
                    if (allVolumes.Count != 0) {

                        stream.WriteLine("<allVolumes type=\"volumes\">");
                        stream.WriteLine(Volume.getLineAsLineHeaders());
                        for (int i = 0; i < allVolumes.Count; i++) {
                            stream.WriteLine(allVolumes[i].getAsLine());
                        }
                        stream.WriteLine("<\\allVolumes>");
                        stream.WriteLine("");

                    }
                    
                    // check if there are taskVolume sets
                    if (taskVolumes.Count != 0) {

                        // loop through the lists with taskVolumes
                        stream.WriteLine("<taskVolumes>");
                        foreach (KeyValuePair<string, List<Volume>> entry in taskVolumes) {
                            if (entry.Value != null) {
                                stream.WriteLine("<" + entry.Key + " type=\"volumes\">");
                                stream.WriteLine(Volume.getLineAsLineHeaders());
                                for (int i = 0; i < entry.Value.Count; i++) {
                                    stream.WriteLine(entry.Value[i].getAsLine());
                                }
                                stream.WriteLine("<\\" + entry.Key + ">");
                            }
                        }
                        stream.WriteLine("<\\taskVolumes>");
                        stream.WriteLine("");

                    }

                    //
                    if (correctionVolume != null) {
                        stream.WriteLine("<corrVolume type=\"volumes\">");
                        stream.WriteLine(Volume.getLineAsLineHeaders());
                        stream.WriteLine(correctionVolume.getAsLine());
                        stream.WriteLine("<\\corrVolume>");
                        stream.WriteLine("");
                    }

                    //
                    if (realignmentVolume != null) {
                        stream.WriteLine("<realignVolume type=\"volumes\">");
                        stream.WriteLine(Volume.getLineAsLineHeaders());
                        stream.WriteLine(realignmentVolume.getAsLine());
                        stream.WriteLine("<\\realignVolume>");
                        stream.WriteLine("");
                    }

                    //
                    if (globalMask != null) {
                        stream.WriteLine("<globalMask type=\"volumes\">");
                        stream.WriteLine(Volume.getLineAsLineHeaders());
                        stream.WriteLine(globalMask.getAsLine());
                        stream.WriteLine("<\\globalMask>");
                        stream.WriteLine("");
                    }

                    //
                    if (globalRTMask != null) {
                        stream.WriteLine("<globalRTMask type=\"volumes\">");
                        stream.WriteLine(Volume.getLineAsLineHeaders());
                        stream.WriteLine(globalRTMask.getAsLine());
                        stream.WriteLine("<\\globalRTMask>");
                        stream.WriteLine("");
                    }

                    //
                    if (roiMask != null) {
                        stream.WriteLine("<roiMask type=\"volumes\">");
                        stream.WriteLine(Volume.getLineAsLineHeaders());
                        stream.WriteLine(roiMask.getAsLine());
                        stream.WriteLine("<\\roiMask>");
                        stream.WriteLine("");
                    }
                    
                    // check if there are taskTrial sets
                    if (taskTrials.Count != 0) {

                        // loop through the lists with taskTrials
                        stream.WriteLine("<taskTrials>");
                        foreach (KeyValuePair<string, List<Block>> entry in taskTrials) {
                            if (entry.Value != null) {
                                stream.WriteLine("<" + entry.Key + " type=\"blocks\">");
                                stream.WriteLine(Block.getLineAsLineHeaders());
                                for (int i = 0; i < entry.Value.Count; i++) {
                                    stream.WriteLine(entry.Value[i].getAsLine());
                                }
                                stream.WriteLine("<\\" + entry.Key + ">");
                            }
                        }
                        stream.WriteLine("<\\taskTrials>");
                        stream.WriteLine("");

                    }
                    // close the file
                    stream.Close();
                    stream = null;

                } catch (Exception) {

                    // close the file
                    if (stream != null) {
                        stream.Close();
                        stream = null;
                    }

                }
            }

        }




        /////
        ///     volumes
        /////

        public static void addVolume(Volume volume) {

            // thread safety
            lock (sessionLock) {

                // add to the list of volumes
                Session.allVolumes.Add(volume);

                // store the volume index in this session
                volume.volumeIndexInSession = Session.allVolumes.Count - 1;
            }

        }

        public static Volume getCorrectionVolume() {
            lock (sessionLock) {
                return Session.correctionVolume;
            }
        }

        public static void setCorrectionVolume(Volume correctionVolume) {
            lock (sessionLock) {
                Session.correctionVolume = correctionVolume;
            }
        }

        public static Volume getRealignmentVolume() {
            lock (sessionLock) {
                return Session.realignmentVolume;
            }
        }

        public static void setRealignmentVolume(Volume realignmentVolume) {
            lock (sessionLock) {
                Session.realignmentVolume = realignmentVolume;
            }
        }

        public static Volume getGlobalMask() {
            lock (sessionLock) {
                return Session.globalMask;
            }
        }

        public static Volume getGlobalRTMask() {
            lock (sessionLock) {
                return Session.globalRTMask;
            }
        }

        public static void setGlobalMask(Volume globalMask) {
            lock (sessionLock) {
                Session.globalMask = globalMask;
            }
        }

        public static void setGlobalRTMask(Volume globalRTMask) {
            lock (sessionLock) {
                Session.globalRTMask = globalRTMask;
            }
        }

        public static Volume getRoiMask() {
            lock (sessionLock) {
                return Session.roiMask;
            }
        }

        public static void setRoiMask(Volume roiMask) {
            lock (sessionLock) {
                Session.roiMask = roiMask;
            }
        }


        public static int getNumberOfVolumes(string name) {

            // check which volume to return
            if (string.Compare(name, "allVolumes", true) == 0) {

                lock (sessionLock) {
                    return allVolumes.Count;
                }

            } else if (string.Compare(name, "corrVolume", true) == 0) {

                lock (sessionLock) {
                    return (correctionVolume == null ? 0 : 1);
                }

            } else if (string.Compare(name, "realignVolume", true) == 0) {

                lock (sessionLock) {
                    return (realignmentVolume == null ? 0 : 1);
                }

            } else if (string.Compare(name, "globalMask", true) == 0) {

                lock (sessionLock) {
                    return (globalMask == null ? 0 : 1);
                }

            } else if (string.Compare(name, "globalRTMask", true) == 0) {

                lock (sessionLock) {
                    return (globalRTMask == null ? 0 : 1);
                }

            } else if (string.Compare(name, "roiMask", true) == 0) {

                lock (sessionLock) {
                    return (roiMask == null ? 0 : 1);
                }

            } else {
                // try as task (or return 0)

                return getNumberOfTaskVolumes(name);

            }

        }

        public static List<Volume> getVolumes(string name) {


            // check which volume to return
            if (string.Compare(name, "allVolumes", true) == 0) {

                lock (sessionLock) {
                    return allVolumes;
                }

            } else if (string.Compare(name, "corrVolume", true) == 0) {

                lock (sessionLock) {
                    List<Volume> volume = new List<Volume>(1);
                    if (correctionVolume != null) volume.Add(correctionVolume);
                    return volume;
                }

            } else if (string.Compare(name, "realignVolume", true) == 0) {

                lock (sessionLock) {
                    List<Volume> volume = new List<Volume>(1);
                    if (realignmentVolume != null) volume.Add(realignmentVolume);
                    return volume;
                }

            } else if (string.Compare(name, "globalMask", true) == 0) {

                lock (sessionLock) {
                    List<Volume> volume = new List<Volume>(1);
                    if (globalMask != null) volume.Add(globalMask);
                    return volume;
                }

            } else if (string.Compare(name, "globalRTMask", true) == 0) {

                lock (sessionLock) {
                    List<Volume> volume = new List<Volume>(1);
                    if (globalRTMask != null) volume.Add(globalRTMask);
                    return volume;
                }

            } else if (string.Compare(name, "roiMask", true) == 0) {

                lock (sessionLock) {
                    List<Volume> volume = new List<Volume>(1);
                    if (roiMask != null) volume.Add(roiMask);
                    return volume;
                }

            } else {
                // try as task (or return 0)

                return getTaskVolumes(name);
            }


        }

        public static int getNumberOfTaskVolumeVariables() {

            // thread safety
            lock (sessionLock) {

                return taskVolumes.Count;

            }

        }

        public static string[] getTaskVolumeVariableNames() {

            // thread safety
            lock (sessionLock) {

                // return the keys of the task volumes dictionary
                string[] keys = new string[taskVolumes.Keys.Count];
                taskVolumes.Keys.CopyTo(keys, 0);
                return keys;

            }

        }

        public static List<Volume> getTaskVolumes(string taskName) {
            List<Volume> volumes = null;

            // thread safety
            lock (sessionLock) {

                // try to retrieve the set
                if (!taskVolumes.TryGetValue(taskName, out volumes)) {
                    // set not found

                    // return null
                    return null;

                }

                // return the volume set
                return volumes;

            }

        }

        public static int getNumberOfTaskVolumes(string taskName) {
            List<Volume> volumes = null;

            // thread safety
            lock (sessionLock) {

                // try to retrieve the set
                if (!taskVolumes.TryGetValue(taskName, out volumes)) {
                    // set not found

                    return 0;

                }

                // return the number of volumes in the set
                return volumes.Count;

            }

        }

        public static void setTaskVolumes(string taskName, List<Volume> volumes) {

            // thread safety
            lock (sessionLock) {

                // create the list or clear it
                List<Volume> localVolumes = null;
                if (taskVolumes.TryGetValue(taskName, out localVolumes)) {
                    localVolumes.Clear();
                } else {
                    localVolumes = new List<Volume>();
                    taskVolumes.Add(taskName, localVolumes);
                }

                // if there are volumes, add them
                if (volumes != null && volumes.Count > 0) {
                    for (int i = 0; i < volumes.Count; i++)
                        localVolumes.Add(volumes[i]);
                }

            }

        }


        public static void setVolumes(string name, List<Volume> volumes) {

            // check which volume to set
            if (string.Compare(name, "allVolumes", true) == 0) {

                lock (sessionLock) {
                    if (volumes == null || volumes.Count < 1)
                        allVolumes = new List<Volume>();
                    else
                        allVolumes = volumes;

                }

            } else if (string.Compare(name, "corrVolume", true) == 0) {

                lock (sessionLock) {
                    if (volumes == null || volumes.Count < 1)
                        correctionVolume = null;
                    else
                        correctionVolume = volumes[0];
                }

            } else if (string.Compare(name, "realignVolume", true) == 0) {

                lock (sessionLock) {
                    if (volumes == null || volumes.Count < 1)
                        realignmentVolume = null;
                    else
                        realignmentVolume = volumes[0];
                }

            } else if (string.Compare(name, "globalMask", true) == 0) {

                lock (sessionLock) {
                    if (volumes == null || volumes.Count < 1)
                        globalMask = null;
                    else
                        globalMask = volumes[0];
                }

            } else if (string.Compare(name, "globalRTMask", true) == 0) {

                lock (sessionLock) {
                    if (volumes == null || volumes.Count < 1)
                        globalRTMask = null;
                    else
                        globalRTMask = volumes[0];
                }

            } else if (string.Compare(name, "roiMask", true) == 0) {

                lock (sessionLock) {
                    if (volumes == null || volumes.Count < 1)
                        roiMask = null;
                    else
                        roiMask = volumes[0];
                }

            } else {
                // try as task

                setTaskVolumes(name, volumes);

            }

        }

        public static void addTaskVolume(string taskName, Volume volume) {

            if (string.Compare(taskName, "allVolumes", true) == 0 ||
                string.Compare(taskName, "corrVolume", true) == 0 ||
                string.Compare(taskName, "realignVolume", true) == 0 ||
                string.Compare(taskName, "globalMask", true) == 0 ||
                string.Compare(taskName, "globalRTMask", true) == 0 ||
                string.Compare(taskName, "roiMask", true) == 0) {

                // message
                logger.Error("Trying to add a task volumes to task '" + taskName + "', this name is a reserved variable name. Not adding valume");

                // return without adding
                return;

            }

            // thread safety
            lock (sessionLock) {

                // retrieve the volume collection (and create if it does not exist yet)
                List<Volume> volumes = null;
                if (!taskVolumes.TryGetValue(taskName, out volumes)) {
                    volumes = new List<Volume>();
                    taskVolumes.Add(taskName, volumes);

                }

                // add the task volume
                volumes.Add(volume);
            }

        }

        public static void clearTaskVolumes(string taskName) {

            // thread safety
            lock (sessionLock) {

                // retrieve the volume collection (and create if it does not exist yet)
                List<Volume> volumes = null;
                if (taskVolumes.TryGetValue(taskName, out volumes)) {
                    if (volumes != null)    volumes.Clear();
                }
                
            }

        }





        /////
        /// Trials
        /////

        /**
         *  Retrieve the number of trial variables
         **/
        public static int getNumberOfTaskTrialVariables() {

            // thread safety
            lock (sessionLock) {

                return taskTrials.Count;

            }

        }

        /**
         *  Retrieve an array with all trial variable names
         **/
        public static string[] getTaskTrialVariableNames() {

            // thread safety
            lock (sessionLock) {

                // return the keys of the task trials dictionary
                string[] keys = new string[taskTrials.Keys.Count];
                taskTrials.Keys.CopyTo(keys, 0);
                return keys;

            }

        }


        public static List<Block> getTaskTrials(string taskName) {
            List<Block> trials = null;

            // thread safety
            lock (sessionLock) {

                // try to retrieve the set
                if (!taskTrials.TryGetValue(taskName, out trials)) {
                    // set not found

                    // return null
                    return null;

                }

                // return the trial set
                return trials;

            }

        }

        public static void setTaskTrials(string taskName, List<Block> trials) {

            // thread safety
            lock (sessionLock) {

                // create the list or clear it
                List<Block> localTrials = null;
                if (taskTrials.TryGetValue(taskName, out localTrials)) {
                    localTrials.Clear();
                } else {
                    localTrials = new List<Block>();
                    taskTrials.Add(taskName, localTrials);
                }

                // if there are trials, add them
                if (trials != null && trials.Count > 0) {
                    for (int i = 0; i < trials.Count; i++)
                        localTrials.Add(trials[i]);
                }

            }

        }
        public static void clearTaskTrials(string taskName) {

            // thread safety
            lock (sessionLock) {

                // retrieve the trials collection (and create if it does not exist yet)
                List<Block> trials = null;
                if (taskTrials.TryGetValue(taskName, out trials)) {
                    if (trials != null) trials.Clear();
                }

            }

        }

    }

}
