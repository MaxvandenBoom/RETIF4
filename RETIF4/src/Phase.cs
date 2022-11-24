/**
 * Phase class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
namespace RETIF4 {

    public class Phase {
		
        public enum StartType : int {
            IMMEDIATE___SCENE_IMMEDIATE,
            IMMEDIATE___SCENE_AT_INCOMING_SCAN,
        };

        public enum EndType : int {
            NONE,                                   // not ending
            TASKSFINISH,                            // ending when the task is finished
            TASKSFINISH_NOSCANS,                    // ending when the task is finished and there are no scans left in the buffer
            TIME,                                   // after x milliseconds
            SCANS,                                  // after x scans
        };

        public enum AutoProcessType : int {
            NONE,                                   // no automatic processing
            CORRECT,                                // automatically RT correct (header and reorient) incoming volume (if possible)
            CORRECT_REALIGN,                        // automatically RT correct (header and reorient) and realign incoming volume (if possible)
            CORRECT_REALIGN_ROI,                    // automatically RT correct (header and reorient) and realign the volume, then extract the ROI values from the incoming volume (if possible)
            CORRECT_REALIGN_ROI_DETREND,            // automatically RT correct (header and reorient) and realign the volume, then extract and detrend the ROI values from the incoming volume (if possible)
            CORRECT_REALIGN_SMOOTH,                 // automatically RT correct (header and reorient), realign and smooth incoming volume (if possible)
            CORRECT_REALIGN_SMOOTH_ROI,             // automatically RT correct (header and reorient), realign and smooth the volume, then extract the ROI values from the incoming volume (if possible)
            CORRECT_REALIGN_SMOOTH_ROI_DETREND,     // automatically RT correct (header and reorient), realign and smooth the volume, then extract and detrend the ROI values from the incoming volume (if possible)
        };

        public int phaseID = 0;
        public string phaseName = "";

        private int preScene = 0;
        private int scene = -1;
        private StartType startType = StartType.IMMEDIATE___SCENE_IMMEDIATE;
        public bool collect = false;
        public string collectionName = "";
        public bool requiresMatlab = false;
        public AutoProcessType autoProcessType = AutoProcessType.NONE;
        public int autoProcessSmoothingFWHM = 4;
        public bool autoProcessWrite = true;

        private EndType endType = EndType.NONE;
        private int endTypeNumScans = 0;
        private int endTypeTimeMs = 0;

        public Phase nextPhase = null;


        // active variables
        public bool tasksFinished = false;


        public Phase(int phaseID, string phaseName) {
            this.phaseID = phaseID;
            this.phaseName = phaseName;
        }

        //
        //
        //

        public EndType getEndType() {
            return endType;
        }

        public int getEndTypeNumScans() {
            return endTypeNumScans;
        }

        public int getEndTypeTimeMs() {
            return endTypeTimeMs;
        }

        public void setEndTypeNone() {
            endType = EndType.NONE;
        }

        public void setEndTypeTasksFinish(bool noScans) {
            if (noScans)    endType = EndType.TASKSFINISH_NOSCANS;
            else            endType = EndType.TASKSFINISH;
        }
        
        public void setEndTypeTime(int milliseconds) {
            if (milliseconds < 0) {
                endType = EndType.NONE;
                return;
            }

            endType = EndType.TIME;
            endTypeTimeMs = milliseconds;
        }

        public void setEndTypeScans(int numScans) {
            if (numScans < 1) {
                endType = EndType.NONE;
                return;
            }

            endType = EndType.SCANS;
            endTypeNumScans = numScans;
        }


        //
        //
        //

        public StartType getStartType() {
            return startType;
        }

        public int getScene() {
            return this.scene;
        }

        public int getPreScene() {
            return this.preScene;
        }

        public void setSceneImmediate(int scene) {
            this.scene = scene;
            startType = StartType.IMMEDIATE___SCENE_IMMEDIATE;
        }

        public void setSceneAtIncomingScan(int scene, int preScene) {
            this.scene = scene;
            this.preScene = preScene;
            startType = StartType.IMMEDIATE___SCENE_AT_INCOMING_SCAN;
        }


        public AutoProcessType getAutoProcessType() {
            return this.autoProcessType;
        }

        public void setAutoProcessType(AutoProcessType type) {
            this.autoProcessType = type;
        }

        public int getAutoProcessSmoothingFWHM() {
            return this.autoProcessSmoothingFWHM;
        }

        public void setAutoProcessSmoothingFWHM(int FWHM) {
            this.autoProcessSmoothingFWHM = FWHM;
        }

        public bool getAutoProcessWrite() {
            return this.autoProcessWrite;
        }

        public void setAutoProcessWrite(bool write) {
            this.autoProcessWrite = write;
        }


    }

}
