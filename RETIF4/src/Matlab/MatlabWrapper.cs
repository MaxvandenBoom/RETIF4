/**
 * MatlabWrapper class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using NLog;
using RETIF4.Helpers;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RETIF4.Matlab {

    public sealed class MatlabWrapper {

        public const string CONFIG_MATLAB_COMPROGID = "Matlab.Application.Single";  // specifies the Matlab COM Server Program ID. This will be used to see if matlab is installed (can be instantiated). Can also be used to force a specific typelibrary, eg. "Matlab.Application.Single.8.4"

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);
        
        [DllImport("user32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageClose(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern int SendMessageGetText(IntPtr hWnd, uint Msg, int wParam, StringBuilder strBuffer);       // get text

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern int SendMessageTextLength(IntPtr hWnd, uint Msg, int wParam, int lParam);                 // text length

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern Int32 SendMessageChar(IntPtr hWnd, uint Msg, uint wParam, int lParam);


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern long GetWindowText(IntPtr hwnd, StringBuilder lpString, long cch);

        [DllImport("User32.Dll")]
        public static extern Int32 PostMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);



        private const UInt32 VK_RETURN = 0x0D;

        private const UInt32 WM_CLOSE = 0x0010;
        private const UInt32 WM_GETTEXTLENGTH = 0x000E;
        private const UInt32 WM_GETTEXT = 0x000D;
        private const UInt32 WM_CHAR = 0x0102;
        private const UInt32 WM_KEYDOWN = 0x0100;

        private const int SW_RESTORE = 9;

        public const int MATLAB_UNAVAILABLE = -1;       // matlab cannot be found or is not available on the system
        public const int MATLAB_STOPPED = 0;            // matlab instance is not in existence
        public const int MATLAB_STOPPING = 1;           // matlab instance is being stopped
        public const int MATLAB_STARTED = 2;            // matlab instance is started and running
        public const int MATLAB_STARTING = 3;           // matlab instance is being started



        private static Logger logger = LogManager.GetLogger("Matlab");

        private static readonly MatlabWrapper instance = new MatlabWrapper();         // instance of this class (singleton)

        private bool matlabTypeFound = false;                           // flag whether the Matlab COM is found (determined during class construction)
        private int matlabState = MATLAB_UNAVAILABLE;                   // the state of the matlab COM instance

        private Type matlabType = null;                                 // the matlab COM type (determined during class construction)
        private dynamic matlabObject = null;                            // the matlab COM instance (after starting)
        private bool hWndMatlabFound = false;                           // flag whether the handle the matlab command window is found
        private bool hWndMatlabEditBoxFound = false;                    // flag whether the handle of the edit box in the matlab command window is found
        private IntPtr hWndMatlab = IntPtr.Zero;                        // handle of the matlab window
        private IntPtr hWndMatlabEditBox = IntPtr.Zero;                 // handle of the matlab window (only) edit box. If SendCommdandMethod is set to 1, this handle is used to send commands to and read output from.
        private int matlabSendCommandMethod = 0;                        // the method used to send commands to matlab. 0 = using the 'Execute' function on the matlab com object (will give no feedback in the command window/console);  1 = using sending text to the command window (gives feedback in the command window/console)
        private bool matlabShowCommandWindow = true;                    // setting whether to show the matlab command window
        private string matlabFuncDirectory = "";                        // folder 

        //private ArrayList commandQue = null;

        private MatlabWrapper() {

            // store the matlab function path
            matlabFuncDirectory = IOHelper.getProgramDirectory() + "matlab";

            try {

                // try to retrieve the Matlab type
                // If matlab is not installed or the type cannot be found for different reasons, this will fail
                matlabType = Type.GetTypeFromProgID(CONFIG_MATLAB_COMPROGID);

                if (matlabType != null) {
                    // Matlab type was found

                    // set type as available, thus able to use matlab
                    matlabState = MATLAB_STOPPED;
                    matlabTypeFound = true;
                    
                }

            } catch (Exception) { }

        }

        public static MatlabWrapper Instance {
            get {
                return instance;
            }
        }

        public static string getFunctionDirectory() {
            return instance.matlabFuncDirectory;
        }

        public static bool isMatlabAvailable() {
            return instance.matlabTypeFound;
        }

        public static void setSendCommandMethod(int sendCommandMethod) {

            // check if matlab is not started yet, message error if is is
            if (instance.matlabState == MATLAB_STARTING || instance.matlabState == MATLAB_STARTED || instance.matlabState == MATLAB_STOPPING) {
                logger.Error("Cannot change Matlab send command method while matlab is running.");
                return;
            }

            // store the method
            instance.matlabSendCommandMethod = sendCommandMethod;

        }
        public static void setShowCommandWindow(bool show) {

            // check if matlab is not started yet, message error if is is
            if (instance.matlabState == MATLAB_STARTING || instance.matlabState == MATLAB_STARTED || instance.matlabState == MATLAB_STOPPING) {
                logger.Error("Cannot change the show command windows setting while matlab is running.");
                return;
            }

            // store the method
            instance.matlabShowCommandWindow = show;

        }

        

        public static string getMatlabConsoleText() {
            if (instance.matlabState == MATLAB_STARTING  || instance.matlabState == MATLAB_STOPPED)  return "";

            if (instance.matlabSendCommandMethod == 0)
                return "COM object method is being used for sendCommand, this method provides no console feedback. \n\nEither this setting was configure like this in the experiment configuration file or the handle of the Matlab Command Window Editbox could not be found and the program fell back on the COM object method.";

            // check if there is no matlab edit box hwnd
            if (!instance.hWndMatlabEditBoxFound)
                return "Not available, could not found Matlab editbox hWnd";

            // try to retrieve the text
            string text = GetWindowText(instance.hWndMatlabEditBox);
            if (text == null)
                return "Not available, could not retrieve text from Matlab editbox hWnd";

            // return
            return text;

        }

        /**
         * return whether a matlab instance is started and running
         **/
        public static int getMatlabState() {

            // check if matlab does not exist on the system
            if (!instance.matlabTypeFound)                  return MATLAB_UNAVAILABLE;            

            // check if matlab is in the process of being started
            if (instance.matlabState == MATLAB_STARTING)    return MATLAB_STARTING;

            // check if matlab is in the process of being stopped
            if (instance.matlabState == MATLAB_STOPPING)    return MATLAB_STOPPING;

            // check if matlab was flagged as stopped
            if (instance.matlabState == MATLAB_STOPPED)     return MATLAB_STOPPED;

            // check if matlab should be started
            if (instance.matlabState == MATLAB_STARTED) {

                // check and return if a matlab instance is still running
                if (checkMatlabInstance()) {
                    return MATLAB_STARTED;
                } else {
                    return MATLAB_STOPPED;
                }

            }

            // 
            return MATLAB_UNAVAILABLE;

        }

        /**
         *  check if a matlab instance is running by window handle or object instance
         *  Returns true is running instance is available
         *  Returns false is the instance was not started, is in the process of stopping or died in the meantime
         **/
        private static bool checkMatlabInstance() {
            if (instance.matlabState == MATLAB_UNAVAILABLE)     return false;
            if (instance.matlabState == MATLAB_STOPPED)         return false;
            

            // check if matlab is in the process of stopping
            if (instance.matlabState == MATLAB_STOPPING)        return false;

            // check if the matlab window handle is available
            if (instance.hWndMatlabFound) {
                // check by window

                if (IsWindow(instance.hWndMatlab)) {

                    // return
                    return true;

                } else {
                    
                    // message
                    logger.Warn("Matlab should be running but was closed (by an external process), closing matlab references");

                    // dispose of matlab properly
                    closeMatlab();

                    // return
                    return false;

                }

            } else {
                // check by object instance
            
                try {

                    // try to retrieve the visible property, this will fail if matlab is closed
                    int ret = instance.matlabObject.visible;

                    // return
                    return true;

                } catch (Exception) {

                    // message
                    logger.Warn("Matlab should be running but was closed (by an external process), closing matlab references");

                    // dispose of matlab properly
                    closeMatlab();

                    // return
                    return false;

                }
            }

        }


        public static void closeMatlab() {
            if (instance.matlabState == MATLAB_UNAVAILABLE)     return;
            if (instance.matlabState == MATLAB_STOPPED)         return;

            // set the state to stopping
            instance.matlabState = MATLAB_STOPPING;
            
            //Console.WriteLine("stopping     state " + instance.matlabState);
            //Thread.Sleep(3000);
            //Console.WriteLine("stopping     state " + instance.matlabState);

            try {

                // try the normal call
                // (will still be active after this)
                instance.matlabObject.Quit();

                // send the quit command to make sure matlab quits
                instance.matlabObject.Execute("quit");

            } catch (System.Runtime.InteropServices.COMException) {
            } catch (Exception) {
            }

            //Console.WriteLine("stopping     state ");

            // clear the window handle settings
            instance.hWndMatlabFound = false;
            instance.hWndMatlabEditBoxFound = false;
            instance.hWndMatlab = IntPtr.Zero;
            instance.hWndMatlabEditBox = IntPtr.Zero;

            // check if there is a matlab window handle
            if (instance.hWndMatlabFound) {

                // close by window by handle
                // (it should/could already be closed, so expect an exception)
                try {

                    SendMessageClose(instance.hWndMatlab, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                } catch (Exception) { }

            }

            // release the com object
            try {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(instance.matlabObject);
            } catch {
            } finally {
                instance.matlabObject = null;
            }

            // clear the matlab object reference
            instance.matlabObject = null;

            // flag matlab as not started
            instance.matlabState = MATLAB_STOPPED;

            // message
            logger.Info("Matlab stopped");

        }


        public static void startMatlab() {
            if (instance.matlabState == MATLAB_UNAVAILABLE)     return;

            // check if matlab is already in the state of being started, if so return
            if (instance.matlabState == MATLAB_STARTING) {

                // message
                logger.Warn("Trying to start matlab while it is already busy starting");

                // return
                return;

            }

            // if the state is set to started, check if the matlab instance is indeed still running
            if (instance.matlabState == MATLAB_STARTED) {

                // check if the matlab instance is still running, if still running, then no need to start (return)
                if (checkMatlabInstance()) {

                    // message
                    logger.Warn("Trying to start matlab while it is already running");

                    // return
                    return;

                } else {

                    // message
                    logger.Warn("Matlab was supposed to be running but was closed (by an external process), restarting");

                }

            }

            // flag matlab as being started
            instance.matlabState = MATLAB_STARTING;

            // create a matlab COM instance
            instance.matlabObject = Activator.CreateInstance(instance.matlabType);
            if (instance.matlabObject == null) {

                // message
                logger.Error("Could not create Matlab instance");
                //MessageBox.Show("Could not create Matlab instance.", "Program error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // set the handle to the matlab window to zero
                instance.hWndMatlabFound = false;
                instance.hWndMatlabEditBoxFound = false;
                instance.hWndMatlab = IntPtr.Zero;
                instance.hWndMatlabEditBox = IntPtr.Zero;

                // flag matlab as not started
                instance.matlabState = MATLAB_STOPPED;

            } else {
                
                // hide the matlab instance (command window)
                if (!instance.matlabShowCommandWindow)
                    instance.matlabObject.visible = false;

                // try to find the window handle of the just created instance
                instance.hWndMatlab = FindWindow(null, "MATLAB Command Window");
                if (instance.hWndMatlab != IntPtr.Zero) {

                    // use the matlab window handle
                    instance.hWndMatlabFound = true;

                    // check if the command window is visible
                    if (instance.matlabShowCommandWindow) {

                        // active and display the matlab window (if the window is minimized or maximized, the system restores it to its original size and position)
                        ShowWindow(instance.hWndMatlab, SW_RESTORE);

                        // bring matlab to focus
                        //SetForegroundWindow(hWnd);                        

                        // position matlab at the bottom-right of the screen
                        Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;
                        int mtlb_x = 1200;
                        int mtlb_y = 0;
                        int mtlb_width = workingRectangle.Width - 1200;
                        int mtlb_height = workingRectangle.Height;
                        if (workingRectangle.Width < 1200 || mtlb_width < 40) {
                            mtlb_width = 300;
                            mtlb_height = 300;
                            mtlb_x = workingRectangle.Width - mtlb_width;
                            mtlb_y = workingRectangle.Height - mtlb_height;
                        }
                        MoveWindow(instance.hWndMatlab, mtlb_x, mtlb_y, mtlb_width, mtlb_height, true);

                    }

                    // try to find the edit box within the matlab window
                    instance.hWndMatlabEditBox = FindChildWindow(instance.hWndMatlab, "Edit", "");
                    if (instance.hWndMatlabEditBox != IntPtr.Zero) {

                        // use the matlab edit box handle
                        instance.hWndMatlabEditBoxFound = true;

                    } else {

                        // do not use the matlab edit box handle
                        instance.hWndMatlabEditBoxFound = false;

                    }

                } else {

                    // do no use the matlab window handle
                    instance.hWndMatlabFound = false;
                    instance.hWndMatlabEditBoxFound = false;

                }

                // check which sendcommand method should be used
                if (instance.matlabSendCommandMethod == 0) {
                    // 0 = using the 'Execute' function on the matlab com object (will give no feedback in the command window/console)

                    // message
                    logger.Info("COM object will be used for sendCommand (no console feedback matlab)");

                } else {
                    // 1 = using sending text to the command window (gives feedback in the command window/console)

                    // check if the editbox is found to send windows messages to
                    if (instance.hWndMatlabEditBoxFound) {
                        // editbox hwnd available

                        // message
                        logger.Info("Windows SendMessage and PostMessage APIs will be used for sendCommand");

                    } else {
                        // editbox hwnd not available

                        // message
                        logger.Warn("Matlab console window handles not available. Windows SendMessage and PostMessage APIs cannot be used for sendCommand, falling back on MATLAB com object (no console feedback)");

                        // set the method to 'Execute' function on the matlab com object
                        instance.matlabSendCommandMethod = 0;

                    }

                }

                // matlab, clear all previous variables
                MatlabWrapper.sendCommand("clear;");

                // matlab, make sure the matlab function that we want to use are included
                MatlabWrapper.sendCommand("addpath(genpath('" + MatlabWrapper.getFunctionDirectory() + "'));");

                // matlab, move to the matlab folder
                MatlabWrapper.sendCommand("cd '" + MatlabWrapper.getFunctionDirectory() + "';");

                // flag as matlab started
                instance.matlabState = MATLAB_STARTED;

                // message
                logger.Info("Matlab is started");

            }

        }

        public static int getIntVariable(string variableName) {
            if (instance.matlabState == MATLAB_UNAVAILABLE)     throw new System.Exception();

            if (instance.matlabState == MATLAB_STOPPED || !checkMatlabInstance()) {

                // message
                logger.Warn("Cannot get the value of variable '" + variableName + "', matlab no longer running");

                // return
                return -1;
                //throw new System.Exception();

            }

            // retrieve the value of a workspace variable
            int value = -1;
            try {
                value = (int)instance.matlabObject.GetVariable(variableName, "base");
            } catch (Exception e) {
                logger.Error("Exception while retrieving int variable ('" + variableName + "') from matlab: " + e.Message + ", returning -1");
                //throw new System.Exception();   // just as uninformative as the actual (COM) exception
            }
            
            return value;

        }
        
        public static double[] getDoubleArray(string variableName) {
            if (instance.matlabState == MATLAB_UNAVAILABLE) throw new System.Exception();

            if (instance.matlabState == MATLAB_STOPPED || !checkMatlabInstance()) {

                // message
                logger.Warn("Cannot get the value of variable '" + variableName + "', matlab no longer running");

                // return
                return null;
                //throw new System.Exception();

            }

            // retrieve the value of a workspace variable
            double[,] value = null;
            try {
                value = (double[,])instance.matlabObject.GetVariable(variableName, "base");
            } catch (Exception e) {

                logger.Error("Exception while retrieving double matrix variable ('" + variableName + "') from matlab: " + e.Message + ", returning null");
                //throw new System.Exception();   // just as uninformative as the actual (COM) exception
                return null;

            }

            // perform a blockcopy to make the data one-dimensional
            double[] arrValue = new double[value.Length];
            const int doubleSize = 8;   // the amount of bytes in a double
            Buffer.BlockCopy(value, 0, arrValue, 0, doubleSize * value.Length);

            // return the one-dimensional values
            return arrValue;

        }

        public static double[,] getDoubleMatrix(string variableName) {
            if (instance.matlabState == MATLAB_UNAVAILABLE) throw new System.Exception();

            if (instance.matlabState == MATLAB_STOPPED || !checkMatlabInstance()) {

                // message
                logger.Warn("Cannot get the value of variable '" + variableName + "', matlab no longer running");

                // return
                return null;
                //throw new System.Exception();

            }

            // retrieve the value of a workspace variable
            double[,] value = null;
            try {
                value = (double[,])instance.matlabObject.GetVariable(variableName, "base");
            } catch (Exception e) {

                logger.Error("Exception while retrieving double matrix variable ('" + variableName + "') from matlab: " + e.Message + ", returning null");
                //throw new System.Exception();   // just as uninformative as the actual (COM) exception
                return null;

            }
            
            return value;

        }

        private static Object commandLock = new Object();

        public static bool sendCommand(string command) {
            if (instance.matlabState == MATLAB_UNAVAILABLE)     return false;

            if (instance.matlabState == MATLAB_STOPPED || !checkMatlabInstance()) {

                // message
                logger.Warn("Cannot send command '" + command + "', matlab no longer running");

                // return
                return false;

            }
            lock (commandLock) {
                try {

                    // check which method to use to send the command
                    if (instance.matlabSendCommandMethod == 0) {
                        // matlab com object method

                        // send command
                        instance.matlabObject.Execute(command);

                    } else {
                        // send/post message method

                        // add the waiter variable before and after
                        command = "RF4_CF=0;" + command + "RF4_CF=1;";

                        // send command
                        for (int i = 0; i < command.Length; i++) {
                            char chrCommand = command[i];
                            SendMessageChar(instance.hWndMatlabEditBox, WM_CHAR, chrCommand, 0);
                        }
                        PostMessage(instance.hWndMatlabEditBox, WM_KEYDOWN, VK_RETURN, 1);

                        // wait for the command to finish
                        int commandFinished = 0;
                        while (commandFinished == 0) {
                            try {
                                commandFinished = MatlabWrapper.getIntVariable("RF4_CF");
                            } catch (Exception) {
                                commandFinished = 1;
                            }
                            Thread.Sleep(10);
                        }

                        // give additional sleep (else sometimes text gets mixed up)
                        Thread.Sleep(10);

                    }

                } catch (System.Runtime.InteropServices.COMException) {

                    // message
                    logger.Warn("Error executing command '" + command + "'");
                    //MessageBox.Show(ex.Message, ex.GetType().ToString());

                    return false;

                } catch (Exception) {

                    // message
                    logger.Warn("Error executing command '" + command + "'");
                    //MessageBox.Show(ex.Message, ex.GetType().ToString());

                    return false;

                }

                return true;

            } // end lock

        }


        /// <summary>
        /// Uses FindWindowEx() to recursively search for a child window with the given class and/or title.
        /// </summary>
        private static IntPtr FindChildWindow( IntPtr hwndParent, string lpszClass, string lpszTitle) {
            return FindChildWindow( hwndParent, IntPtr.Zero, lpszClass, lpszTitle );
        }

        /// <summary>
        /// Uses FindWindowEx() to recursively search for a child window with the given class and/or title,
        /// starting after a specified child window.
        /// If lpszClass is null, it will match any class name. It's not case-sensitive.
        /// If lpszTitle is null, it will match any window title.
        /// </summary>
        private static IntPtr FindChildWindow( IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszTitle) {
            // Try to find a match.
            IntPtr hwnd = FindWindowEx( hwndParent, IntPtr.Zero, lpszClass, lpszTitle );
            if ( hwnd == IntPtr.Zero ) {
                // Search inside the children.
                IntPtr hwndChild = FindWindowEx( hwndParent, IntPtr.Zero, null, null );
                while ( hwndChild != IntPtr.Zero && hwnd == IntPtr.Zero ) {
                    hwnd = FindChildWindow( hwndChild, IntPtr.Zero, lpszClass, lpszTitle );
                    if ( hwnd == IntPtr.Zero ) {
                        // If we didn't find it yet, check the next child.
                        hwndChild = FindWindowEx( hwndParent, hwndChild, null, null );
                    }
                }
            }
            return hwnd;
        }


        private static string GetWindowText(IntPtr hWnd) {

            // retrieve the hWnd text length
            int textLength = SendMessageTextLength(hWnd, WM_GETTEXTLENGTH, 0, 0);

            // if no length then return null
            if (textLength <= 0) return null;  // no text

            // build (copy) the string
            StringBuilder sb = new StringBuilder(textLength + 1);
            SendMessageGetText(hWnd, WM_GETTEXT, textLength + 1, sb);

            // return the text
            return sb.ToString();

        }


    }
}
