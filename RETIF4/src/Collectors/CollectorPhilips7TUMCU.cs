/**
 * CollectorPhilips7TUMCU class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using RETIF4.Triggers;
using RETIF4.Events;
using RETIF4.Helpers;
using RETIF4.Data;
using NLog;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;

namespace RETIF4.Collectors {

    public class CollectorPhilips7TUMCU : ICollector {

        private static Logger logger = LogManager.GetLogger("Collector");

        public const string collectorName = "Philips 7T UMC Utrecht";
	    public const int threadLoopDelay = 100;			                    // thread loop delay (1000ms / 10 run times per second = rest 100ms)

        public int volumeTriggerValue = 49;                                 // the trigger value that is interpreted as a new scan coming in
	
	    private bool running = true;					                    // flag to define if the collector thread is still running (setting to false will stop the collector thread)
        private bool collecting = false;				                    // flag to define if the collector is collecting from the scanner or ignoring volumes coming in
        private Object lockCollecting = new Object();
        private bool busyCollecting = false;			                    // flag to define whether we are in the middle of collecting

        private string drinDataDumperDirectory = null;                      // the directory to look in for new data, is the output of the Drin Data Dumper
        private string scansStorageDirectory = null;                        // the directory which holds all the subfolders with all the scan data

        private string collectionName = null;
        private string currentScanDataDirectory = null;                     // the directory to which all incoming scans are currently moved

        private Object incomingLock = new Object();                         // lock object to lock processing either coming in from a trigger or a scan file

        private int volumeIDCounter = 0;                                    // variable to hold the volume ID count
        private int lastProcessedVolumeID = 0;                              // identifier of the last volume (directory number and volume number of the last volume processed combined)

        private List<int> arrScanVolumeIDs = new List<int>();               // array holding all the volume ID that are expected to come in (preceded by a trigger from the scanner) and/or that have come in (in the form of a scan)
        private List<int> arrScanVolumeConditions = new List<int>();        // array holding all the volume conditions (at the time of the trigger from the scanner)
        private List<string> arrScanVolumeDatetimes = new List<string>();   // array holding all the volume datetimes (at the time of the trigger from the scanner)
        private List<long> arrScanVolumeTimestamps = new List<long>();      // array holding all the volume timestamps (at the time of the trigger from the scanner)

        private List<Volume> arrVolumes = new List<Volume>();               // array with volumes objects collected for a certain task


	    ////
	    //
	    ////

        // A 'collector' event(handler). An EventHandler delegate is associated with the event.
        // methods should be subscribed to this object
        public event EventHandler<CollectorEventArgs> newVolumeHandler;
	

        public void simulateCollectEvent() {
    	
            // log message
            logger.Info("collector --> simulate fire event");

            // raise the event
            raiseCollectEvent(null);

        }
  
        private void raiseCollectEvent(Volume volume) {
    	
            // fire event
            CollectorEventArgs args = new CollectorEventArgs();
            args.volume = volume;
            EventHandler<CollectorEventArgs> handler = newVolumeHandler;
            if (handler != null)    handler(this, args);


        }
    

        ////
        //
        ////

        public CollectorPhilips7TUMCU() {

            //
            // retrieve collector settings from the app.config
            // 

            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            string strVolumeTriggerValue = appSettings["CollectorVolumeTriggerValue"] ?? "";
            if (!string.IsNullOrEmpty(strVolumeTriggerValue)) int.TryParse(strVolumeTriggerValue, out volumeTriggerValue);




            // create variables with the directories that should be used
            string drinDataDumperDirectory = MainThread.getWorkDirectory() + "ScansIn";
            string scansStorageDirectory = MainThread.getWorkDirectory() + "Data";

            // log message
            logger.Info("Collector " + collectorName);

		    // check the scans input path
		    if (String.IsNullOrEmpty(drinDataDumperDirectory.Trim())) {
			
			    // message
                logger.Error("DDD path is empty, cannot collect images");
			
		    } else {
			
                // check if the path exists and is a directory
                if (Directory.Exists(drinDataDumperDirectory)) {

                    // message and set the path
                    logger.Info("DDD path set to \'" + drinDataDumperDirectory + "\'");
                    this.drinDataDumperDirectory = drinDataDumperDirectory;

                } else {

                    // messsage
                    logger.Warn("DDD path \'" + drinDataDumperDirectory + "\' does not exist, creating folder");

                    // create the directory
                    Directory.CreateDirectory(drinDataDumperDirectory);

                    // set the path
                    this.drinDataDumperDirectory = drinDataDumperDirectory;

                }

		    }

            

		    // check the scans input path
		    if (String.IsNullOrEmpty(scansStorageDirectory.Trim())) {
			
			    // warning message
                logger.Warn("Warning - scans storage path is empty, cannot collect images");
			
		    } else {
			
                // check if the path exists and is a directory
                if (Directory.Exists(scansStorageDirectory)) {
                    // exists

                    // message and set the path
                    logger.Info("Scans storage path set to \'" + scansStorageDirectory + "\'");
                    this.scansStorageDirectory = scansStorageDirectory;

                } else {
                    // folder does not exist

                    // message
                    logger.Warn("Scans storage path \'" + scansStorageDirectory + "\' does not exist, creating folder");


                    // create the directory
                    Directory.CreateDirectory(scansStorageDirectory);

                    // set the path
                    this.scansStorageDirectory = scansStorageDirectory;

                }

		    }

            ITrigger trigger = MainThread.getTrigger();
            if (trigger != null) {

                // add a callback to the COMtrigger to process the incoming trigger signaling that a new volume is being acquired
                trigger.triggerHandler += processTrigger;

                // check if the trigger object is of the type SerialTrigger
                if (trigger.GetType() == typeof(SerialTrigger)) {

                    // message
                    logger.Info("Listening for triggers on " + ((SerialTrigger)trigger).getComPort() + ", giving value " + volumeTriggerValue);

                } else {
                    
                    // message
                    logger.Warn("A different ITrigger object than SerialTrigger is set for the collector.");
                    logger.Info("Listening for triggers with value " + volumeTriggerValue);

                }
                
            } else {

                // message
                logger.Error("No (serial) trigger set for the collector. Make sure the trigger class is set first and available from the MainThread");

            }
            
            // start a new thread
            Thread thread = new Thread(this.run);
            thread.Start();

	    }

	    /**
	     * Start the collector to accept and process new volumes from the scanner
	     */
        public bool start() {

            // start with a standard collection name
            return start("Task");
		
        }

	    /**
	     * Start the collector to accept and process new volumes from the scanner
	     */
        public bool start(String collectionName) {

            lock(lockCollecting) {

		        // check if the collector is collecting
		        if (collecting) {
			
			        // stop collecting
			        stop();
		
			        // allow the collection to stop (timeout in 15 seconds)
			        int timeout = 1500;
			        while (busyCollecting && timeout != 0) {
                        Thread.Sleep(10);
				        timeout--;
			        }
			
		        }
		
		        // initially not collecting and set as not able to collect
                collecting = false;             // (for clarity, the stop above already should have set this to false)
                bool ableToCollect = true;      // flag to define whether the preparations on the directories involved for collecting were successful

		        // reset the current scan data directory path
                currentScanDataDirectory = null;
             
                // set the volume ID counters to 0, so the next will be 1 (which is the first scan)
                // if it is a continous scan the first scan will be needed to get in sync reset all incoming other information
                volumeIDCounter = 0;
                lastProcessedVolumeID = 0;

                // reset the arrays with collected timing/condition information
                arrScanVolumeIDs.Clear();
                arrScanVolumeConditions.Clear();
                arrScanVolumeDatetimes.Clear();
                arrScanVolumeTimestamps.Clear();
    
		        // check if the DDD and storage paths are set
		        if (drinDataDumperDirectory == null || scansStorageDirectory == null) {
                    // either the DDD and/or storage path is not set

                    // message
                    logger.Error("Error while starting collection, either the DDD and/or storage path is not set to a valid directory");

                    // not collecting
                    ableToCollect = false;

                } else {
	
	                //
	                // empty the dump directory before starting
	                //
	                // move the dump directory content to the clearing folder to
	                // ensure the dump directory is empty before the start of
	                // the collector
	                // move all content in the dump directory except for the last sequentially named directory
	                // (this directory could be still in use by the DrinDataDumper, and removing it will crash the DDD)
	                // In the last directory (if there is one) move all the files to the clear directory
	                //



                    // inventorize/list the directories and files in the data dump directory
                    string[] dirs = new string[0];
                    string[] files = new string[0];
                    try {

                        dirs = Directory.GetDirectories(drinDataDumperDirectory);
                        files = Directory.GetFiles(drinDataDumperDirectory);

                    } catch(Exception) {

                        // message
                        logger.Error("Error while starting collection, could not list DDD subdirectories and files");

                        // not collecting
                        ableToCollect = false;

                    }
                    if (ableToCollect) {

			            // check if the dump folder has files
			            if (dirs.Length != 0 || files.Length != 0) {
				            // has files/directories
				
				            // acquire a new clear destination folder
				            string clearTargetFolder = createNextDataSubdirectory("clear_" + DateHelper.getDateTime());
				            if (clearTargetFolder == null) {
					            // if a new clear destination folder was successfully acquired

                                // message
                                logger.Error("Error while starting collection, unable to create a clear data sub directory");

                                // not collecting
                                ableToCollect = false;

                            } else {

                                try {

                                    // sort the directories
                                    Array.Sort(dirs);
                        
                                    // if there are loose files in the dump directory
                                    if (files.Length > 0) {

			                            // tiny break to allow the files to move (it is
			                            // possible that some files exist but are still in
			                            // use by the process placing them there, allowing a
			                            // very short break should allow those processes to
			                            // let go of the file (prevents move errors)
                                        Thread.Sleep(100);
                                
			            	            // move the loose files
                                        string dstFile = "";
			            	            for (int i = 0; i < files.Length; i++) {
                                            dstFile = Path.Combine(clearTargetFolder, Path.GetFileName(files[i]));
                                            File.Move(files[i], dstFile);
			            	            }

                                    }

                                    // if there are directories in the dump directory
                                    if (dirs.Length > 0) {
				    
                                        // loop through the subdirectories in the dump directory
                                        for (int i = 0; i < dirs.Length; i++) {
		            		
                                            string dstPath = Path.Combine(clearTargetFolder, IOHelper.MakeRelativePath(drinDataDumperDirectory, dirs[i]));

                                            // check if this is the last subdirectory in the dump directory
                                            if (i == dirs.Length - 1) {
                                                // last subdirectory
                                            
                                                // make sure the sequential subdirectory exists
                                                // in the target folder as well
                                                if (!Directory.Exists(dstPath))     Directory.CreateDirectory(dstPath);


                                                // copy all files seperately (this purposely not
                                                // done by as an entiry directory move since if we start
                                                // the collector on the fly, the last file of the last sequential
                                                // directory might still be in use. To prevent this from happening
                                                // we should not move the last volume, however every volume consist
                                                // of three files which we don't want to split
                                                files = Directory.GetFiles(dirs[i]);
                                                Array.Sort(files);

                                                // if there are scans available
                                                if (files.Length > 0) {

                                                    // tiny break to allow the files to move (it is
                                                    // possible that some files exist but are still in
                                                    // use by the process placing them there, allowing a
                                                    // very short break should allow those processes to
                                                    // let go of the file
                                                    Thread.Sleep(100);
                                                                                                
                                                    // inventarize the unique filesnames, and different extensions in lists 
                                                    Dictionary<string, List<string>> uniqueFiles = new Dictionary<string, List<string>>();
                                                    for (int j = 0; j < files.Length; j++) {
                                                        string filename = Path.GetFileName(files[j]);
                                                        string extension = Path.GetExtension(filename);
                                                        string filenameWithoutExtension = filename.Substring(0, filename.Length - extension.Length);
                                                        if (!uniqueFiles.ContainsKey(filenameWithoutExtension)) {
                                                            uniqueFiles[filenameWithoutExtension] = new List<string>();
                                                        }
                                                        uniqueFiles[filenameWithoutExtension].Add(extension);
                                                    }

                                                    // loop through the unique filenames (volumes)
                                                    string srcFile = "";
                                                    string dstFile = "";
                                                    foreach (KeyValuePair<string, List<string>> filename in uniqueFiles) {

                                                        // check if the volume is a complete set of 3 files
                                                        //if (filename.Value.Count >= 2) {
                                                        if (filename.Value.Count == 3) {
                                                            // complete set of 3 files
                                                            
                                                            // move the 3 volume files
                                                            foreach (string extension in filename.Value) {
                                                                srcFile = Path.Combine(dirs[i], (filename.Key + extension));
                                                                dstFile = Path.Combine(dstPath, (filename.Key + extension));
                                                                //logger.Trace(srcFile);
                                                                //logger.Trace(dstFile);
                                                                File.Move(srcFile, dstFile);

                                                            }

                                                        }

                                                    }

                                                }

                                            } else {
                                                // not last subdirectory
		                        
                                                // move the entire directory
                                                Directory.Move(dirs[i], dstPath);



                                            }
		            		
                                        }
		
                                    }

                                } catch (Exception) {

                                    // message
                                    logger.Error("Error while starting collection, unable to move content of the DDD directory to the clear data sub directory ('" + clearTargetFolder + "')");

                                    // not collecting
                                    ableToCollect = false;

                                }

                            }

				        }
				
			        }
		
		        }

                if (ableToCollect) {

                    // create a new directory to collect in
                    currentScanDataDirectory = createNextDataSubdirectory(collectionName + "_" + DateHelper.getDateTime());

                    // store the collection name
                    this.collectionName = collectionName;

                    // message
                    logger.Info("Incoming scans will be moved to '" + currentScanDataDirectory + "'");
                    logger.Info("Collection started for '" + collectionName + "'");

                    // start collecting
                    // (everything that comes in from here is considered to belong to this collection)
                    collecting = true;

                    // return
                    return true;

                } else {

                    // message
                    logger.Error("Errors have occured, unable to collect for '" + collectionName + "'");

                    // return
                    return false;

                }

            }

        }


	    /**
	     * Stop the collector from accepting and processing new volumes coming from the scanner
	     */
	    public void stop() {

            lock(lockCollecting) {

                // check if the collector is collecting
                if (collecting) {

                    // message
                    logger.Info("Collection stopped for '" + collectionName + "'");

                    // stop collecting
                    collecting = false;

                }

            }

	    }


	    /**
	     * Returns whether the collector is collecting new volumes from the scanner 
	     * 
	     * @return Whether the collector is collecting new volumes from the scanner
	     */
	    public bool isCollecting() {
		
            lock(lockCollecting) {
    		    return collecting;
            }
	    }
	
	    /**
	     * Stop the collector from accepting and processing new volumes coming from the scanner 
         * and stop the collector thread
	     */
	    public void destroy() {

            // stop collecting (stop will check if it was running in the first place)
		    stop();
		
		    // stop the thread from running
		    running = false;
		
		    // allow the collection to stop (timeout in 15 seconds)
		    int timeout = 1500;
		    while (busyCollecting && timeout != 0) {
                Thread.Sleep(10);
			    timeout--;
		    }
		
		    // allow the collector thread to stop
            Thread.Sleep(200);
		
	    }
	
	    /**
	     * Returns whether the collector thread is still running
	     * Note, this is something different than actually collecting
	     * 
	     * @return Whether the collector thread is running
	     */
	    public bool isRunning() {
		    return running;
	    }

	    /**
	     * Collector running thread
	     */
        private void run() {

            // name this thread
            if (Thread.CurrentThread.Name == null) {
                Thread.CurrentThread.Name = "Collector Thread";
            }

            // log message
            logger.Debug("Thread started");

		    // loop while running
		    while(running) {

                lock(lockCollecting) {

			        // check if we are collecting
			        if (collecting) {
				
				        // flag as busy collecting (used to wait for the collecting to finish when stopping the collector)
                        // (TODO: might be double as the lock will accoplish the same, can't hurt though)
				        busyCollecting = true;

	                    // lock (no incoming trigger adjusting variables, thread safety)
                        lock(incomingLock) {
                        
                            // inventorize/move files in the root of the data dump directory
                            // (there should not be any files in there anyway but still check and move)
                            string[] rootFiles = new string[0];
                            try {
                                rootFiles = Directory.GetFiles(drinDataDumperDirectory);
                                if (rootFiles.Length != 0) {

			                        // tiny break to allow the files to move (it is
			                        // possible that some files exist but are still in
			                        // use by the process placing them there, allowing a
			                        // very short break should allow those processes to
			                        // let go of the file (prevents move errors)
                                    Thread.Sleep(20);
                                
			                        // move the loose files
                                    string dstFile = "";
			                        for (int i = 0; i < rootFiles.Length; i++) {
                                        dstFile = Path.Combine(currentScanDataDirectory, Path.GetFileName(rootFiles[i]));
                                        File.Move(rootFiles[i], dstFile);
			                        }

                                }
                            } catch(Exception) {

                                // message
                                logger.Error("Error while retrieving/moves files from the root of the dump folder");

                            }

                            // retrieve the files from the subdiretories in the dump folder
                            string[] files = null;
                            try {

                                files = Directory.GetFiles(drinDataDumperDirectory, "*.*", SearchOption.AllDirectories);
                                Array.Sort(files);

                            } catch (Exception) {

                                // message
                                logger.Error("Error while retrieving files in the dump folder");

                            }

                            // if there are scans available
                            if (files.Length > 0) {
                            
                                try {

                                    // tiny break to allow the files to move (it is
                                    // possible that some files exist but are still in
                                    // use by the process placing them there, allowing a
                                    // very short break should allow those processes to
                                    // let go of the file
                                    Thread.Sleep(20);

                                    // make path relative and inventarize the unique filesnames (and different extensions in lists)
                                    List<string> uniqueFilesKeys = new List<string>();
                                    List<List<string>> uniqueFilesValues = new List<List<string>>();
                                    for (int j = 0; j < files.Length; j++) {

                                        // make path relative to the DDD directory
                                        files[j] = IOHelper.MakeRelativePath(drinDataDumperDirectory, files[j]);

                                        // inventarize unique files with their different extensions
                                        string filename = Path.GetFileName(files[j]);
                                        string extension = Path.GetExtension(filename);
                                        string fileWithoutExtension = files[j].Substring(0, files[j].Length - extension.Length);
                                        int keyIndex = uniqueFilesKeys.IndexOf(fileWithoutExtension);
                                        if (keyIndex == -1) {
                                            uniqueFilesKeys.Add(fileWithoutExtension);
                                            uniqueFilesValues.Add(new List<string>());
                                            keyIndex = uniqueFilesKeys.Count - 1;
                                        }
                                        uniqueFilesValues[keyIndex].Add(extension);
                                        
                                    }

                                    // loop through the unique filesets
                                    string volumePath = null;
                                    for (int j = 0; j < uniqueFilesKeys.Count; j++) {

                                        // make sure the number of volume files is a multiple of 3
                                        // this makes sure one scanset, (hdr, img, txt) is complete
                                        if (uniqueFilesValues[j].Count == 3) {
                                        //if (uniqueFilesValues[j].Count >= 2) {
                                            // complete volume

                                            // check if there is a subdirectory (should be)
                                            string inSubDirectory = Path.GetDirectoryName(uniqueFilesKeys[j]);
                                            if (String.IsNullOrEmpty(inSubDirectory.Trim())) continue;

                                            // make sure the sequential subdirectory exists
                                            // in the target folder as well
                                            volumePath = Path.Combine(currentScanDataDirectory, inSubDirectory);
                                            if (!Directory.Exists(volumePath)) Directory.CreateDirectory(volumePath);

                                            // add the filename to the volume path
                                            volumePath = Path.Combine(currentScanDataDirectory, uniqueFilesKeys[j]);

                                            // move the volume
                                            string prefExtension = null;
                                            for (int k = 0; k < uniqueFilesValues[j].Count; k++) {
                                                File.Move(  Path.Combine(drinDataDumperDirectory, (uniqueFilesKeys[j] + uniqueFilesValues[j][k])), 
                                                            volumePath + uniqueFilesValues[j][k]);
                                                if (string.Compare(uniqueFilesValues[j][k], ".img", true) == 0) {
                                                    prefExtension = uniqueFilesValues[j][k];
                                                }
                                            }

                                            // check if the prefered extension was found
                                            if (prefExtension == null) {

                                                // message
                                                logger.Error("Found and copied volume files ('" + volumePath + "'), but could not find preferred extension, not processing volume");

                                                // clear the volumePath, so the volume will not be processed any further
                                                volumePath = "";

                                            } else {

                                                // add the extension to the volume path
                                                volumePath = volumePath + prefExtension;

                                            }
                                            
                                        }

                                    }

                                    // check if there is a path to the last image file that was moved (should come from the last loop)
                                    if (volumePath != null) {
                                        string filename = Path.GetFileName(volumePath);
                                        string extension = Path.GetExtension(filename);
                                        string fileWithoutExtension = volumePath.Substring(0, volumePath.Length - extension.Length);
                                        string inSubDirectory = Path.GetDirectoryName(IOHelper.MakeRelativePath(currentScanDataDirectory, volumePath));

                                        // build the scan identifier (= subfolder / scan number)
                                        int volumeNr = 0;
                                        int subDirNr = 0;
                                        string strScanNr = "";
                                        if (fileWithoutExtension.Length > 5)    strScanNr = fileWithoutExtension.Substring(fileWithoutExtension.Length - 5);
                                        if (int.TryParse(strScanNr, out volumeNr) && int.TryParse(inSubDirectory, out subDirNr)) {
                                            int currentScanID = subDirNr * 100000 + volumeNr;

                                            // create a new volume object
                                            Volume volume = new Volume(Volume.VolumeSource.Collected);
                                            volume.volumeId = currentScanID;
                                            volume.volumeNr = volumeNr;
                                            volume.subDirNr = subDirNr;
                                            volume.filepath = volumePath;
                                            arrVolumes.Add(volume);

                                            //logger.Trace("currentScanID: " + currentScanID);
                                            //logger.Trace("lastProcessedVolumeID: " + lastProcessedVolumeID);

                                            // check if the scan image is higher/newer than the last one processed
                                            // (base this on the combination of the scan directory and the scan number, this way the
                                            // scanner can be stopped and started while the task can keep on running)
                                            if (currentScanID > lastProcessedVolumeID) {

                                                // update the last processed number
                                                lastProcessedVolumeID = currentScanID;

                                                // set a value holding the predicted quality
                                                // -1 = bad, was not expected, no timing/condition information available
                                                // 0 = ad hoc created (and retrieved) timing/condition information
                                                // 1 = predicted (without any matching issues)
                                                // 2 = predicted (with directory naming correction)
                                                int predictiveQuality = 1;

                                                // check if data which came in has a higher volume number (directory and volume
                                                // number combined) than the volume trigger counter and was thus not announced by a trigger
                                                if (currentScanID > volumeIDCounter) {
                                                    // is higher, this can have two causes:
                                                    // 1. the triggercounter is lagging behind because it missed the first trigger
                                                    // 2. the triggercounter start with one decimal not taking the directory into account (triggercounter + array should be updated)


                                                    // check if the cause is in the directory addition to the count
                                                    //
                                                    // TODO: als er meerdere triggers zijn geweest en er komt een scan binnen met directory toevoeging
                                                    // (waarbij de directory wel in de array staat mits de directory wordt toegevoegd, In welke situatie
                                                    // doet dit zich dit voor? (= als zowel scan 1 als 2 vertraagd binnenkomen, nadat de eerste twee 
                                                    // scan start triggers al zijn geweest) En wat dan?
                                                    //if newDataNr == obj.scanTriggerCounter
                                                    if (volumeIDCounter < 100000 && volumeNr <= volumeIDCounter) {
                                                        // directory sequential naming addition causes the volume number to be higher than the volume trigger counter

                                                        // message
                                                        logger.Warn("Scanvolume ID counter out of sync because of directory, correcting");

                                                        // set the predictive quality for later
                                                        predictiveQuality = 2;

                                                        // find the index of the last occurance of the current scan number
                                                        int lastIndex = -1;
                                                        for (int i = 0; i < arrScanVolumeIDs.Count; i++)
                                                            if (arrScanVolumeIDs[i] == volumeNr)     lastIndex = i;

                                                        if (lastIndex != -1) {
                                                            // correct the array with volume counters by adding the dir
                                                            for (int i = lastIndex; i < arrScanVolumeIDs.Count; i++)
                                                                arrScanVolumeIDs[i] = arrScanVolumeIDs[i] + subDirNr * 100000;
                                                            //arrScanVolumeIDs[i] = arrScanVolumeIDs[i] + subDirNr;
                                                        }

                                                        // correct the counter (make it as high as the highest scan number in the array)
                                                        if (arrScanVolumeIDs.Count == 0)
                                                            volumeIDCounter = currentScanID;
                                                        else
                                                            volumeIDCounter = arrScanVolumeIDs[arrScanVolumeIDs.Count - 1];

                                                        // message
                                                        logger.Info("Scanvolume ID counter & array corrected. VolumeIDCounter: " + volumeIDCounter);

                                                    } else {
                                                        // the triggercounter is lagging behind because:
                                                        // 1. it missed the first trigger
                                                        // 2. the scanner has stopped and started again under a new directory (so the next scan predicted

                                                        // TODO: kijk of dit slimmer kan in het geval van de
                                                        // tweede oorzaak. Deze zou opzich nog wel in de array
                                                        // moeten zijn

                                                        // message
                                                        logger.Warn("Scanvolume ID counter out of sync because of missing trigger or restarted scan (directory). Volume " + currentScanID + " is less reliable because of delay condition retrieval");

                                                        // set the predictive quality for later
                                                        predictiveQuality = 0;

                                                        // correct by ad hoc generation of a new entry for this
                                                        // scan to fall in. This will also cause the trigger
                                                        // counter to update to this value so from this moment
                                                        // on new entry (on scan start trigger) will be numbered
                                                        // properly
                                                        generateNextVolumeEntry(currentScanID);

                                                    }

                                                }

                                                // check if the scan volume can be found in array with predicted incoming scans
                                                int volumeArrIndex = -1;
                                                for (int i = 0; i < arrScanVolumeIDs.Count; i++)
                                                    if (arrScanVolumeIDs[i] == currentScanID)     volumeArrIndex = i;
                                                if (volumeArrIndex != -1) {

                                                    // set the volume as not discarded (used)
                                                    volume.discarded = false;

                                                    // flag volume as matched
                                                    volume.matched = true;

                                                    // set the volume predictive quality
                                                    volume.predictiveQuality = predictiveQuality;

                                                    // add additional timing/condition information (which was already gathered when the trigger came in)
                                                    volume.condition = arrScanVolumeConditions[volumeArrIndex];
                                                    volume.dateTime = arrScanVolumeDatetimes[volumeArrIndex];
                                                    volume.timeStamp = arrScanVolumeTimestamps[volumeArrIndex];

                                                    // message
                                                    if (arrScanVolumeConditions[volumeArrIndex] == -1)
                                                        logger.Debug("Volume " + currentScanID + " found in array, matched with condition: -1 and time: " + volume.dateTime);
                                                    else
                                                        logger.Debug("Volume " + currentScanID + " found in array, matched with condition: " + volume.condition + " and time: " + volume.dateTime);

                                                    // raise the collected event
                                                    raiseCollectEvent(volume);

                                                } else {

                                                    // set the volume as not discarded (used)
                                                    volume.discarded = false;

                                                    // flag volume as not matched
                                                    volume.matched = false;

                                                    // set predictive quality (should already be set)
                                                    volume.predictiveQuality = -1;

                                                    // message
                                                    logger.Warn("Unable to find volume, could not match timing/condition information to scan " + currentScanID + ". Still processing without timing/condition information.");

                                                    // raise the collected event
                                                    raiseCollectEvent(volume);
                                                }


                                            } else {

                                                // set the volume as discarded
                                                volume.discarded = true;

                                                // message
                                                logger.Warn("Volume (" + strScanNr + "/" + currentScanID + ") came in older than the last scan processed (" + lastProcessedVolumeID + "), discarding");

                                            }

                                        } else {

                                            // message
                                            logger.Error("Error while building the scan identifier from scan ('" + volumePath + "'), path and filename structure not as expected. Scan will not be processed.");

                                        }

                                    }

                                } catch (Exception e) {

                                    // message
                                    logger.Error("Error while retrieving/moving files from the dump folder: " + e.Message);

                                }
                            }

                        }
	
				        // flag as no longer busy collecting (used to wait for the collecting to finish when stopping the collector)
				        busyCollecting = false;
				
			        }

			    }

			    // if still running then sleep to allow other processes
			    if (running) {
                    Thread.Sleep(threadLoopDelay);
			    }
			
		    }

            // log message
            logger.Debug("Thread stopped");

        }


	    public void processTrigger(object sender, TriggerEventArgs e) {
            //logger.Debug("Collector processTrigger {0} {1}", e.value, e.datetime);

		    // check the collector is collecting and if the trigger value indicates a new volume coming in 
		    if (collecting && e.value == volumeTriggerValue) {

			    // message
                logger.Info("Process trigger (new scan volume) scanner in");
			
	            // generate a new scanvolume entry (which would hold the
	            // current view condition, time and timestamp) expecting a
	            // scan volume number of scanTriggerCounter + 1
                generateNextVolumeEntry(volumeIDCounter + 1);   
			
		    }
		
	    }


	    /**
         * this function generates an entry in the array which holds the
         * scans that are expected to come in. When generated it also
         * stores the current view condition and local system time.
         * 
	     * @param counter	the expected scan number to be generated
	     */
	    private void generateNextVolumeEntry(int counter) {

            // lock (no incoming volume adjusting variables, thread safety)
            lock(incomingLock) {

                // create an entry (based on the counter)
                arrScanVolumeIDs.Add(counter);

                // retrieve and store the condition
                int condition = MainThread.getViewCondition();
                arrScanVolumeConditions.Add(condition);
   
                // store the datetime and timestamp
                arrScanVolumeDatetimes.Add(DateHelper.getDateTime());
                arrScanVolumeTimestamps.Add(Stopwatch.GetTimestamp());

                // message
                logger.Debug("Created entry, expecting volume " + counter + "; condition: " + arrScanVolumeConditions[arrScanVolumeConditions.Count - 1] + "; datetime: " + arrScanVolumeDatetimes[arrScanVolumeDatetimes.Count - 1]);
            
                // update the scan triggerCounter
                volumeIDCounter = counter;

                // message
                logger.Debug("Scanvolume ID Counter updated to " + counter);

            }

	    }


	    /**
	     * create a next subfolder inside of the datadirectory. First try if
	     * name is available, if that is already taken append with numbers. 
	     * 
	     * @param name of the subdirectory to be created
	     * @return the resulting path after creation
	     */
	    private string createNextDataSubdirectory(String name) {
		    if (scansStorageDirectory == null)	return null;
		
            try {
                
		        // make sure there is at least a name for the subdirectory
                if (String.IsNullOrEmpty(name.Trim())) name = "sub";
		
		        // create an initial path without counters added
                String directory = Path.Combine(scansStorageDirectory, name);
		
		        // start looping and continue looping while the wanted subdirectory already exists		
		        int tries = 1;
                while (Directory.Exists(directory)) {

			        // create a path appended with a number
                    directory = Path.Combine(scansStorageDirectory, (name + "_" + tries));
			
                    // try next suffix
			        tries = tries + 1;
			
		        }
		
		        // create the new directory
                Directory.CreateDirectory(directory);
		
		        // return the subdirectory
                return directory;

            } catch(Exception) {

                return null;

            }

	    }

    }
}
