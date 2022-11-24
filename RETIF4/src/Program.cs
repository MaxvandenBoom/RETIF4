/**
 * Program class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using System.Windows.Forms;
using System.Threading;
using NLog;
using System.Diagnostics;
using RETIF4.Nifti;
using System.Drawing;
using RETIF4.Experiment;
using RETIF4.GUI;
using RETIF4.Data;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using RETIF4.Helpers;

namespace RETIF4 {
    
    class Program {
        
        private const int ERROR_BAD_ENVIRONMENT = 0xA;

        private static Logger logger = LogManager.GetLogger("Program");

        [STAThread]
        static void Main(string[] args) {

            // retrieve the current culture
            CultureInfo culture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();

            // adjust decimal seperator and group seperator
            culture.NumberFormat.NumberDecimalSeparator = ".";
            culture.NumberFormat.NumberGroupSeparator = "";
            culture.NumberFormat.NumberGroupSizes = new int[] { 0 };

            // set the culture for every future thread and the current thread
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            
            /*
            // test if the nifti functions can be used
            try {

                NiftiDLL.n_GetBitVersion();
            } catch (Exception) {
                logger.Info("Unable to use nifti (dll) functions, exiting program with error.");
                MessageBox.Show("Unable to use nifti (dll) functions, exiting program with error.", "Program error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.ExitCode = ERROR_BAD_ENVIRONMENT;
                return;
            }

            // message
            if (Environment.Is64BitProcess) {
                logger.Info("Processes are run in a 64 bit environment");
            } else {
                logger.Info("Processes are run in a 32 bit environment");
            }
            logger.Info("Nifti functions use " + NiftiDLL.n_GetBitVersion() + "-bit dlls");
            */

            //NiftiDLL.ITKTest();

            /*

            //NiftiImage b = Nifti.n_ReadNifti_Safe(".\\templates\\func.img");
            
            //NiftiImage a = Nifti.n_ReadNifti_Safe(".\\templates\\ch2bet.nii");
            Stopwatch stopWatch = new Stopwatch();
            
            Bitmap bitmap = null;
            NiftiImage a = NiftiDLL.n_ReadNifti_Safe(".\\templates\\ch2bet.nii");
            //NiftiImage a = Nifti.n_ReadNifti_Safe(".\\templates\\func.img");
            a.determineNiftiRange();

            Console.WriteLine("a.getNiftiHighest: " + a.getNiftiHighest());
            Console.WriteLine("a.getNiftiLowest: " + a.getNiftiLowest());

            
            //byte[] test = new byte[6];
            //a.data = test;
            //a.nx = 2;
            //a.ny = 3;
            //a.nz = 1;
            
            
            
            //byte[] test = new byte[24];
            //test[0] = 255;
            //a.localDataType = NiftiImage.DT_BYTE;
            //a.byteData = test;
            //a.nx = 3;
            //a.ny = 4;
            //a.nz = 2;
            //a.setNiftiRange(0, 255);
             
                      
            //byte[] test = new byte[64];
            //a.data = test;
            //a.nx = 4;
            //a.ny = 4;
            //a.nz = 4;
            

            //byte[] test = new byte[24];
            //a.data = test;
            //a.nx = 3;
            //a.ny = 4;
            //a.nz = 2;
            
            double average = 0;
            double averageTicks = 0;
            int icounter = 0;
            for (icounter = 0; icounter < 20; icounter++) {
                stopWatch.Start();

                //NiftiRotationHelper.RotateAndFlipNiftiData(a, 0, NiftiRotationHelper.Rotations.Rot90CC, 0, false, false, false);
                
                //Nifti.n_ReadNifti("D:\\ch2bet.nii");
                //Nifti.n_test();

                
                
                bitmap = NiftiImageHelper.getYSliceAsBitmap24bit(a, 20, a.getNiftiLowest(), a.getNiftiHighest(), 0);

                stopWatch.Stop();
                
                TimeSpan ts = stopWatch.Elapsed;
                average += ts.Milliseconds;
                averageTicks += ts.Ticks;

                Console.WriteLine("RunTime s:" + ts.Seconds + "   ms:" + ts.Milliseconds + "     ticks:" + ts.Ticks);
                stopWatch.Reset();
                
            }
            average = average / icounter;
            averageTicks = averageTicks / icounter;
            Console.WriteLine("RunTime average ms: " + average);
            Console.WriteLine("RunTime average ticks: " + averageTicks);


            */













            /*
            string refPath = "D:\\G4\\realignment_volume.img";
            for (int i = 1; i < 30; i++) {

                string inPath = "D:\\G4\\cr_Timmy(" + i + ").img";
                string outPath = "D:\\G5\\cr_Timmy(" + i + ").img";
                string outPath2 = "D:\\G5\\acr_Timmy(" + i + ").img";

                //NiftiDLL.ITKRealign(refPath, inPath, outPath);

                File.Copy(inPath, outPath, true);
                File.Copy(inPath.Replace(".img",".hdr"), outPath.Replace(".img", ".hdr"), true);

                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "D:\\BCexec\\realign.exe";
                p.StartInfo.Arguments = "--fwhm 5 --interp 1 --quality 0.9 --sep 4 --reference " + refPath + " " + outPath + " " + outPath2;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();

                //Console.WriteLine(path);
            }
            */


            //double[] a = new double[] { 10, 11, 14, 16, 20, 21, 20, 21, 10, 21 };
            //double[] b = DetrendHelper.detrend(a, true, 200);













            
            // name this thread
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "Main/Experiment Thread";

            // create the experiment
            MainThread mainThread = new MainThread(new AmygExperiment());
            //MainThread mainThread = new MainThread(new ImageryExperiment());
            //MainThread mainThread = new MainThread(new ApplePickExperiment());

            // create the GUI interface object
            GUIMain gui = new GUIMain();
            
		    // create a GUI (as a separate process)
		    // and pass a reference to the experiment for the GUI to pull information from and push commands to the experiment object
            Thread thread = new Thread(() => {

                // name this thread
                if (Thread.CurrentThread.Name == null) {
                    Thread.CurrentThread.Name = "GUI Thread";
                }

                // setup the GUI
                Application.EnableVisualStyles();

                // message
                logger.Info("starting GUI (thread)");

                // start the GUI
                Application.Run(gui);

                // message
                logger.Info("GUI (thread) stopped");

            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            
            // wait for the GUI to start or a maximum amount of 5 seconds (5.000 / 50 = 100)
            int waitCounter = 100;
            while (!gui.isLoaded() && waitCounter > 0) {
                Thread.Sleep(50);
                waitCounter--;
            }

            // start the mainthread
            mainThread.run();

            // stop all the winforms (Informs all message pumps that they must terminate, and then closes all application windows after the messages have been processed.)
            Application.Exit();

            // exit the environment
            Environment.Exit(0);
            

        }
    }
}
