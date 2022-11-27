/**
 * ApplePickView class
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
using RETIF4.Experiment;
using System;


namespace RETIF4.Views {
    
    public class ApplePickView : OpenTKView, IViewRF {
    //public class ApplePickView : SharpGLView, IViewRF {
            
        private static Logger logger = LogManager.GetLogger("View");                        // the logger object for the view

        public enum Scenes : int {
            NoneOrNoSceneChange = -1,           // reserved, always keep this
            BlackScreen = 0,                    // advised, keep this

            // define your own scenes
            BackgroundOnly = 1,
            TextOnly = 2,
            Feedback = 3,
            FeedbackDual = 4,

        };


        //
        Scenes currentScene = Scenes.NoneOrNoSceneChange;                              // the current scene which is being shown

        uint backgroundTexture = 0;                                                         // the background texture reference
        uint basketTexture = 0;                                                             // the basket texture reference
        double basketX = 0;





        public ApplePickView() : base(120, 0, 0, 640, 480, true) {
        
        }

        public ApplePickView(int updateFrequency, int x, int y, int width, int height, bool border) : base(updateFrequency, x, y, width, height, border) {
            
        }

        ///////////////////////
        /// task functions
        //////////////////////

        public int getCondition() {
            return 0;
        }

        public string getGUIViewInfo() {
            string info = "";
            
            // scene
            info += "Scene: " + (int)currentScene;
            if (currentScene == Scenes.NoneOrNoSceneChange)     info += " (not set)";
            if (currentScene == Scenes.BlackScreen)             info += " (black background)";
            if (currentScene == Scenes.BackgroundOnly)          info += " (background only)";
            if (currentScene == Scenes.Feedback)                info += " (neurofeedback)";
            
            info += Environment.NewLine;

            // condition
            info += "Condition: " + 0 + Environment.NewLine;

            // player
            info += "Player position: " + 0 + Environment.NewLine;


            return info;
        }

        // Starts a scene. This function is called when the phase is started but before the actual scene is
        // started (gives the opportunity to show something before a task actually begins (without creating a new phase that preceeds the phase)
        public void startPreScene(int scene) {

            // set the new scene
            this.currentScene = (Scenes)scene;

        }

        public void startScene(int scene) {

            // set the new scene
            this.currentScene = (Scenes)scene;

        }



        ///////////////////////
        /// openGL load and draw functions
        //////////////////////


        protected override void load() {
            
            backgroundTexture = loadImage(".\\images\\background.png");
            basketTexture = loadImage(".\\images\\basket.png");


        }

        protected override void unload() {


        }

        protected override void resize(int width, int height) {
            

        }

        protected override void update(double secondsElapsed) {
            //Console.WriteLine(secondsElapsed);
            basketX += (glControlWidth / 6.0) * secondsElapsed;
            if (basketX > glControlWidth) {
                basketX = 0;
            }
        }

        protected override void render() {
            
            ////
            // background
            ////
            if (currentScene < 0 || currentScene == Scenes.BlackScreen) {

                // scene - black screen
                // scene ....

                glBindTexture2D(0);
                glColor3(0f, 0f, 0f);
                glBeginQuads();
                    glVertex2(glControlWidth, glControlHeight);
                    glVertex2(0.0f, glControlHeight);
                    glVertex2(0.0f, 0.0f);
                    glVertex2(glControlWidth, 0.0f);
                glEnd();

            } else if ( currentScene == Scenes.BackgroundOnly || currentScene == Scenes.Feedback || currentScene == Scenes.FeedbackDual) {

                // Scene - background only
                // Scene - feedback
                // Scene - feedback dual

                // background
                glBindTexture2D(backgroundTexture);
                glColor3(1f, 1f, 1f);
                glBeginQuads();

                    glTexCoord2(1.0f, 1.0f);
                    glVertex2(glControlWidth, glControlHeight);

                    glTexCoord2(0.0f, 1.0f);
                    glVertex2(0.0f, glControlHeight);

                    glTexCoord2(0.0f, 0.0f);
                    glVertex2(0.0f, 0.0f);

                    glTexCoord2(1.0f, 0.0f);
                    glVertex2(glControlWidth, 0.0f);

                glEnd();

            }





            if (currentScene == Scenes.Feedback || currentScene == Scenes.FeedbackDual) {

            
                // basket
                glBindTexture2D(basketTexture);
                glColor3(1f, 1f, 1f);
                glBeginQuads();

                    glTexCoord2(1.0f, 1.0f);
                    glVertex2(basketX + glControlWidth / 6, glControlHeight / 6);

                    glTexCoord2(0.0f, 1.0f);
                    glVertex2(basketX, glControlHeight / 6);

                    glTexCoord2(0.0f, 0.0f);
                    glVertex2(basketX, 0.0f);

                    glTexCoord2(1.0f, 0.0f);
                    glVertex2(basketX + glControlWidth / 6, 0.0f);

                glEnd();
            
            }

        }

        //
        //
        //


        public Block[] getTrialSequence() {
            return null;
        }

    }

}
