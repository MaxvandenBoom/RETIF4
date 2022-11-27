/**
 * AmygView class
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
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace RETIF4.Views {

    public class AmygView : OpenTKView, IViewRF {
        //public class AmygView : SharpGLView, IViewRF {

        private static Logger logger = LogManager.GetLogger("View");                        // the logger object for the view

        public enum Scenes : int {
            NoneOrNoSceneChange = -1,           // reserved, always keep this
            Black = 0,                          // advised, keep this
            CalibrateView = 1,                  // advised, keep this

            // define your own scenes
            ScannerSetup = 3,
            LocalizerInstructions = 5,
            Localizer = 7,
            LocalizerDone = 9,

            Break_1 = 11,
            Feedback_Run1_Instructions = 13,
            Feedback_Run1 = 15,
            Feedback_Run1_Done = 17,

            Feedback_Run2_Instructions = 23,
            Feedback_Run2 = 25,
            Feedback_Run2_Done = 27,

            Feedback_Run3_Instructions = 33,
            Feedback_Run3 = 35,
            Feedback_Run3_Done = 37,

            Transfer_Instructions = 43,
            Transfer = 45,
            Transfer_Done = 47,

        };

        public enum CalBlockConditions : int {
            Rest = 0,
            FaceNeutral = 1,
            FaceAngry = 2

        }

        public enum NfTrialConditions : int {
            NeutralView = 0,
            AngryView = 1,
            AngryRegulate = 2

        }

        // constants
        private const double RECTANGLE_HEIGHT_PERC = 90;                                    // the size of the rectangle in percentages of the screen height 
        private const double RECTANGLE_Y_OFFSET_PERC = -2;                                  // the y offset of the rectangle in percentages of the screen height 
        private const double RECTANGLE_DOT_RADIUS_PERC = 2;                                 // the size of the dot in the rectangle in percentages of the screen height 
        private const double RECTANGLE_DOT_Y_OFFSET_PERC = -2;                              // the y offset of the dot in relation to the middle of the rectangle in percentages of the screen height
        private const int FACE_DISPLAY_DURATION = 500;                                      // the time single face is displayed

        private const int LOC_NUMBLOCKS = 8;                                                // number of localizer blocks
        private const int LOC_REST_DURATION = 16000;                                        // rest duration inbetween faces in milliseconds
        private const int LOC_FACES_NEUTRAL_DURATION = 16000;                               // faces neutral block duration in milliseconds
        private const int LOC_FACES_ANGRY_DURATION = 16000;                                 // faces angry block duration in milliseconds
        /*
        private const int NF_FIXATION_DURATION = 5000;                                      // fixation/rest duration at the beginning of each neurofeedback trial
        private const int NF_INSTRUCTION_DURATION = 2000;                                   // instruction duration at the middle of each neurofeedback trial
        private const int NF_FEEDBACK_DURATION = 10000;                                     // feedback duration at the end of each neurofeedback trial
        private const int NF_FEEDBACK_UPDATEINTERVAL = 200;                                 // feedback update interval
        */
        private const int NF_NUMBLOCKS = 5;                                                 // number of neurofeedback blocks (one block contains one 'view neutral', one 'view angry' and one 'regulate angry')
        private const int NF_FIXATION_DURATION = 16000;                                     // fixation/rest duration at the beginning of each neurofeedback trial
        private const int NF_INSTRUCTION_DURATION = 2000;                                   // instruction duration at the middle of each neurofeedback trial
        private const int NF_FEEDBACK_DURATION = 24000;                                     // feedback duration at the end of each neurofeedback trial
        private const int NF_FEEDBACK_UPDATEINTERVAL = 200;                                 // feedback update interval
        
        // view variables
        private Scenes currentScene = Scenes.NoneOrNoSceneChange;                           // the current scene which is being shown
        private long phaseStartTimestamp = 0;

        private int faceRectWidth = 0;
		private int faceRectHeight = 0;
        private int faceRectLeft = 0;
        private int faceRectTop = 0;
        private int barRectWidth = 0;
        private int lbarRectLeft = 0;
        private int rbarRectLeft = 0;
        private double dotLeft = 0;
        private double dotTop = 0;
        private double dotRadius = 20;

        private glFreeTypeFont textFont = new glFreeTypeFont();


        // localizer task variables
        private Block[] locBlockSequence = new Block[0];
        private int locBlockSequenceCounter = 0;
        private CalBlockConditions locCurrentBlockCondition = CalBlockConditions.Rest;
        private int[] locFaces = new int[0];                                                // the faces used in the localizer task (set during load)

        // feedback task variables 
        private NfTrialConditions[] nfTrialSequence = new NfTrialConditions[0];             // 
        private int nfTrialSequenceCounter = 0;                                             // the nf trial counter
        private NfTrialConditions nfCurrentTrialCondition = NfTrialConditions.NeutralView;  // the nf trial condition (neutralview/angryview/angryregulate)
        private int[] nfFaces = new int[0];                                                 // the faces to use in the neurofeedback task (set during load)

        private int nfTrialPhase = 0;                                                       // the nf trial phase: fixation (0), instruction (1), feedback (2)
        private long nfTrialPhaseEndTime = 0;                                               // the endtime of the feedback trialphase (the 'stateEndTime' variable is updated more frequent to pull feedback values)
        private RingBuffer nfFeedbackBaseLineBuffer = null;
        private double nfFeedbackBaseLineMean = 0.0;
        private double nfFeedbackValue = 0.0;


        // localizer and feedback task variables
        private Random rnd = new Random();                                              // random object to create randomizations
        private long stateEndTime = 0;                                                  // endtime of a view state (can be the end of a condition or the end of a trialstate within a condition)
        private int currentFace = 0;                                                    // the face that is shown
        private int currentFaceEmotion = 0;                                             // the emotion of the face that is shown
        private int currentFaceTexture = 0;                                             // the texture that should be displayed (in the render function, updated by the update function)
        private Face[] faces = null;                                                    // array of face objects to display
        private int[] faceSequence = new int[0];                                        // sequence of faces for a block of faces (reshuffled at the start of every block or feedback trial phase)
        private int faceSequenceIndex = 0;                                              // the index for the sequence of faces (reset at the start of every block or feedphase trial phase)

        //
        //
        //



        public AmygView() : base(50, 0, 0, 640, 480, true) {
            initialize();
        }


        public AmygView(int updateFrequency, int x, int y, int width, int height, bool border) : base(updateFrequency, x, y, width, height, border) {
            initialize();
        }

        

        private void initialize() {

            // set the background color
            this.setBackgroundColor(0.0f, 0.0f, 0.0f);
            //this.setBackgroundColor(0.2470588f, 0.2470588f, 0.2470588f);

            // call resize to size the controls on display
            resize(getContentWidth(), getContentHeight());

        }

        public void printLinesCenter(string[] lines, glFreeTypeFont font) {

            // determine the line height
            float lineHeight = font.height + font.height * 0.5f;

            // calculate the height
            float textHeight = lines.Length * lineHeight;

            // calculate the top of the text
            float textTop = (getContentHeight() - textHeight) / 2;

            // loop through the lines
            for (int i = 0; i < lines.Length; i++) {
                if (string.IsNullOrEmpty(lines[i])) continue;

                // print the line
                int lineWidth = font.getTextWidth(lines[i]);
                textFont.printLine((getContentWidth() - lineWidth) / 2, textTop + i * lineHeight, lines[i]);

            }

        }

        ///////////////////////
        /// task functions
        //////////////////////

        public int getCondition() {

            // check if a task is currently viewed
            if (currentScene == Scenes.Localizer) {

                //
                return (int)locCurrentBlockCondition;

            } else if ( currentScene == Scenes.Feedback_Run1 ||
                        currentScene == Scenes.Feedback_Run2 ||
                        currentScene == Scenes.Feedback_Run3 ||
                        currentScene == Scenes.Transfer) {

                // 
                return (int)nfCurrentTrialCondition * 3 + nfTrialPhase;

            }

            // return -1 (nothing condition related is being viewed)
            return -1;
            
        }

        public string getGUIViewInfo() {
            string info = "";
            
            // scene
            info += "Scene: " + (int)currentScene;
            if (currentScene == Scenes.NoneOrNoSceneChange)     info += " (not set)";
            if (currentScene == Scenes.CalibrateView)           info += " (view localizer)";
            if (currentScene == Scenes.Localizer)               info += " (localizer task)";
            if (currentScene == Scenes.Feedback_Run1)           info += " (feedback run 1)";
            if (currentScene == Scenes.Feedback_Run2)           info += " (feedback run 2)";
            if (currentScene == Scenes.Feedback_Run3)           info += " (feedback run 3)";
            if (currentScene == Scenes.Transfer)                info += " (transfer)";

            info += Environment.NewLine;

            // check if a task is currently viewed
            if (currentScene == Scenes.Localizer) {

                if (locCurrentBlockCondition == CalBlockConditions.Rest)
                    info += "Condition: rest" + Environment.NewLine;
                else if (locCurrentBlockCondition == CalBlockConditions.FaceNeutral)
                    info += "Condition: face neutral" + Environment.NewLine;
                else
                    info += "Condition: face angry" + Environment.NewLine;

            } else if (currentScene == Scenes.Feedback_Run1 || currentScene == Scenes.Feedback_Run2 || currentScene == Scenes.Feedback_Run3 || currentScene == Scenes.Transfer) {

                if (nfCurrentTrialCondition == NfTrialConditions.NeutralView)
                    info += "Condition: Neutral View (0)" + Environment.NewLine;
                else if (nfCurrentTrialCondition == NfTrialConditions.AngryView)
                    info += "Condition: Angry view (1)" + Environment.NewLine;
                else
                    info += "Condition: Angry regulate (2)" + Environment.NewLine;
                

                if (nfTrialPhase == 0)
                    info += "Phase: fixation (0)" + Environment.NewLine;
                else if (nfTrialPhase == 1)
                    info += "Phase: instruction (1)" + Environment.NewLine;
                else
                    info += "Phase: feedback (2)" + Environment.NewLine;


            } else {
                // other

                info += "Condition: n/a" + Environment.NewLine;

            }


            // return text
            return info;
        }

        // Starts a scene. This function is called when the phase is started but before the actual scene is
        // started (gives the opportunity to show something before a task actually begins (without creating a new phase that preceeds the phase)
        public void startPreScene(int scene) {

            // set the new scene
            this.currentScene = (Scenes)scene;

        }

        public void startScene(int sceneIndex) {
            Scenes scene = (Scenes)sceneIndex;

            // clear the scene before setting a new scene
            // 
            // this function is called in one thread, while the view render process is in another
            // syncing would be a more elegant/perfect solution, however a lock would slow down the render loop
            this.currentScene = Scenes.NoneOrNoSceneChange;
            currentFaceTexture = 0;

            ////
            // 
            ////
            if (scene == Scenes.CalibrateView) {
                // scene - CalibrateView
                
                
            } else if (scene == Scenes.ScannerSetup || scene == Scenes.Break_1 ||
                        scene == Scenes.LocalizerInstructions || scene == Scenes.LocalizerDone ||
                        scene == Scenes.Feedback_Run1_Instructions || scene == Scenes.Feedback_Run2_Instructions || scene == Scenes.Feedback_Run3_Instructions ||
                        scene == Scenes.Feedback_Run1_Done || scene == Scenes.Feedback_Run2_Done || scene == Scenes.Feedback_Run3_Done ||
                        scene == Scenes.Transfer_Instructions || scene == Scenes.Transfer_Done ) {
                // Scene - ScannerSetup
                // Scene - Break 1
                // Scene - Feedback Instructions
                // Scene - Feedback done
                // Scene - Localizer Instructions
                // Scene - Localizer done
                // Scene - Feedback Instructions
                // Scene - Feedback done
                // Scene - Transfer Instruction
                // Scene - Transfer done


            } else if (scene == Scenes.Localizer) {
                // Scene - Localizer

                // generate a block sequence   // 0 = REST, 1 = NEUTRAL FACES, 2 = ANGRY FACES
                bool[] blockRnd = new bool[LOC_NUMBLOCKS];
                for (int i = 0; i < LOC_NUMBLOCKS / 2; i++) blockRnd[i] = true;
                blockRnd.Shuffle();
                locBlockSequence = new Block[LOC_NUMBLOCKS * 3 + 1];
                for (int iBlock = 0; iBlock < 8; iBlock++) {
                    locBlockSequence[iBlock * 3] = new Block(0, LOC_REST_DURATION);
                    locBlockSequence[iBlock * 3 + (blockRnd[iBlock] ? 1 : 2)] = new Block(1, LOC_FACES_NEUTRAL_DURATION);
                    locBlockSequence[iBlock * 3 + (blockRnd[iBlock] ? 2 : 1)] = new Block(2, LOC_FACES_ANGRY_DURATION);
                }
                locBlockSequence[LOC_NUMBLOCKS * 3] = new Block(0, LOC_REST_DURATION);         // end with rest

                // set the conditon 
                locBlockSequenceCounter = 0;
                locCurrentBlockCondition = (CalBlockConditions)locBlockSequence[0].condition;
                int currentBlockDuration = locBlockSequence[0].duration;

                // set the phase starttime
                phaseStartTimestamp = Stopwatch.GetTimestamp();

                // set the endTime for this state
                stateEndTime = phaseStartTimestamp + (int)(Stopwatch.Frequency * (currentBlockDuration / 1000.0));
                
                // build the header
                string runHeader = "Datetime: " + DateHelper.getDateTimeWithMs() + Environment.NewLine;
                runHeader += "Ticks per second: " + Stopwatch.Frequency + Environment.NewLine;
                runHeader += "Virtual screen: " + SystemInformation.VirtualScreen.Width + " x " + SystemInformation.VirtualScreen.Height + Environment.NewLine;
                runHeader += "Window: " + getWindowX() + " x " + getWindowY() + " - border: " + (hasBorder() ? "1" : "0") + Environment.NewLine;
                runHeader += "Content size: " + getContentWidth() + "x" + getContentHeight() + Environment.NewLine;
                runHeader += "FACE_DISPLAY_DURATION: " + FACE_DISPLAY_DURATION + Environment.NewLine;
                runHeader += "LOC_NUMBLOCKS: " + LOC_NUMBLOCKS + Environment.NewLine;
                runHeader += "LOC_REST_DURATION: " + LOC_REST_DURATION + Environment.NewLine;
                runHeader += "LOC_FACE_NEUTRAL_DURATION: " + LOC_FACES_NEUTRAL_DURATION + Environment.NewLine;
                runHeader += "LOC_FACE_ANGRY_DURATION: " + LOC_FACES_ANGRY_DURATION + Environment.NewLine;
                
                // get the time passed since the start of the phase (which is the onset)
                double timePassed = getTimePassedSincePhaseStart();

                // start the trial logging
                ViewData.newRun("Localizer", runHeader, new string[] { "timestamp", "timeFromStart", "condition", "conditionLabel", "duration" });

                // store first rest 
                ViewData.setTrialValue("timestamp", phaseStartTimestamp.ToString());
                ViewData.setTrialValue("timeFromStart", timePassed.ToString());
                ViewData.setTrialValue("condition", ((int)locCurrentBlockCondition).ToString());
                ViewData.setTrialValue("conditionLabel", locCurrentBlockCondition.ToString());
                ViewData.setTrialValue("duration", currentBlockDuration.ToString());
                ViewData.saveTrial();

                // update the blockSequence array with the onset
                locBlockSequence[locBlockSequenceCounter].onset = timePassed;

                // callback the mainthread and the experiment on a new trial start
                MainThread.processViewTrialStart((double)locCurrentBlockCondition);

            } else if (scene == Scenes.Feedback_Run1 || scene == Scenes.Feedback_Run2 || scene == Scenes.Feedback_Run3 || scene == Scenes.Transfer) {
                // Scene - Feedback

                // generate a sequence
                int[] blockRnd = new int[NF_NUMBLOCKS * 3];
                for (int i = 0; i < NF_NUMBLOCKS; i++) {
                    blockRnd[i * 3 + 1] = 1;
                    blockRnd[i * 3 + 2] = 2;
                }
                blockRnd.Shuffle();
                nfTrialSequence = new NfTrialConditions[blockRnd.Length];
                for (int i = 0; i < blockRnd.Length; i++) {
                    if (blockRnd[i] == 0)
                        nfTrialSequence[i] = NfTrialConditions.NeutralView;
                    else if (blockRnd[i] == 1)
                        nfTrialSequence[i] = NfTrialConditions.AngryRegulate;
                    else
                        nfTrialSequence[i] = NfTrialConditions.AngryView;
                }

                // reset the feedback buffers
                nfFeedbackBaseLineBuffer = new RingBuffer(3);           // now takes from the last 3 rest periods (/trials)
                nfFeedbackValue = 0;
                nfFeedbackBaseLineMean = 0;

                // set the trial index counter/condition/phase 
                nfTrialSequenceCounter = 0;
                nfCurrentTrialCondition = nfTrialSequence[0];
                nfTrialPhase = 0;

                // set the phase starttime
                phaseStartTimestamp = Stopwatch.GetTimestamp();

                // set the endTime for the fixation/rest state
                stateEndTime = Stopwatch.GetTimestamp() + (int)(Stopwatch.Frequency * (NF_FIXATION_DURATION / 1000.0));

                // build the header
                string runHeader = "Datetime: " + DateHelper.getDateTimeWithMs() + Environment.NewLine;
                runHeader += "Ticks per second: " + Stopwatch.Frequency + Environment.NewLine;
                runHeader += "Virtual screen: " + SystemInformation.VirtualScreen.Width + " x " + SystemInformation.VirtualScreen.Height + Environment.NewLine;
                runHeader += "Window: " + getWindowX() + " x " + getWindowY() + " - border: " + (hasBorder() ? "1" : "0") + Environment.NewLine;
                runHeader += "Content size: " + getContentWidth() + "x" + getContentHeight() + Environment.NewLine;
                runHeader += "FACE_DISPLAY_DURATION: " + FACE_DISPLAY_DURATION + Environment.NewLine;
                runHeader += "NF_NUMBLOCKS: " + NF_NUMBLOCKS + Environment.NewLine;
                runHeader += "NF_FIXATION_DURATION: " + NF_FIXATION_DURATION + Environment.NewLine;
                runHeader += "NF_INSTRUCTION_DURATION: " + NF_INSTRUCTION_DURATION + Environment.NewLine;
                runHeader += "NF_FEEDBACK_DURATION: " + NF_FEEDBACK_DURATION + Environment.NewLine;
                runHeader += "NF_FEEDBACK_UPDATEINTERVAL: " + NF_FEEDBACK_UPDATEINTERVAL + Environment.NewLine;

                // get the time passed since the start of the phase (which is the onset)
                double timePassed = getTimePassedSincePhaseStart();

                // start the trial logging
                ViewData.newRun("Neurofeedback", runHeader, new string[] { "timestamp", "timeFromStart", "trial", "condition", "conditionLabel" });

                // store first trial 
                ViewData.setTrialValue("timestamp", phaseStartTimestamp.ToString());
                ViewData.setTrialValue("timeFromStart", timePassed.ToString());
                ViewData.setTrialValue("trial", nfTrialSequenceCounter.ToString());
                ViewData.setTrialValue("condition", ((int)nfCurrentTrialCondition).ToString());
                ViewData.setTrialValue("conditionLabel", nfCurrentTrialCondition.ToString());
                ViewData.saveTrial();

                // callback the mainthread and the experiment on a new trial start
                MainThread.processViewTrialStart((double)nfCurrentTrialCondition);

            }

            // set the new scene to view
            this.currentScene = scene;

        }

        ///////////////////////
        /// openGL load and draw functions
        //////////////////////


        protected override void load() {

            // initialize the text font
            textFont.init(this, "fonts\\ariblk.ttf", (uint)(getContentHeight() / 10), "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789., -'");
            //textFont.init(this, "fonts\\ariblk.ttf", (uint)(getContentHeight() / 24), "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789., -'");

            // disable line smoothing
            glDisableLineSmooth();

            // load the faces
            string[] faceIDs = new string[] {   "BF_002", "BF_005", "BF_006", "BF_009", "BF_010", "BF_011", "BF_012", "BF_016", "BF_017", "BF_025", "BF_027", "BF_029", "BF_033", "BF_039", "BF_049",
                                                "BM_002", "BM_009", "BM_011", "BM_017", "BM_022", "BM_023", "BM_026", "BM_031", "BM_032", "BM_033", "BM_034", "BM_036", "BM_041", "BM_045", "BM_046",
                                                "WF_003", "WF_007", "WF_009", "WF_016", "WF_019", "WF_020", "WF_021", "WF_022", "WF_023", "WF_024", "WF_026", "WF_031", "WF_034", "WF_036", "WF_037",
                                                "WM_003", "WM_004", "WM_006", "WM_010", "WM_012", "WM_013", "WM_016", "WM_024", "WM_025", "WM_028", "WM_034", "WM_035", "WM_037", "WM_040", "WM_041"    };
            faces = new Face[faceIDs.Length];
            for (int i = 0; i < faceIDs.Length; i++) {
                faces[i] = new Face(faceIDs[i], new string[] {  "images\\Amyg\\" + faceIDs[i] + "\\Calm",
                                                                "images\\Amyg\\" + faceIDs[i] + "\\Angry" }, 
                                                                FACE_DISPLAY_DURATION);
                faces[i].load(this);
            }


            // determine which faces to use for localizer
            int[] seqBF = new int[15];
            int[] seqBM = new int[15];
            int[] seqWF = new int[15];
            int[] seqWM = new int[15];
            for (int i = 0; i < 15; i++) {
                seqBF[i] = i;
                seqBM[i] = i + 15;
                seqWF[i] = i + 30;
                seqWM[i] = i + 45;
            }
            seqBF.Shuffle();
            seqBM.Shuffle();
            seqWF.Shuffle();
            seqWM.Shuffle();
            locFaces = new int[20];
            for (int i = 0; i < 5; i++) {
                locFaces[i]         = seqBF[i];
                locFaces[i + 5]     = seqBM[i];
                locFaces[i + 10]    = seqWF[i];
                locFaces[i + 15]    = seqWM[i];
            }
            locFaces.Shuffle();
            

            // determine which faces to use for neurofeedback
            // (the ones not in localizer)
            nfFaces = new int[faces.Length - locFaces.Length];
            int nfFacesArrayCounter = 0;
            for (int i = 0; i < faces.Length; i++) {
                if (Array.IndexOf(locFaces, i) < 0) {
                    nfFaces[nfFacesArrayCounter] = i;
                    nfFacesArrayCounter++;
                }
            }
            
        }

        protected override void unload() {

            // loop through the faces and unload the textures
            for (int i = 0; i < faces.Length; i++) {
                faces[i].unload(this);
            }

            // clear the text font
            textFont.clean();

            // stop viewdata
            ViewData.destroy();

        }

        protected override void resize(int width, int height) {
			
            // calculate the face rectangle size and position
            faceRectHeight = (int)(height * (RECTANGLE_HEIGHT_PERC / 100));
            faceRectTop = height - faceRectHeight;
			faceRectTop += (int)(height * (RECTANGLE_Y_OFFSET_PERC / 100));
			faceRectWidth = (int)(faceRectHeight * 0.8);
			faceRectLeft = (width - faceRectWidth) / 2;
            
            // calculate the fixation dot position
            dotRadius = (height * (RECTANGLE_DOT_RADIUS_PERC / 100.0));
            dotLeft = faceRectLeft + ((faceRectWidth - dotRadius * 2) / 2) + dotRadius;
            dotTop = faceRectTop + ((faceRectHeight - dotRadius * 2) / 2) + dotRadius;
            dotTop += (int)(height * (RECTANGLE_DOT_Y_OFFSET_PERC / 100));

            // calculate the feedback bar width (for one bar) and lefts (for both bars)
            barRectWidth = faceRectHeight / 5;
            lbarRectLeft = faceRectLeft - barRectWidth - 2;
            rbarRectLeft = faceRectLeft + faceRectWidth + 2; 

        }
        

        /**
         *  function to update animation, called before render
         *  
         **/
        protected override void update(double secondsElapsed) {
            if (currentScene == Scenes.NoneOrNoSceneChange || currentScene == Scenes.Black)     return;

            // 
            if (currentScene == Scenes.Localizer) {
                // Scene - Localizer
                
                // update the localizer task
                processLocalizer();

                // check if we are in a condition where a face should be shown
                if (locCurrentBlockCondition != CalBlockConditions.Rest) {
                    // not rest, faces neutral or angry

                    // retrieve the face texture to show
                    currentFaceTexture = faces[currentFace].animate(currentFaceEmotion);
                    if (faces[currentFace].isFaceAnimationFinished(currentFaceEmotion)) {
                        // the animation of this face is finised

                        // select a new face (loop if necessary) and start the animation
                        faceSequenceIndex++;
                        if (faceSequenceIndex >= faceSequence.Length)   faceSequenceIndex = 0;
                        currentFace = faceSequence[faceSequenceIndex];
                        currentFaceTexture = faces[currentFace].resetAndStartAnimation(currentFaceEmotion, false);

                    }

                }

            } else if (currentScene == Scenes.Feedback_Run1 || currentScene == Scenes.Feedback_Run2 || currentScene == Scenes.Feedback_Run3 || currentScene == Scenes.Transfer) {
                // Scene - Feedback

                // update the feedback task 
                processFeedback();

                // check if we are in a trial phase where a face should be shown
                if (nfTrialPhase == 2) {
                    // feedback phase, not fixation or instruction

                    // retrieve the face texture to show
                    currentFaceTexture = faces[currentFace].animate(currentFaceEmotion);
                    if (faces[currentFace].isFaceAnimationFinished(currentFaceEmotion)) {
                        // the animation of this face is finised

                        // select a new face (loop if necessary) and start the animation
                        faceSequenceIndex++;
                        if (faceSequenceIndex >= faceSequence.Length) faceSequenceIndex = 0;
                        currentFace = faceSequence[faceSequenceIndex];
                        currentFaceTexture = faces[currentFace].resetAndStartAnimation(currentFaceEmotion, false);

                    }

                }

            }
        }

        /**
         *  render function, called after update
         *  
         **/
        protected override void render() {
            if (currentScene == Scenes.NoneOrNoSceneChange || currentScene == Scenes.Black) return;
            

            //
            // scenes
            // 
            
            if (currentScene == Scenes.ScannerSetup) {

                // print the text
                glColor3(1f, 1f, 1f);
                printLinesCenter(new string[] { "- Instellende scans (5 min) -",
                                                "",
                                                "Je hoeft nu even niets te doen",
                                                }, textFont);
                
            } else if (currentScene == Scenes.LocalizerInstructions) {

                printLinesCenter(new string[] { "- Lokalizatie taak (6 min) -",
                                                "",
                                                "Kijk steeds eerst naar de stip",
                                                "en dan naar de gezichten",
                                                }, textFont);

            } else if (currentScene == Scenes.Localizer) {

                if (locCurrentBlockCondition == CalBlockConditions.Rest) {
                    // rest
                    
                    // dot in the middle
                    glColor3(1f, 1f, 1f);
                    glBindTexture2D(0);
                    glBeginPolygon();
                    for (double i = 0; i < 2 * Math.PI; i += Math.PI / 6)
                        glVertex3(Math.Cos(i) * dotRadius + dotLeft, Math.Sin(i) * dotRadius + dotTop, 0.0);
                    glEnd();

                } else {
                    // faces

                    // select color
                    glColor3(1f, 1f, 1f);
                    
                    // set the texture    
                    glBindTexture2D(currentFaceTexture);
                    
                    // draw face
                    glBeginTriangles();

                        // vertex 0
                        glTexCoord2(1.0f, 1.0f);
                        glVertex3(faceRectLeft + faceRectWidth, 	faceRectTop + faceRectHeight, 	0.0f);

                        glTexCoord2(1.0f, 0.0f);
                        glVertex3(faceRectLeft + faceRectWidth, 	faceRectTop, 					0.0f);

                        glTexCoord2(0.0f, 0.0f);
                        glVertex3(faceRectLeft, 					faceRectTop, 					0.0f);

                        //vertex 1
                        glTexCoord2(0.0f, 1.0f);
                        glVertex3(faceRectLeft, 					faceRectTop + faceRectHeight, 	0.0f);

                        glTexCoord2(1.0f, 1.0f);
                        glVertex3(faceRectLeft + faceRectWidth, 	faceRectTop + faceRectHeight, 	0.0f);

                        glTexCoord2(0.0f, 0.0f);
                        glVertex3(faceRectLeft, 					faceRectTop, 					0.0f);

                    glEnd();

                }

            } else if ( currentScene == Scenes.LocalizerDone || 
                        currentScene == Scenes.Feedback_Run1_Done || currentScene == Scenes.Feedback_Run2_Done || currentScene == Scenes.Feedback_Run3_Done || currentScene == Scenes.Transfer_Done) {

                // print the text
                glColor3(1f, 1f, 1f);
                printLinesCenter(new string[] { "- Klaar -",
                                                "",
                                                "Einde van de taak, wacht de volgende instructie af",
                                                }, textFont);

            } else if (currentScene == Scenes.Break_1) {

                // print the text
                glColor3(1f, 1f, 1f);
                printLinesCenter(new string[] { "- Pauze (3 min) -",
                                                "",
                                                "Je hoeft nu even niets te doen",
                                                }, textFont);

            } else if ( currentScene == Scenes.Feedback_Run1_Instructions || 
                        currentScene == Scenes.Feedback_Run2_Instructions || 
                        currentScene == Scenes.Feedback_Run3_Instructions) {

                // print the text
                glColor3(1f, 1f, 1f);
                printLinesCenter(new string[] { "- Neurofeedback taak (10 min) -",
                                                "",
                                                "Je krijgt steeds een stip, een korte instructie",
                                                "('kijk' of 'reguleer') en dan neurofeedback",
                                                "",
                                                "- bij 'kijk', enkel naar de gezichten kijken",
                                                "- bij 'reguleer', kijk en reguleer omlaag",
                                                }, textFont);

            } else if (currentScene == Scenes.Transfer_Instructions) {

                // print the text
                glColor3(1f, 1f, 1f);
                printLinesCenter(new string[] { "- Transfer taak (10 min) -",
                                                "",
                                                "Je krijgt steeds een stip, een korte instructie",
                                                "('kijk' of 'reguleer') en dan neurofeedback",
                                                "",
                                                "- bij 'kijk', enkel naar de gezichten kijken",
                                                "- bij 'reguleer', kijk en reguleer omlaag",
                                                "",
                                                "Je krijg ditmaal geen feedback (balken) te zien.",
                                                }, textFont);

            } else if (currentScene == Scenes.Feedback_Run1 || currentScene == Scenes.Feedback_Run2 || currentScene == Scenes.Feedback_Run3 || currentScene == Scenes.Transfer) {

                if (nfTrialPhase == 0) {
                    // fixation

                    // dot in the middle
                    glColor3(1f, 1f, 1f);
                    glBindTexture2D(0);
                    glBeginPolygon();
                    for (double i = 0; i < 2 * Math.PI; i += Math.PI / 6)
                        glVertex3(Math.Cos(i) * dotRadius + dotLeft, Math.Sin(i) * dotRadius + dotTop, 0.0);
                    glEnd();

                } else if (nfTrialPhase == 1) {
                    // instruction

                    // determine the text
                    string conditionText = "Kijk";
                    if (nfCurrentTrialCondition == NfTrialConditions.AngryRegulate)     conditionText = "Reguleer";
                    
                    // print the text
                    glColor3(1f, 1f, 1f);
                    glBindTexture2D(0);
                    printLinesCenter(new string[] { conditionText }, textFont);


                } else {
                    // feedback

                    // feedback bars only on the feedback runs (not the transfer run
                    if (currentScene == Scenes.Feedback_Run1 || currentScene == Scenes.Feedback_Run2 || currentScene == Scenes.Feedback_Run3) {

                        // reset to empty texture
                        glBindTexture2D(0);

                        // convert percentage signal change to the bar percentage
                        //   |   |- bar 100% = 2.8;     =  2.1;
                        //   |   |
                        //   |   |- bar 30%  = 0;       =  0;
                        //   |   |
                        //   |   |- bar 0%   = -1.2;    = -0.9;
                        //double barValue = 30.0 + 25.0 * nfFeedbackValue;        // range -1.2 to 2.8
                        double barValue = 30.0 + (100.0/3.0) * nfFeedbackValue;        // range -.9 to 2.1
                        if (barValue > 100)     barValue = 100;
                        if (barValue < 0)       barValue = 0;

                        // determine the bar height and top
                        int barHeight = (int)Math.Round(faceRectHeight / 100.0 * barValue);
                        int barTop = faceRectTop + (faceRectHeight - barHeight);

                        // check if the there is a bar to draw                    
                        if (barHeight > 0) {
                        
                            // draw bars
                            glColor3(.6f, .6f, .6f);
                            //glColor3(1f, 0.9f, 0.0f);
                            glBeginTriangles();

                                // vertex 0 - left bar
                                glVertex3(lbarRectLeft + barRectWidth + 1, 	faceRectTop + faceRectHeight, 	0.0f);
                                glVertex3(lbarRectLeft + barRectWidth + 1, 	barTop, 						0.0f);
                                glVertex3(lbarRectLeft + 1, 				barTop, 						0.0f);

                                //vertex 1 - left bar
                                glVertex3(lbarRectLeft + 1, 				faceRectTop + faceRectHeight, 	0.0f);
                                glVertex3(lbarRectLeft + barRectWidth + 1, 	faceRectTop + faceRectHeight, 	0.0f);
                                glVertex3(lbarRectLeft + 1, 				barTop, 						0.0f);

                                // vertex 0 - right bar
                                glVertex3(rbarRectLeft + barRectWidth + 1, 	faceRectTop + faceRectHeight, 	0.0f);
                                glVertex3(rbarRectLeft + barRectWidth + 1, 	barTop, 						0.0f);
                                glVertex3(rbarRectLeft + 1, 				barTop, 						0.0f);

                                //vertex 1 - right bar
                                glVertex3(rbarRectLeft + 1, 				faceRectTop + faceRectHeight, 	0.0f);
                                glVertex3(rbarRectLeft + barRectWidth + 1, 	faceRectTop + faceRectHeight, 	0.0f);
                                glVertex3(rbarRectLeft + 1, 				barTop, 						0.0f);

                            glEnd();
                        }

                        // draw left bar outline
                        glColor3(1.0f, 1.0f, 1.0f);
                        drawRectangle(  lbarRectLeft, 					faceRectTop,
                                        lbarRectLeft + barRectWidth, 	faceRectTop + faceRectHeight, 
                                        3, 1.0f, 1.0f, 1.0f);
                        drawLine(lbarRectLeft - 10, 	faceRectTop + (int)((double)faceRectHeight / 1.428571428571429) + 1,
                                 lbarRectLeft, 		    faceRectTop + (int)((double)faceRectHeight / 1.428571428571429) + 1,
                                 5, false, 1.0f, 1.0f, 1.0f);

                        // draw right bar outline
                        glColor3(1.0f, 1.0f, 1.0f);
                        drawRectangle(  rbarRectLeft, faceRectTop,
                                        rbarRectLeft + barRectWidth, faceRectTop + faceRectHeight,
                                        3, 1.0f, 1.0f, 1.0f);
                        drawLine(rbarRectLeft + barRectWidth + 10,  faceRectTop + (int)((double)faceRectHeight / 1.428571428571429) + 1,
                                 rbarRectLeft + barRectWidth,       faceRectTop + (int)((double)faceRectHeight / 1.428571428571429) + 1,
                                 5, false, 1.0f, 1.0f, 1.0f);

                    }

                    // set normal color for face
                    glColor3(1f, 1f, 1f);

                    // set the texture    
                    glBindTexture2D(currentFaceTexture);

                    // draw face
                    glBeginTriangles();

                        // vertex 0
                        glTexCoord2(1.0f, 1.0f);
                        glVertex3(faceRectLeft + faceRectWidth, 	faceRectTop + faceRectHeight, 	0.0f);

                        glTexCoord2(1.0f, 0.0f);
                        glVertex3(faceRectLeft + faceRectWidth, 	faceRectTop, 					0.0f);

                        glTexCoord2(0.0f, 0.0f);
                        glVertex3(faceRectLeft, 					faceRectTop, 					0.0f);

                        //vertex 1
                        glTexCoord2(0.0f, 1.0f);
                        glVertex3(faceRectLeft, 					faceRectTop + faceRectHeight, 	0.0f);

                        glTexCoord2(1.0f, 1.0f);
                        glVertex3(faceRectLeft + faceRectWidth, 	faceRectTop + faceRectHeight, 	0.0f);

                        glTexCoord2(0.0f, 0.0f);
                        glVertex3(faceRectLeft, 					faceRectTop, 					0.0f);

                    glEnd();
                    
                }

            }
            
        }
        
        private void processLocalizer() {
            
            // check if the end time of the state has passed
            if (Stopwatch.GetTimestamp() >= stateEndTime) {

                // check if we are at the end of the task
                if (locBlockSequenceCounter >= locBlockSequence.Length - 1) {
                    // end of task

                    // set the scene to done
                    startScene((int)Scenes.LocalizerDone);

                } else {
                    // not at end of task

                    // get the time passed since the start of the phase (which is the onset)
                    double timePassed = getTimePassedSincePhaseStart();

                    // go to the next in the sequence
                    locBlockSequenceCounter++;

                    // update the blockSequence array with the onset of the next trial
                    locBlockSequence[locBlockSequenceCounter].onset = timePassed;

                    // set the condition of the next trial
                    locCurrentBlockCondition = (CalBlockConditions)locBlockSequence[locBlockSequenceCounter].condition;
                    int currentBlockDuration = locBlockSequence[locBlockSequenceCounter].duration;

                    // check if the next condition involves showing faces
                    if (locCurrentBlockCondition != CalBlockConditions.Rest) {
                        // not rest, faces neutral or angry

                        // generate a sequence of faces based on the faces meant for the localizer task (no repeats)
                        int locFacesNeeded = (int)Math.Ceiling((double)LOC_FACES_NEUTRAL_DURATION / FACE_DISPLAY_DURATION);     // assume equal timing for neutral and other conditions
                        faceSequence = locFaces.ExtendSequenceWithShuffledRepeat(locFacesNeeded, false);

                        // determine face, emotion and start the animation
                        faceSequenceIndex = 0;
                        currentFace = faceSequence[faceSequenceIndex];
                        if (locCurrentBlockCondition == CalBlockConditions.FaceNeutral)     currentFaceEmotion = 0;
                        if (locCurrentBlockCondition == CalBlockConditions.FaceAngry)       currentFaceEmotion = 1;
                        currentFaceTexture = faces[currentFace].resetAndStartAnimation(currentFaceEmotion, false);

                    }

                    // store new condition
                    ViewData.setTrialValue("timestamp", Stopwatch.GetTimestamp().ToString());
                    ViewData.setTrialValue("timeFromStart", timePassed.ToString());
                    ViewData.setTrialValue("condition", ((int)locCurrentBlockCondition).ToString());
                    ViewData.setTrialValue("conditionLabel", locCurrentBlockCondition.ToString());
                    ViewData.setTrialValue("duration", currentBlockDuration.ToString());
                    ViewData.saveTrial();
                    
                    // set the endtimes
                    stateEndTime = Stopwatch.GetTimestamp() + (int)(Stopwatch.Frequency * (currentBlockDuration / 1000.0));

                    // callback the mainthread and the experiment on a new trial start
                    MainThread.processViewTrialStart((double)locCurrentBlockCondition);

                }

            }   // end stateEndTime if
            
        }   // end function



        private void processFeedback() {

            // check if the end time of the state has passed
            if (Stopwatch.GetTimestamp() >= stateEndTime) {

                // select which phase of the trial
                if (nfTrialPhase == 0) {
                    // end of fixation
                    
                    // check if we are at the end of the task
                    if (nfTrialSequenceCounter >= nfTrialSequence.Length) {
                        // end of task
                        
                        // set the scene to done
                        if (currentScene == Scenes.Feedback_Run1)   startScene((int)Scenes.Feedback_Run1_Done);
                        if (currentScene == Scenes.Feedback_Run2)   startScene((int)Scenes.Feedback_Run2_Done);
                        if (currentScene == Scenes.Feedback_Run3)   startScene((int)Scenes.Feedback_Run3_Done);
                        if (currentScene == Scenes.Transfer)        startScene((int)Scenes.Transfer_Done);
                    
                    } else {
                        // not the end of the task

                        // go the the instruction phase
                        nfTrialPhase = 1;
                        
                        // set the endTime for the instruction state
                        stateEndTime = Stopwatch.GetTimestamp() + (int)(Stopwatch.Frequency * (NF_INSTRUCTION_DURATION / 1000.0));

                        // retrieve the feedback values from the mainthread (and thus from the experiment) 
                        // to get the feedback baseline mean
                        double[] feedbackValues = MainThread.getFeedbackValues();       // retrieves the full range of feedback values (currently 4?)
                        if (feedbackValues.Length > 0) {
                            nfFeedbackBaseLineBuffer.Put(feedbackValues.Average());
                            double[] baselineValues = nfFeedbackBaseLineBuffer.DataSequential();
                            nfFeedbackBaseLineMean = baselineValues.Average();
                        }

                    }

                } else if (nfTrialPhase == 1) {
                    // end of instruction
                    
                    // generate a sequence of faces based on the faces meant for the feedback task (no repeats)
                    int nfFacesNeeded = (int)Math.Ceiling((double)NF_FEEDBACK_DURATION / FACE_DISPLAY_DURATION);     // assume equal timing for neutral and other conditions
                    faceSequence = nfFaces.ExtendSequenceWithShuffledRepeat(nfFacesNeeded, false);

                    // determine face, emotion and start the animation
                    faceSequenceIndex = 0;
                    currentFace = faceSequence[faceSequenceIndex];
                    if (nfCurrentTrialCondition == NfTrialConditions.NeutralView)   currentFaceEmotion = 0;
                    if (nfCurrentTrialCondition == NfTrialConditions.AngryView)     currentFaceEmotion = 1;
                    if (nfCurrentTrialCondition == NfTrialConditions.AngryRegulate) currentFaceEmotion = 1;
                    currentFaceTexture = faces[currentFace].resetAndStartAnimation(currentFaceEmotion, false);

                    // go to the feedback phase
                    nfTrialPhase = 2;

                    // set the endTime for the feedback state
                    nfTrialPhaseEndTime = Stopwatch.GetTimestamp() + (int)(Stopwatch.Frequency * (NF_FEEDBACK_DURATION / 1000.0));
                    
                    // set the state endtime to the feedback update interval
                    stateEndTime = Stopwatch.GetTimestamp() + (int)(Stopwatch.Frequency * (NF_FEEDBACK_UPDATEINTERVAL / 1000.0));

                } else {
                    // end of feedback state (can be end of update interval or the feedback phase)

                    // check if the end time of the feedback passed
                    if (Stopwatch.GetTimestamp() >= nfTrialPhaseEndTime) {
                        //  end of feedback/trial

                        // go to the next trial
                        nfTrialSequenceCounter++;

                        // check if this is a valid trial (there is a next trial)
                        if (nfTrialSequenceCounter < nfTrialSequence.Length) {
                            // there is a next trial (not the end of the task)

                            // get the time passed since the start of the phase (which is the onset)
                            double timePassed = getTimePassedSincePhaseStart();

                            // set the condition of the next trial
                            nfCurrentTrialCondition = nfTrialSequence[nfTrialSequenceCounter];

                            // store the trial information
                            ViewData.setTrialValue("timestamp", phaseStartTimestamp.ToString());
                            ViewData.setTrialValue("timeFromStart", timePassed.ToString());
                            ViewData.setTrialValue("trial", nfTrialSequenceCounter.ToString());
                            ViewData.setTrialValue("condition", ((int)nfCurrentTrialCondition).ToString());
                            ViewData.setTrialValue("conditionLabel", nfCurrentTrialCondition.ToString());
                            ViewData.saveTrial();

                            // callback the mainthread and the experiment on a new trial start
                            MainThread.processViewTrialStart((double)nfCurrentTrialCondition);

                        }

                        // go to the fixation phase
                        nfTrialPhase = 0;

                        // set the endTime for the fixation state
                        stateEndTime = Stopwatch.GetTimestamp() + (int)(Stopwatch.Frequency * (NF_FIXATION_DURATION / 1000.0));

                    } else {
                        // end of update interval

                        // retrieve the feedback value from the mainthread (and thus from the experiment) 
                        nfFeedbackValue = MainThread.getFeedbackValue();    // returns the current smoothed value (amplitude determines over how many, 3?)
                        Console.WriteLine("nfFeedbackValue " + nfFeedbackValue);
                        // convert the feedback value (using the baseline mean) to the percentage signal change from baseline
                        nfFeedbackValue = nfFeedbackValue / nfFeedbackBaseLineMean * 100 - 100;
                        //Console.WriteLine("feedbackValue    " + feedbackValue);
                        Console.WriteLine("nfFeedbackValue " + nfFeedbackValue);

                        // set the state endtime to the feedback update interval
                        stateEndTime = Stopwatch.GetTimestamp() + (int)(Stopwatch.Frequency * (NF_FEEDBACK_UPDATEINTERVAL / 1000.0));

                        // check if the feedback update interval is after the feedback phase endtime
                        if (stateEndTime > nfTrialPhaseEndTime) {

                            // set the next state end time to the feedback phase endtime (which is shorter than the feedback update interval)
                            stateEndTime = nfTrialPhaseEndTime;

                        }

                    }

                }

            }

        }
        
        private double getTimePassedSincePhaseStart() {
            return (Stopwatch.GetTimestamp() - phaseStartTimestamp) / (double)Stopwatch.Frequency;
        }

        public Block[] getTrialSequence() {
            // If this function is called, then it should be called at the end of a task
            // 
            // The view only knowns startScene, so as long as no other scene was started
            // the currentScene should still be on the latest task)
            // 

            // check if a task is currently viewed
            if (currentScene == Scenes.Localizer) {

                // 
                int minus = 0;
                for (int i = 0; i < locBlockSequence.Length; i++) {
                    if (locBlockSequence[i].onset == 0) minus++;
                }
                Block[] retBlockSequence = new Block[locBlockSequence.Length - minus];
                int arrCounter = 0;
                for (int i = 0; i < locBlockSequence.Length; i++) {
                    if (locBlockSequence[i].onset != 0) {
                        retBlockSequence[arrCounter] = new Block();
                        retBlockSequence[arrCounter].condition = locBlockSequence[i].condition;
                        retBlockSequence[arrCounter].duration = locBlockSequence[i].duration;
                        retBlockSequence[arrCounter].onset = locBlockSequence[i].onset;
                        arrCounter++;
                    }
                }

                // return the localizer block sequence
                return retBlockSequence;

            }

            // return an empty block array
            return new Block[0];

        }

        /**
         * Face helper class for this view
         **/
        private class Face {
            public string id = "";
            private int animDuration = 0;                   // the duration for one face animation in milliseconds (for each emotion)

            private int[] currentFrameIndex = null;         // the current frame to be shown (for each emotion)
            private long[] currentFrameEndtime = null;      // the current endtime of the frame (for each emotion)
            private string[] imageFolder = null;            // the folders which contain the frames (for each emotion)
            private int[][] images = null;                  // the textures  (for each emotion)
            private int[] numberOfFrames = null;            // the number of frames (for each emotion)
            private int[] frameDuration = null;             // the ticks per seconds (!) a frame should be shown (based on the given duration and the number of frames in the folder),  (for each emotion)

            private bool[] animationEnded = null;           // whether a face animation has ended (or played once in the case of a looping animation)
            private bool[] loopAnimation = null;            // store whether the animation should be looped (for each emotion)


            /**
             *
             * animDuration     - the duration for one face animation should take in milliseconds
             **/
            public Face(string id, string[] emotionImageFolder, int animDuration) {
                this.id = id;
                this.animDuration = animDuration;
                this.imageFolder = emotionImageFolder;

                currentFrameIndex = new int[emotionImageFolder.Length];
                currentFrameEndtime = new long[emotionImageFolder.Length];
                images = new int[emotionImageFolder.Length][];
                numberOfFrames = new int[emotionImageFolder.Length];
                frameDuration = new int[emotionImageFolder.Length];
                animationEnded = new bool[emotionImageFolder.Length];
                loopAnimation = new bool[emotionImageFolder.Length];
            }

            /**
             * Load the images from the input folders as textures in memory
             **/
            public void load(OpenTKView view) {

                // loop through the emotion categories
                for (int emotionIndex = 0; emotionIndex < imageFolder.Length; emotionIndex++) {

                    // check if the folder exists
                    if (Directory.Exists(imageFolder[emotionIndex])) {

                        // list the files in the folder
                        string[] imageFilesPng = Directory.GetFiles(imageFolder[emotionIndex], "*.png", SearchOption.AllDirectories);
                        string[] imageFilesBmp = Directory.GetFiles(imageFolder[emotionIndex], "*.bmp", SearchOption.AllDirectories);
                        string[] imageFiles = new string[imageFilesPng.Length + imageFilesBmp.Length];
                        imageFilesPng.CopyTo(imageFiles, 0);
                        imageFilesBmp.CopyTo(imageFiles, imageFilesPng.Length);

                        // check if there are images
                        if (imageFiles.Length == 0) {

                            // message
                            logger.Warn("No images found in folder ('" + imageFolder[emotionIndex] + "') for face ' " + id + "' with emotionIndex " + emotionIndex);

                            // no images
                            images[emotionIndex] = null;
                            numberOfFrames[emotionIndex] = 0;

                        } else {
                            
                            // load the files in the folder to memory
                            images[emotionIndex] = new int[imageFiles.Length];
                            for (int i = 0; i < images[emotionIndex].Length; i++)
                                images[emotionIndex][i] = (int)view.loadImage(imageFiles[i], false, true);

                            // store the number of frames
                            numberOfFrames[emotionIndex] = images[emotionIndex].Length;

                            // calculate the duration of each frame (based on the total duration and the number of frames
                            frameDuration[emotionIndex] = (int)(Stopwatch.Frequency * ((double)animDuration / numberOfFrames[emotionIndex] / 1000.0));

                        }

                    } else {
                        // image folder does not exist
                        
                        // message
                        logger.Warn("Folder ('" + imageFolder[emotionIndex] + "') does not exist for face ' " + id + "' with emotionIndex " + emotionIndex);

                        // no images
                        images[emotionIndex] = null;
                        numberOfFrames[emotionIndex] = 0;
                    }

                }

            }

            /**
             * Unload the face textures from memory
             **/
            public void unload(OpenTKView view) {

                // loop through the emotion categories
                for (int emotionIndex = 0; emotionIndex < imageFolder.Length; emotionIndex++) {


                    // unload the images
                    if (images[emotionIndex] != null && images[emotionIndex].Length > 0) {
                        for (int i = 0; i < images[emotionIndex].Length; i++)
                            view.glDeleteTexture(images[emotionIndex][i]);
                    }

                }

            }

            /**
             * Should be called every update/render loop to animate the face
             * 
             * Returns the texture id that should be used upon the call
             * 
             **/
            public int animate(int emotionIndex) {
                if (images[emotionIndex] == null)   return 0;

                // check if the end time of current frame has passed
                if (currentFrameEndtime[emotionIndex] != 0 && Stopwatch.GetTimestamp() >= currentFrameEndtime[emotionIndex]) {

                    // check if this was the last frame in the animation
                    if (currentFrameIndex[emotionIndex] >= numberOfFrames[emotionIndex] - 1) {
                        // the last frame of the animation

                        // flag the animation as ended
                        animationEnded[emotionIndex] = true;

                        // check if the animation should be looped
                        if (loopAnimation[emotionIndex]) {

                            // start at the first frame again
                            currentFrameIndex[emotionIndex] = 0;

                            // set a new frame endtime based on the frame duration
                            currentFrameEndtime[emotionIndex] = Stopwatch.GetTimestamp() + frameDuration[emotionIndex];

                        } else {

                            // set the endtime to 0 (meaning effectively not checking)
                            currentFrameEndtime[emotionIndex] = 0;

                        }

                    } else {
                        // more frames in the animation

                        // go to the next frame
                        currentFrameIndex[emotionIndex]++;

                        // set a new frame endtime based on the frame duration
                        currentFrameEndtime[emotionIndex] = Stopwatch.GetTimestamp() + frameDuration[emotionIndex];

                    }

                }

                // return the texture to use
                return images[emotionIndex][currentFrameIndex[emotionIndex]];

            }

            /**
             * resets and starts the animation
             * 
             * Returns the texture id of the first frame in the animation
             **/
            public int resetAndStartAnimation(int emotionIndex, bool loopAnimation = false) {
                if (images[emotionIndex] == null)   return 0;

                // set the current frame index to the beginning
                currentFrameIndex[emotionIndex] = 0;

                // flag the animation as not ended
                animationEnded[emotionIndex] = false;

                // store whether the animation should be looped
                this.loopAnimation[emotionIndex] = loopAnimation;

                // set the endtime based on the frame duration
                currentFrameEndtime[emotionIndex] = Stopwatch.GetTimestamp() + frameDuration[emotionIndex];
                
                // return the texture to use
                return images[emotionIndex][currentFrameIndex[emotionIndex]];

            }

            /**
             * return whether the face animation is finished (or looped once)
             **/
            public bool isFaceAnimationFinished(int emotionIndex) {
                return animationEnded[emotionIndex];
            }

        }   // end face class

    }   // end ThremoView class

}
