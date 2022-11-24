/**
 * GUIMain class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using NLog;
using NLog.Config;
using NLog.Windows.Forms;
using RETIF4.Nifti;
using RETIF4.Matlab;
using RETIF4.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using RETIF4.Data;
using System.IO;
using RETIF4.Helpers;
using NLog.Targets;
using NLog.Conditions;

namespace RETIF4.GUI {
    
    public partial class GUIMain : Form {
        
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        private const int WM_SETREDRAW = 0x0b;
        
        private static Logger logger;

        private List<Phase> phases = null;                      // 
        private IViewRF view = null;                            // reference to the view, used to pull information from and push commands to
        private bool loaded = false;                            // flag to hold whether the form is loaded

        private string matlabConsoleText = "";                  // the text in the matlab console (is stored here because as soon as it is put in a textbox, it changes and cannot be compared to determine whether it updated)

        private int numSessionTaskVolumeVariables = -1;         // hold the number of task volume variables in the session
        private int numSessionTaskTrialVariables = -1;          // hold the number of task trial variables in the session
        private string selectedSessionVolumeVariableName = "";  // the selected session volume variable
        private string selectedSessionTrialVariableName = "";   // the selected session trial variable

        private Rectangle repViewWindow = new Rectangle();                              // stores the representation of the view coordinates
        private int pnlViewWidth = 0;
        private int pnlViewHeight = 0;
        private int pnlViewLimitLeft = 0;
        private int pnlViewLimitRight = 0;
        private int pnlViewLimitTop = 0;
        private int pnlViewLimitBottom = 0;

        private ToolTip lblSessionVolumeInfoFilenameTooltip = new ToolTip();

        /**
         * GUI constructor
         * 
         * @param experiment	Reference to experiment, is used to pull information from and push commands to
         */
        public GUIMain() {
            
            // initialize form components
            InitializeComponent();

            // update the experiment information
            updateExperimentInformation();
            
            // retrieve/update the view information in the GUI
            updateViewInformation();

        }

        public bool isLoaded() {
            return loaded;
        }

        private void GUI_FormClosing(object sender, FormClosingEventArgs e) {

            // check whether the user is closing the form
            if (e.CloseReason == CloseReason.UserClosing) {

                // ask the user for confirmation
                if (MessageBox.Show("Are you sure you want to close?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) {
                    // user clicked no

                    // cancel the closing
                    e.Cancel = true;

                } else {

                    // message
                    logger.Info("User close GUI");

                    // continuing will close the form

                }

            }
            
            // check if the form is actually closing
            if (e.Cancel == false) {

                // set GUI update timers if necessary
                tmrExperimentUpdate.Enabled = false;
                tmrViewUpdate.Enabled = false;
                tmrSessionUpdate.Enabled = false;
                tmrMatlabUpdate.Enabled = false;

                // tell the experiment that the GUI is closed
                MainThread.eventGUIClosed();

            }

        }

        private void btnStartPhase_Click(object sender, EventArgs e) {

            // get the type chosen
            Phase.StartType type = Phase.StartType.IMMEDIATE___SCENE_IMMEDIATE;
            if (cmbPhaseStarttype.SelectedIndex == 1)   type = Phase.StartType.IMMEDIATE___SCENE_AT_INCOMING_SCAN;
            //if (cmbPhaseStarttype.SelectedIndex == 1)   type = Phase.StartType.PHASE_AT_INCOMING_SCAN___SCENE_NOW;
            //if (cmbPhaseStarttype.SelectedIndex == 2)   type = Phase.StartType.PHASE_AT_INCOMING_SCAN___SCENE_WITH_PHASE;
            //if (cmbPhaseStarttype.SelectedIndex == 3)   type = Phase.StartType.PHASE_AFTER_CURRENT_PHASE___SCENE_NOW;
            //if (cmbPhaseStarttype.SelectedIndex == 4)   type = Phase.StartType.PHASE_AFTER_CURRENT_PHASE___SCENE_WITH_PHASE;

            // log message
            string message = "User action - start phase " + phases[cmbPhase.SelectedIndex].phaseID + " (" + phases[cmbPhase.SelectedIndex].phaseName + ")";
            if (type == Phase.StartType.IMMEDIATE___SCENE_IMMEDIATE)                    message += ", start scene immediate";
            if (type == Phase.StartType.IMMEDIATE___SCENE_AT_INCOMING_SCAN)             message += ", start scene at first incoming scan";
            //if (type == Phase.StartType.PHASE_AT_INCOMING_SCAN___SCENE_NOW)             message += " at incoming scan, start scene now";
            //if (type == Phase.StartType.PHASE_AT_INCOMING_SCAN___SCENE_WITH_PHASE)      message += " at incoming scan, start scene together with phase";
            //if (type == Phase.StartType.PHASE_AFTER_CURRENT_PHASE___SCENE_NOW)          message += " after current phase, start scene now";
            //if (type == Phase.StartType.PHASE_AFTER_CURRENT_PHASE___SCENE_WITH_PHASE)   message += " after current phase, start scene together with phase";
            logger.Info(message);

            // retrieve the phase index and start the phase
            MainThread.startPhase(phases[cmbPhase.SelectedIndex], type);

        }

        private void btnViewStart_Click(object sender, EventArgs e) {
            
            if (view != null) {

                // log message
                logger.Info("User action - start view");

                // start the view
                view.start();

                // wait till the view is loaded or a maximum amount of 30 seconds (30.000 / 50 = 600)
                int waitCounter = 600;
                while(!view.isStarted() && waitCounter > 0) {
                    Thread.Sleep(50);
                    waitCounter--;
                }

                // retrieve/update the view information in the GUI
                updateViewInformation();

            }

        }

        private void updateExperimentInformation() {

            // check if there are no phases retrieved yet
            if (phases == null) {

                // retrieve the phases from the experiment
                phases = MainThread.getPhases();

                // check if there are phases defined
                if (phases != null && phases.Count > 0) {
                    // phases defined

                    // list the experiment phases in the dropdown    
                    for (int i = 0; i < phases.Count; i++) {
                        cmbPhase.Items.Add(phases[i].phaseID + ". " + phases[i].phaseName);
                    }

                    // select the first as standard
                    cmbPhase.SelectedIndex = 0;
                    
                } else {
                    // no phases defined

                    // set back to null (will keep checking)
                    phases = null;

                }

                // enable phase selection and start options
                cmbPhase.Enabled = true;
                btnPhaseStart.Enabled = true;
                cmbPhaseStarttype.Enabled = true;


            }

            // update phase
            Phase phase = MainThread.getCurrentPhase();
            if (phase == null) {
                lblCurrentPhase.Text = "";
            } else {
                lblCurrentPhase.Text = phase.phaseID + ". " + phase.phaseName;
            }

            // update status
            Status status = MainThread.getCurrentStatus();
            string statusText = "";
            //if (status == PHASE_STATUS.STATUS_DONE)          statusText = "Done";
            //if (status == PHASE_STATUS.STATUS_RUNNING)       statusText = "Running";
            //if (status == PHASE_STATUS.STATUS_STANDBY)       statusText = "Standby";
            //if (status == PHASE_STATUS.STATUS_STOPPED)       statusText = "Stopped";
            lblCurrentStatus.Text = statusText;
            
        }

        private void updateViewInformation() {

            // check if there is no view set, and try to retrieve the view
            if (view == null) {
                view = MainThread.getView();
                if (view != null) {
                    btnViewStart.Enabled = true;
                    grpViewPosSize.Enabled = true;
                }
            }
            
            // check whether the ratio's of both the screen and panel and calculate it's conversion factor
            if ((pnlViewPos.Width / (double)pnlViewPos.Height) < (SystemInformation.VirtualScreen.Width / (double)SystemInformation.VirtualScreen.Height)) {
                // base on panel width

                toPnlFactor = pnlViewPos.Width / (double)SystemInformation.VirtualScreen.Width;
                toViewFactor = SystemInformation.VirtualScreen.Width / (double)pnlViewPos.Width;

            } else {
                // base on panel height

                toPnlFactor = pnlViewPos.Height / (double)SystemInformation.VirtualScreen.Height;
                toViewFactor = SystemInformation.VirtualScreen.Height / (double)pnlViewPos.Height;

            }

            // reposition the screen in the panel
            picViewPosScreen.Size = new Size(   (int)Math.Round(SystemInformation.VirtualScreen.Width * toPnlFactor),
                                                (int)Math.Round(SystemInformation.VirtualScreen.Height * toPnlFactor) );
            picViewPosScreen.Location = new Point(  (int)Math.Round((pnlViewPos.Width - picViewPosScreen.Width) / 2.0),
                                                    (int)Math.Round((pnlViewPos.Height - picViewPosScreen.Height) / 2.0) );


            // return if no view
            if (view == null)   return;

            // check if the view is strted
            if (view.isStarted()) {

                // show the view in the panel
                if (!picViewPosWindow.Visible)  picViewPosWindow.Visible = true;

                // retrieve the view information
                txtViewInfo.Text = view.getGUIViewInfo();
          
                // disable the button (the view can only be started once)
                if (btnViewStart.Enabled)
                    btnViewStart.Enabled = false;

                // enable the windows width and height text fields
                if (!txtViewWindowWidth.Enabled) {
                    txtViewWindowWidth.Enabled = true;
                    txtViewWindowWidth.BackColor = Color.White;
                }
                if (!txtViewWindowHeight.Enabled) {
                    txtViewWindowHeight.Enabled = true;
                    txtViewWindowHeight.BackColor = Color.White;
                }
                // enable the view info
                if (!grpViewInfo.Enabled)   
                    grpViewInfo.Enabled = true;


                // retrieve the window and content properties
                int windowX = view.getWindowX();
                int windowY = view.getWindowY();
                int windowWidth = view.getWindowWidth();
                int windowHeight = view.getWindowHeight();
                int contentWidth = view.getContentWidth();
                int contentHeight = view.getContentHeight();
                bool hasBorder = view.hasBorder();

                // set the view window and content position and dimensions
                if (txtViewWindowX.BackColor == Color.White)
                    txtViewWindowX.Text = windowX.ToString();

                if (txtViewWindowY.BackColor == Color.White)
                    txtViewWindowY.Text = windowY.ToString();

                if (txtViewWindowWidth.BackColor == Color.White) {
                    if (windowWidth == 0)
                        txtViewWindowWidth.Text = "";
                    else
                        txtViewWindowWidth.Text = windowWidth.ToString();
                }

                if (txtViewWindowHeight.BackColor == Color.White) {
                    if (windowHeight == 0)
                        txtViewWindowHeight.Text = "";
                    else
                        txtViewWindowHeight.Text = windowHeight.ToString();
                }

                if (txtViewContentWidth.BackColor == Color.White)
                    txtViewContentWidth.Text = contentWidth.ToString();

                if (txtViewContentHeight.BackColor == Color.White)
                    txtViewContentHeight.Text = contentHeight.ToString();

                chkViewWindowBorder.Checked = hasBorder;


                // check if the window is not dragged
                if (!draggingViewWindow && !leftSizing && !rightSizing && !topSizing && !bottomSizing) {

                    // store the view window coordinates
                    repViewWindow.X = windowX;
                    repViewWindow.Y = windowY;
                    repViewWindow.Width = windowWidth;
                    repViewWindow.Height = windowHeight;

                    // set the window representation location and size
                    if (hasBorder) {
                        picViewPosWindow.Size = new Size(     (int)Math.Round(windowWidth * toPnlFactor),
                                                                (int)Math.Round(windowHeight * toPnlFactor)    );
                    } else {
                        picViewPosWindow.Size = new Size(     (int)Math.Round(contentWidth * toPnlFactor),
                                                                (int)Math.Round(contentHeight * toPnlFactor)    );
                    }
                    picViewPosWindow.Location = new Point(    (int)Math.Round((windowX * toPnlFactor) + picViewPosScreen.Location.X),
                                                                (int)Math.Round((windowY * toPnlFactor) + picViewPosScreen.Location.Y)   );
                
                }

            }
        }
        
        private void updateSessionInformation() {
            
            // check if the number of task volumes variables has changed
            if (lstSessionVolumeVariables.Items.Count == -1 || numSessionTaskVolumeVariables != Session.getNumberOfTaskVolumeVariables()) {

                // retrieve the volume variable names
                string[] taskVolumeVariableNames = Session.getTaskVolumeVariableNames();

                // set selectionmode to none (this prevents the first item from being selected)
                // and we use abuse this setting to stop the handling of selection events
                lstSessionVolumeVariables.SelectionMode = SelectionMode.None;

                // clear the list with names
                lstSessionVolumeVariables.Items.Clear();

                // (re-)populate the list
                lstSessionVolumeVariables.Items.Add("AllVolumes");
                lstSessionVolumeVariables.Items.Add("CorrVolume");
                lstSessionVolumeVariables.Items.Add("RealignVolume");
                lstSessionVolumeVariables.Items.Add("GlobalMask");
                lstSessionVolumeVariables.Items.Add("GlobalRTMask");
                lstSessionVolumeVariables.Items.Add("RoiMask");
                for (int i = 0; i < taskVolumeVariableNames.Length; i++) {
                    lstSessionVolumeVariables.Items.Add(taskVolumeVariableNames[i]);
                }

                // allow selection again (and the handling of selection events)
                lstSessionVolumeVariables.SelectionMode = SelectionMode.One;
                
                // re-select the item
                if (selectedSessionVolumeVariableName.Length != 0) {
                    int index = lstSessionVolumeVariables.FindString(selectedSessionVolumeVariableName);
                    if (index != -1) lstSessionVolumeVariables.SelectedIndex = index;
                }
                
                // update the amount of session task volume variables
                numSessionTaskVolumeVariables = taskVolumeVariableNames.Length;

            }

            // check if the number of task trial variables has changed
            if (lstSessionTrialVariables.Items.Count == -1 || numSessionTaskTrialVariables != Session.getNumberOfTaskTrialVariables()) {

                // retrieve the trial variable names
                string[] taskTrialVariableNames = Session.getTaskTrialVariableNames();

                // set selectionmode to none (this prevents the first item from being selected)
                // and we use abuse this setting to stop the handling of selection events
                lstSessionTrialVariables.SelectionMode = SelectionMode.None;

                // clear the list with names
                lstSessionTrialVariables.Items.Clear();

                // (re-)populate the list
                for (int i = 0; i < taskTrialVariableNames.Length; i++) {
                    lstSessionTrialVariables.Items.Add(taskTrialVariableNames[i]);
                }

                // allow selection again (and the handling of selection events)
                lstSessionTrialVariables.SelectionMode = SelectionMode.One;

                // re-select the item
                if (selectedSessionTrialVariableName.Length != 0) {
                    int index = lstSessionTrialVariables.FindString(selectedSessionTrialVariableName);
                    if (index != -1) lstSessionTrialVariables.SelectedIndex = index;
                }

                // update the amount of session task Trial variables
                numSessionTaskTrialVariables = taskTrialVariableNames.Length;

            }

            // update the list
            updateSessionVolumeVariableInformation();

        }


        private void updateSessionVolumeVariableInformation(bool newVariable = false, bool fullUpdate = false) {

            bool updateTimedataImage = false;

            // check if a task volume variable is selected
            if (selectedSessionVolumeVariableName.Length != 0) {

                // check if there is a new variable
                if (newVariable) {
                    
                    // set selectionmode to none (this prevents the first item from being selected)
                    lstSessionVolumes.SelectionMode = SelectionMode.None;

                    // clear the list
                    lstSessionVolumes.Items.Clear();

                }


                // check if one of the single volume variables is selected
                if (string.Compare(selectedSessionVolumeVariableName, "corrVolume", true) == 0 ||
                    string.Compare(selectedSessionVolumeVariableName, "realignVolume", true) == 0 ||
                    string.Compare(selectedSessionVolumeVariableName, "globalMask", true) == 0 ||
                    string.Compare(selectedSessionVolumeVariableName, "globalRTMask", true) == 0 ||
                    string.Compare(selectedSessionVolumeVariableName, "roiMask", true) == 0)        {
                    // single volume variables, always update full (since these can be set and overwritten at any time)

                    // switch to full update
                    fullUpdate = true;
                    
                }

                // check if we want to do a full update
                if (fullUpdate) {

                    // retrieve the volumes currently in the list
                    List<Volume> volumes = Session.getVolumes(selectedSessionVolumeVariableName);

                    // check if the items which are currently in the list have changed
                    for (int i = lstSessionVolumes.Items.Count - 1; i >= 0; i--) {
                        if (i >= volumes.Count || string.Compare(lstSessionVolumes.Items[i].ToString(), Volume.getVolumeGUIDisplayString(volumes[i])) != 0) {

                            // set selectionmode to none (this prevents the first item from being selected)
                            lstSessionVolumes.SelectionMode = SelectionMode.None;

                            // remove the items that are not in sync
                            lstSessionVolumes.Items.RemoveAt(i);

                        }
                    }

                    // check if the list is empty and there are volumes to add
                    if (lstSessionVolumes.Items.Count == 0 && volumes != null && volumes.Count > 0) {

                        // set selectionmode to none (this prevents the first item from being selected)
                        lstSessionVolumes.SelectionMode = SelectionMode.None;

                    }

                    // flag to update the timedata image
                    updateTimedataImage = true;

                }

                // not a full retrieve but instead incremental
                // mostly for multiple volume variables (since these grow but are unlikely to change entirely)
                    
                // check if the number of volumes is higher then the amount of volumes in the list
                if (Session.getNumberOfVolumes(selectedSessionVolumeVariableName) > lstSessionVolumes.Items.Count) {
                    // new volumes to be added to the list

                    List<Volume> volumes = Session.getVolumes(selectedSessionVolumeVariableName);
                    if (volumes != null) {

                        for (int i = lstSessionVolumes.Items.Count; i < volumes.Count; i++) {
                            lstSessionVolumes.Items.Add(Volume.getVolumeGUIDisplayString(volumes[i]));
                        }

                    }

                    // flag to update the timedata image
                    updateTimedataImage = true;

                }

                // check if we switched selection off (refreshed the list)
                if (lstSessionVolumes.SelectionMode == SelectionMode.None) {

                    // allow selection again (and the handling of selection events)
                    lstSessionVolumes.SelectionMode = SelectionMode.One;
                    
                }

            }

            // update the volume information
            updateSessionVolumeInformation();

            // update the volume datatime image (if needed)
            if (updateTimedataImage)    updateSessionVolumeTimedataImage();

        }

        private void updateSessionVolumeTimedataImage() {

            // create a bitmap image
            Bitmap bmp = new Bitmap(picSessionTimedata.Width, picSessionTimedata.Height);

            // build the image
            using (Graphics g = Graphics.FromImage(bmp)) {

                // clear with transparency
                g.Clear(Color.Transparent);

                // calculate the width and height of the actual graph (pic - space for labels)
                int graphLeft = 30;
                int graphWidth = bmp.Width - graphLeft;
                int graphHeight = bmp.Height - 30;

                // black out the graph region
                g.FillRectangle(Brushes.Black, graphLeft, 0, graphWidth, graphHeight);

                // check if there is more than one volume to display
                if (lstSessionVolumes.Items.Count > 1) {

                    List<Volume> volumes = Session.getVolumes(selectedSessionVolumeVariableName);

                    // determine the highest and lowest value for the roi
                    double lowestVal = (volumes.Count > 0 && volumes[0].roiValues != null ? volumes[0].roiValues.Average() : 0);
                    double highestVal = lowestVal;
                    for (int i = 0; i < volumes.Count - 1; i++) {
                        if (volumes[i].roiValues == null) continue;
                        double val = volumes[i].roiValues.Average();
                        if (val < lowestVal)    lowestVal = val;
                        if (val > highestVal)   highestVal = val;
                    }
                    double deltaVal = highestVal - lowestVal;

                    // calculate the volume spacing
                    double volSpacing = (double)graphWidth / (volumes.Count - 1);
                    Pen linePen = new Pen(Color.Yellow, 3);
                    Pen lineOrangePen = new Pen(Color.Orange, 2);

                    int lastCondition = volumes[0].condition;

                    // loop through the volumes for drawing
                    for (int i = 0; i < volumes.Count; i++) {
                        if (volumes[i].roiValues == null) continue;

                        // 
                        int x = (int)(i * volSpacing);

                        //
                        int y = graphHeight / 2;
                        if (deltaVal != 0) {
                            y = (int)((volumes[i].roiValues.Average() - lowestVal) / deltaVal * graphHeight);
                            y = graphHeight - y;
                        }


                        // check if the condition changed
                        if (volumes[i].condition != lastCondition) {

                            // update the condition
                            lastCondition = volumes[i].condition;

                            // draw a vertical line
                            g.DrawLine(lineOrangePen, graphLeft + x, 0, graphLeft + x, graphHeight);

                        }

                        // check if there is a previous volume
                        if (i < volumes.Count - 1) {

                            // 
                            int xNext = (int)((i + 1) * volSpacing);

                            //
                            int yNext = graphHeight / 2;
                            if (deltaVal != 0) {
                                yNext = (int)((volumes[i + 1].roiValues.Average() - lowestVal) / deltaVal * graphHeight);
                                yNext = graphHeight - yNext;
                            }

                            // draw a line between this and the next volume
                            g.DrawLine(linePen, graphLeft + x, y, graphLeft + xNext, yNext);

                        }

                        // draw a dot at the volume
                        g.FillEllipse(Brushes.Blue, graphLeft + x - 3, y - 3, 7, 7);


                    }





                }

            }


            // update the image
            picSessionTimedata.Image = bmp;
            picSessionTimedata.Refresh();

        }


        private void updateSessionVolumeInformation() {

            // check if a volume is selected
            if (lstSessionVolumes.SelectedItems.Count != 0) {

                // retrieve the selected volume
                List<Volume> volumes = Session.getVolumes(selectedSessionVolumeVariableName);
                if (volumes != null && volumes.Count != 0) {
                    Volume volume = volumes[lstSessionVolumes.SelectedIndex];

                    // set information
                    lblSessionVolumeInfoID.Text = volume.volumeId + " (" + volume.subDirNr + " / " + volume.volumeNr + ")";
                    lblSessionVolumeInfoIndex.Text = volume.volumeIndexInSession.ToString();
                    lblSessionVolumeInfoDateTime.Text = volume.dateTime + " (stamp: " + volume.timeStamp + ")";
                    lblSessionVolumeInfoCondition.Text = volume.condition.ToString();
                    lblSessionVolumeInfoFilename.Text = volume.filepath;

                    // add a tooltip to the filename, these can be long
                    lblSessionVolumeInfoFilenameTooltip.SetToolTip(lblSessionVolumeInfoFilename, volume.filepath);
                    
                    // prevent clearing
                    return;

                } 

            }

            // nothing selected, clear
            lblSessionVolumeInfoID.Text = "-";
            lblSessionVolumeInfoIndex.Text = "-";
            lblSessionVolumeInfoDateTime.Text = "-";
            lblSessionVolumeInfoCondition.Text = "-";
            lblSessionVolumeInfoFilename.Text = "-";
            lblSessionVolumeInfoFilenameTooltip.RemoveAll();
            
        }

        private void updateMatlabInformation() {

            if (MatlabWrapper.getMatlabState() == MatlabWrapper.MATLAB_STOPPED) {
                // stopped

                lblMatlabState.Text = "Stopped";
                
                btnMatlabStartStop.Enabled = true;
                btnMatlabStartStop.Text = "Start";

                txtMatlabConsole.Text = "";
                matlabConsoleText = "";
                txtMatlabConsole.Enabled = true;
                
            } else if (MatlabWrapper.getMatlabState() == MatlabWrapper.MATLAB_STOPPING) {
                // stopping

                lblMatlabState.Text = "Stopping";

                btnMatlabStartStop.Enabled = false;
                btnMatlabStartStop.Text = "Stop";

                // retrieve console text and update if necessary
                string text = MatlabWrapper.getMatlabConsoleText();
                if (!text.Equals(matlabConsoleText)) {
                    matlabConsoleText = text;
                    txtMatlabConsole.Text = text;
                }
                txtMatlabConsole.Enabled = true;

            } else if (MatlabWrapper.getMatlabState() == MatlabWrapper.MATLAB_STARTED) {
                // started
                
                lblMatlabState.Text = "Started";
                
                btnMatlabStartStop.Enabled = true;
                btnMatlabStartStop.Text = "Stop";

                // retrieve console text and update if necessary
                string text = MatlabWrapper.getMatlabConsoleText();
                if (!text.Equals(matlabConsoleText)) {
                    matlabConsoleText = text;
                    txtMatlabConsole.Text = text;
                }
                txtMatlabConsole.Enabled = true;

            } else if (MatlabWrapper.getMatlabState() == MatlabWrapper.MATLAB_STARTING) {
                // starting
                
                lblMatlabState.Text = "Starting";
                
                btnMatlabStartStop.Enabled = false;
                btnMatlabStartStop.Text = "Stop";

                txtMatlabConsole.Text = "";
                matlabConsoleText = "";
                txtMatlabConsole.Enabled = false;

            } else if (MatlabWrapper.getMatlabState() == MatlabWrapper.MATLAB_UNAVAILABLE) {
                // unavailable

                lblMatlabState.Text = "Unavailable";
                
                btnMatlabStartStop.Enabled = false;
                btnMatlabStartStop.Text = "Start";

                txtMatlabConsole.Text = "";
                matlabConsoleText = "";
                txtMatlabConsole.Enabled = false;

            }
        }
        RichTextBoxTarget rtbTarget = new RichTextBoxTarget();

        private void GUI_Load(object sender, EventArgs e) {

            // position the form at the bottom-right of the screen
            Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;
            this.Location = new System.Drawing.Point(0, workingRectangle.Height - this.Height);

            // Create logger
            logger = LogManager.GetLogger("GUI");
            LoggingConfiguration logConfig = LogManager.Configuration;
            
            rtbTarget.FormName = this.Name;
            rtbTarget.ControlName = "txtConsole";
            rtbTarget.Layout = "[${time}] ${logger}: ${message}";
            rtbTarget.UseDefaultRowColoringRules = true;
            rtbTarget.AutoScroll = true;
            txtConsole.ScrolledToBottom += txtConsole_ScrolledToBottom;
            txtConsole.ScrolledNotBottom += txtConsole_ScrolledNotBottom;

            // create and add highlight rules
            RichTextBoxRowColoringRule highlightPhaseStartedRule = new RichTextBoxRowColoringRule();
            highlightPhaseStartedRule.Condition = ConditionParser.ParseExpression("contains(message, 'starting phase')");
            highlightPhaseStartedRule.FontColor = "green";
            highlightPhaseStartedRule.Style = FontStyle.Bold;
            rtbTarget.RowColoringRules.Add(highlightPhaseStartedRule);

            RichTextBoxRowColoringRule highlightPhaseEndedRule = new RichTextBoxRowColoringRule();
            highlightPhaseEndedRule.Condition = ConditionParser.ParseExpression("contains(message, 'current phase')");
            highlightPhaseEndedRule.FontColor = "green";
            highlightPhaseEndedRule.Style = FontStyle.Bold;
            rtbTarget.RowColoringRules.Add(highlightPhaseEndedRule);

            // add the target
            logConfig.AddTarget("richTextBox", rtbTarget);
            //LoggingRule rule = new LoggingRule("*", LogLevel.Trace, rtbTarget);
            LoggingRule rule = new LoggingRule("*", LogLevel.Info, rtbTarget);
            logConfig.LoggingRules.Add(rule);
            LogManager.Configuration = logConfig;

            // log message
            logger.Debug("Logger connected to textbox");

            // set the form loaded flag to try
            loaded = true;

            // set GUI update timers if necessary
            tmrExperimentUpdate.Enabled = (tabControl.SelectedTab == tabExperiment);
            tmrViewUpdate.Enabled       = (tabControl.SelectedTab == tabView);
            tmrSessionUpdate.Enabled    = (tabControl.SelectedTab == tabSession);
            tmrMatlabUpdate.Enabled     = (tabControl.SelectedTab == tabMatlab);

            // one timer (GUI) is always running (regardless of which tab is being shown)
            tmrGUI.Enabled = true;

            // make sure the GUI toolbar is never out of reach (happens with small screens)
            if (this.Top < 0) this.Top = 0;

            // upon start, do one initial draw (will cause a black background) for the session timedata image
            updateSessionVolumeTimedataImage();

        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e) {

            // retrieve the information
            if (tabControl.SelectedTab == tabExperiment)    updateExperimentInformation();
            if (tabControl.SelectedTab == tabView)          updateViewInformation();
            if (tabControl.SelectedTab == tabSession) {
                updateSessionInformation();
                updateSessionVolumeVariableInformation(false, true);
            }
            if (tabControl.SelectedTab == tabMatlab)        updateMatlabInformation();

            // enable/disable the update timer
            tmrExperimentUpdate.Enabled = (tabControl.SelectedTab == tabExperiment);
            tmrViewUpdate.Enabled       = (tabControl.SelectedTab == tabView);
            tmrSessionUpdate.Enabled    = (tabControl.SelectedTab == tabSession);
            tmrMatlabUpdate.Enabled     = (tabControl.SelectedTab == tabMatlab);

        }

        private void tmrGUI_Tick(object sender, EventArgs e) {

            // determine whether matlab is not available
            if (tabMatlab.Enabled && !MatlabWrapper.isMatlabAvailable()) {
                // matlab is not available while the tab is still enabled

                // if the matlab tab is chosen, go to a different tab
                if (tabControl.SelectedTab == tabMatlab)
                    tabControl.SelectedTab = tabExperiment;

                // remove the control from the GUI
                tabControl.Controls.Remove(this.tabMatlab);
                
                // set the tab to false, so this part will only be executed once
                tabMatlab.Enabled = false;

            }

        }

        private void tmrExperimentUpdate_Tick(object sender, EventArgs e) {

            // retrieve the experiment information
            updateExperimentInformation();

        }

        private void tmrViewUpdate_Tick(object sender, EventArgs e) {

            // retrieve the view information
            updateViewInformation();

        }


        private void tmrSessionUpdate_Tick(object sender, EventArgs e) {

            // retrieve the view information
            updateSessionInformation();

        }

        private void tmrMatlabUpdate_Tick(object sender, EventArgs e) {

            // retrieve the matlab information
            updateMatlabInformation();

        }

        private void chkViewWindowBorder_CheckedChanged(object sender, EventArgs e) {
            if (view != null)   view.setBorder(chkViewWindowBorder.Checked);
        }

        private void btnViewSetWindow_Click(object sender, EventArgs e) {
            int correctX = -1;      // -1 do nothing with, 0 is change and incorrect, 1 is change and correct
            int correctY = -1;
            int correctWidth = -1;
            int correctHeight = -1;

            // check window x
            int x = 0;
            if (txtViewWindowX.BackColor != Color.White) {
                try {
                    x = Int32.Parse(txtViewWindowX.Text);
                    correctX = 1;
                } catch (Exception) {
                    txtViewWindowX.BackColor = Color.LightPink;
                    correctX = 0;
                }
            }

            // check window y
            int y = 0;
            if (txtViewWindowY.BackColor != Color.White) {
                try {
                    y = Int32.Parse(txtViewWindowY.Text);
                    correctY = 1;
                } catch (Exception) {
                    txtViewWindowY.BackColor = Color.LightPink;
                    correctY = 0;
                }
            }

            // check window width
            int width = 0;
            if (txtViewWindowWidth.BackColor != Color.White && txtViewWindowWidth.Enabled) {
                try {
                    width = Int32.Parse(txtViewWindowWidth.Text);
                    correctWidth = 1;
                    if (width <= 0)  throw new Exception();
                } catch (Exception) {
                    txtViewWindowWidth.BackColor = Color.LightPink;
                    correctWidth = 0;
                }
            }

            // check window height
            int height = 0;
            if (txtViewWindowHeight.BackColor != Color.White && txtViewWindowHeight.Enabled) {
                try {
                    height = Int32.Parse(txtViewWindowHeight.Text);
                    correctHeight = 1;
                    if (height <= 0) throw new Exception();
                } catch (Exception) {
                    txtViewWindowHeight.BackColor = Color.LightPink;
                    correctHeight = 0;
                }
            }

            // if to be corrected, set the focus and selection to one of the fields
            if (correctX == 0) {
                txtViewWindowX.Focus();
                txtViewWindowX.SelectAll();
            } else if (correctY == 0) {
                txtViewWindowY.Focus();
                txtViewWindowY.SelectAll();
            } else if (correctWidth == 0) {
                txtViewWindowWidth.Focus();
                txtViewWindowWidth.SelectAll();
            } else if (correctHeight == 0) {
                txtViewWindowHeight.Focus();
                txtViewWindowHeight.SelectAll();
            }

            // check if all fields are correct
            if (correctX != 0 && correctY != 0 && correctWidth != 0 && correctHeight != 0 && view != null) {

                // apply the adjustments
                if (correctX == -1 && correctY == 1)                view.setWindowLocation(view.getWindowX(), y);
                else if (correctX == 1 && correctY == -1)           view.setWindowLocation(x, view.getWindowY());
                else if (correctX == 1 && correctY == 1)            view.setWindowLocation(x, y);

                if (correctWidth == -1 && correctHeight == 1)       view.setWindowSize(view.getWindowWidth(), height);
                else if (correctWidth == 1 && correctHeight == -1)  view.setWindowSize(width, view.getWindowHeight());
                else if (correctWidth == 1 && correctHeight == 1)   view.setWindowSize(width, height);

                // reset the background colors
                txtViewWindowX.BackColor = Color.White;
                txtViewWindowY.BackColor = Color.White;
                if (txtViewWindowWidth.Enabled) txtViewWindowWidth.BackColor = Color.White;
                if (txtViewWindowHeight.Enabled) txtViewWindowHeight.BackColor = Color.White;

                txtViewContentWidth.BackColor = Color.White;
                txtViewContentHeight.BackColor = Color.White;

            }

        }

        private void btnViewSetContent_Click(object sender, EventArgs e) {
            int correctWidth = -1;      // -1 do nothing with, 0 is change and incorrect, 1 is change and correct
            int correctHeight = -1;


            // check content width
            int width = 0;
            if (txtViewContentWidth.BackColor != Color.White) {
                try {
                    width = Int32.Parse(txtViewContentWidth.Text);
                    correctWidth = 1;
                    if (width <= 0)  throw new Exception();
                } catch (Exception) {
                    txtViewContentWidth.BackColor = Color.LightPink;
                    correctWidth = 0;
                }
            }

            // check window height
            int height = 0;
            if (txtViewContentHeight.BackColor != Color.White) {
                try {
                    height = Int32.Parse(txtViewContentHeight.Text);
                    correctHeight = 1;
                    if (height <= 0) throw new Exception();
                } catch (Exception) {
                    txtViewContentHeight.BackColor = Color.LightPink;
                    correctHeight = 0;
                }
            }


            // check if all fields are correct
            if (correctWidth != 0 && correctHeight != 0 && view != null) {

                // apply the adjustments
                if (correctWidth == -1 && correctHeight == 1)       view.setContentSize(view.getContentWidth(), height);
                else if (correctWidth == 1 && correctHeight == -1)  view.setContentSize(width, view.getContentHeight());
                else if (correctWidth == 1 && correctHeight == 1)   view.setContentSize(width, height);

                // reset the background colors
                if (txtViewWindowWidth.Enabled)     txtViewWindowWidth.BackColor = Color.White;
                if (txtViewWindowHeight.Enabled)    txtViewWindowHeight.BackColor = Color.White;

                txtViewContentWidth.BackColor = Color.White;
                txtViewContentHeight.BackColor = Color.White;

            }

        }

        private void txtViewWindowX_KeyDown(object sender, KeyEventArgs e) {
            bool validInput = (e.KeyCode == Keys.Home || e.KeyCode == Keys.End || e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back || e.KeyCode == Keys.OemMinus || (e.KeyValue >= 48 && e.KeyValue <= 57));
            e.SuppressKeyPress = !validInput;

            if (validInput)
                txtViewWindowX.BackColor = Color.LemonChiffon;

        }

        private void txtViewWindowY_KeyDown(object sender, KeyEventArgs e) {
            bool validInput = (e.KeyCode == Keys.Home || e.KeyCode == Keys.End || e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back || e.KeyCode == Keys.OemMinus || (e.KeyValue >= 48 && e.KeyValue <= 57));
            e.SuppressKeyPress = !validInput;

            if (validInput)
                txtViewWindowY.BackColor = Color.LemonChiffon;

        }

        private void txtViewWindowWidth_KeyDown(object sender, KeyEventArgs e) {
            bool validInput = (e.KeyCode == Keys.Home || e.KeyCode == Keys.End || e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back || e.KeyCode == Keys.OemMinus || (e.KeyValue >= 48 && e.KeyValue <= 57));
            e.SuppressKeyPress = !validInput;

            if (validInput)
                txtViewWindowWidth.BackColor = Color.LemonChiffon;

        }

        private void txtViewWindowHeight_KeyDown(object sender, KeyEventArgs e) {
            bool validInput = (e.KeyCode == Keys.Home || e.KeyCode == Keys.End || e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back || e.KeyCode == Keys.OemMinus || (e.KeyValue >= 48 && e.KeyValue <= 57));
            e.SuppressKeyPress = !validInput;

            if (validInput)
                txtViewWindowHeight.BackColor = Color.LemonChiffon;

        }

        private void txtViewContentWidth_KeyDown(object sender, KeyEventArgs e) {
            bool validInput = (e.KeyCode == Keys.Home || e.KeyCode == Keys.End || e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back || e.KeyCode == Keys.OemMinus || (e.KeyValue >= 48 && e.KeyValue <= 57));
            e.SuppressKeyPress = !validInput;

            if (validInput)
                txtViewContentWidth.BackColor = Color.LemonChiffon;

        }

        private void txtViewContentHeight_KeyDown(object sender, KeyEventArgs e) {
            bool validInput = (e.KeyCode == Keys.Home || e.KeyCode == Keys.End || e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back || e.KeyCode == Keys.OemMinus || (e.KeyValue >= 48 && e.KeyValue <= 57));
            e.SuppressKeyPress = !validInput;

            if (validInput)
                txtViewContentHeight.BackColor = Color.LemonChiffon;

        }

        private void cmbPhase_SelectedIndexChanged(object sender, EventArgs e) {

            // retrieve the starttype of the phase
            Phase.StartType type = phases[cmbPhase.SelectedIndex].getStartType();

            // (re-)create the list of starttype possibilities
            cmbPhaseStarttype.Items.Clear();
            cmbPhaseStarttype.Items.Add("Immediate, start scene immediate"          + (type == Phase.StartType.IMMEDIATE___SCENE_IMMEDIATE ? " *" : ""));
            cmbPhaseStarttype.Items.Add("Immediate, start scene at first incoming scan"   + (type == Phase.StartType.IMMEDIATE___SCENE_AT_INCOMING_SCAN ? " *" : ""));
            //cmbPhaseStarttype.Items.Add("Phase at incoming scan, start scene now"                       + (type == Phase.StartType.PHASE_AT_INCOMING_SCAN___SCENE_NOW ? " *" : ""));
            //cmbPhaseStarttype.Items.Add("Phase at incoming scan, start scene together with phase"       + (type == Phase.StartType.PHASE_AT_INCOMING_SCAN___SCENE_WITH_PHASE ? " *" : ""));
            //cmbPhaseStarttype.Items.Add("Phase after current phase, start scene now"                    + (type == Phase.StartType.PHASE_AFTER_CURRENT_PHASE___SCENE_NOW ? " *" : ""));
            //cmbPhaseStarttype.Items.Add("Phase after current phase, start scene together with phase"    + (type == Phase.StartType.PHASE_AFTER_CURRENT_PHASE___SCENE_WITH_PHASE ? " *" : ""));

            // set the standard starttype in the combobox
            if (type == Phase.StartType.IMMEDIATE___SCENE_IMMEDIATE)                                      cmbPhaseStarttype.SelectedIndex = 0;
            if (type == Phase.StartType.IMMEDIATE___SCENE_AT_INCOMING_SCAN)                               cmbPhaseStarttype.SelectedIndex = 1;
            //if (type == Phase.StartType.PHASE_AT_INCOMING_SCAN___SCENE_NOW)             cmbPhaseStarttype.SelectedIndex = 1;
            //if (type == Phase.StartType.PHASE_AT_INCOMING_SCAN___SCENE_WITH_PHASE)      cmbPhaseStarttype.SelectedIndex = 2;
            //if (type == Phase.StartType.PHASE_AFTER_CURRENT_PHASE___SCENE_NOW)          cmbPhaseStarttype.SelectedIndex = 3;
            //if (type == Phase.StartType.PHASE_AFTER_CURRENT_PHASE___SCENE_WITH_PHASE)   cmbPhaseStarttype.SelectedIndex = 4;

        }
        
        private void button1_Click(object sender, EventArgs e) {
           
        }

        private void btnMatlabStartStop_Click(object sender, EventArgs e) {         

            if (MatlabWrapper.getMatlabState() == MatlabWrapper.MATLAB_STARTED) {

                // message
                logger.Info("Manually stopping matlab");

                // close matlab
                var thread = new Thread(() => {
                    MatlabWrapper.closeMatlab();
                });
                thread.Name = "Matlab start/stop thread";
                thread.Start();

            } else if(MatlabWrapper.getMatlabState() == MatlabWrapper.MATLAB_STOPPED) {

                // message
                logger.Info("Manually starting matlab");

                // start matlab
                var thread = new Thread(() => {
                    MatlabWrapper.startMatlab();
                });
                thread.Name = "Matlab start/stop thread";
                thread.Start();

            }

            // immediately after starting the thread update the information
            updateMatlabInformation();

        }

        private void button2_Click(object sender, EventArgs e)
        {

            MatlabWrapper.sendCommand("a = 10;");
            MatlabWrapper.sendCommand("a");


            int a = 0;
            try {
            a = MatlabWrapper.getIntVariable("a");
            } catch (Exception) {}
            Console.WriteLine("-- " + a);
            

        }

        private void lstSessionVolumeVariables_SelectedIndexChanged(object sender, EventArgs e) {
            
            // check if we are handling selection events (abusing the selectionmode setting for this)
            if (lstSessionVolumeVariables.SelectionMode != SelectionMode.None) {
                
                // store the selected item
                selectedSessionVolumeVariableName = lstSessionVolumeVariables.SelectedItem.ToString();
            }

            // update the variable information (full update)
            updateSessionVolumeVariableInformation(true, true);

        }

        private void lstSessionVolumes_SelectedIndexChanged(object sender, EventArgs e) {

            // check if we are handling selection events (abusing the selectionmode setting for this)
            if (lstSessionVolumes.SelectionMode != SelectionMode.None) {

                // update the volume information
                updateSessionVolumeInformation();

            }

        }

        private void btnSessionVolumeVariableImport_Click(object sender, EventArgs e) {

            // open file dialog to open file
            OpenFileDialog dlgLoadSessionFile = new OpenFileDialog();

            // set initial directory
            //dlgLoadSessionFile.InitialDirectory = Directory.GetCurrentDirectory();
            dlgLoadSessionFile.Filter = "Session files (*.txt; *.dat)|*.txt;*.dat|All files (*.*)|*.*";
            dlgLoadSessionFile.RestoreDirectory = true;

            // check if ok has been clicked on the dialog
            if (dlgLoadSessionFile.ShowDialog() == DialogResult.OK) {
                
                // try to retrieve the variables
                string[] volumeVariables = Session.listSessionVariableNamesFromFile(dlgLoadSessionFile.FileName, true, false);
                if (volumeVariables == null || volumeVariables.Length == 0) {

                    // message and return
                    logger.Error("No volume variables found in file '" + dlgLoadSessionFile.FileName + "'");
                    return;
                }

                // allow the user to choose variable(s) to import by popup
                string[] volumeVariableNames = ListMessageBox.ShowMultiple("Select variable to import", volumeVariables);
                if (volumeVariableNames != null && volumeVariableNames.Length > 0) {

                    // read the variable volumes from the file
                    Dictionary<string, List<Volume>> variables = Session.readSessionVolumeVariableFromFile(dlgLoadSessionFile.FileName, volumeVariableNames);
                    if (variables == null) {

                        // message and return
                        logger.Error("Volumes variables could not be read from file '" + dlgLoadSessionFile.FileName + "'");
                        return;
                    }

                    // import them into the session
                    foreach (KeyValuePair<string, List<Volume>> entry in variables) {

                        if (entry.Value.Count == 0) {
                            logger.Error("Error while reading variable '" + entry.Key + "' from file '" + dlgLoadSessionFile.FileName + "'");
                        } else if (entry.Value.Count == 0) {
                            logger.Warn("Could not find volumes for variable '" + entry.Key + "' in file '" + dlgLoadSessionFile.FileName + "'");
                        } else {
                            Session.setVolumes(entry.Key, entry.Value);
                        }

                    }

                }

            }

        }

        private void btnSessionTrialsVariableImport_Click(object sender, EventArgs e) {

            // open file dialog to open file
            OpenFileDialog dlgLoadSessionFile = new OpenFileDialog();

            // set initial directory
            //dlgLoadSessionFile.InitialDirectory = Directory.GetCurrentDirectory();
            dlgLoadSessionFile.Filter = "Session files (*.txt; *.dat)|*.txt;*.dat|All files (*.*)|*.*";
            dlgLoadSessionFile.RestoreDirectory = true;

            // check if ok has been clicked on the dialog
            if (dlgLoadSessionFile.ShowDialog() == DialogResult.OK) {

                // try to retrieve the variables
                string[] trialVariables = Session.listSessionVariableNamesFromFile(dlgLoadSessionFile.FileName, false, true);
                if (trialVariables == null || trialVariables.Length == 0) {

                    // message and return
                    logger.Error("No trial variables found in file '" + dlgLoadSessionFile.FileName + "'");
                    return;
                }

                // allow the user to choose variable(s) to import by popup
                string[] trialVariableNames = ListMessageBox.ShowMultiple("Select variable to import", trialVariables);
                if (trialVariableNames != null && trialVariableNames.Length > 0) {

                    // read the variable trials from the file
                    Dictionary<string, List<Block>> variables = Session.readSessionTrialsVariableFromFile(dlgLoadSessionFile.FileName, trialVariableNames);
                    if (variables == null) {

                        // message and return
                        logger.Error("Trial variables could not be read from file '" + dlgLoadSessionFile.FileName + "'");
                        return;
                    }

                    // import them into the session
                    foreach (KeyValuePair<string, List<Block>> entry in variables) {

                        if (entry.Value.Count == 0) {
                            logger.Error("Error while reading variable '" + entry.Key + "' from file '" + dlgLoadSessionFile.FileName + "'");
                        } else if (entry.Value.Count == 0) {
                            logger.Warn("Could not find trials for variable '" + entry.Key + "' in file '" + dlgLoadSessionFile.FileName + "'");
                        } else {
                            Session.setTaskTrials(entry.Key, entry.Value);
                        }

                    }

                }

            }

        }

        private int[] getViewPosWindowDragMode(MouseEventArgs e) {

            // determine the border drag width and height
            int dragBorderWidth = 20;
            int dragBorderHeight = 20;
            if (picViewPosWindow.Width < 60) dragBorderWidth = (int)(picViewPosWindow.Width / 3.0);
            if (picViewPosWindow.Height < 60) dragBorderHeight = (int)(picViewPosWindow.Height / 3.0);

            // check x and y
            int xState = 0;
            int yState = 0;
            if (e.X < dragBorderWidth) {
                xState = 1;     // left border
            } else if (e.X > picViewPosWindow.Width - dragBorderWidth) {
                xState = 2;     // right border
            }
            if (e.Y < dragBorderHeight) {
                yState = 1;     // top border
            } else if (e.Y > picViewPosWindow.Height - dragBorderHeight) {
                yState = 2;     // bottom border
            }

            // return the border drag states
            return new int[] { xState, yState };

        }

        private Point mouseDownLocation = new Point(0, 0);
        private double toPnlFactor = 0;
        private double toViewFactor = 0;
        private bool draggingViewWindow = false;
        private bool leftSizing = false;
        private bool rightSizing = false;
        private bool topSizing = false;
        private bool bottomSizing = false;

        private void picViewPosWindow_MouseDown(object sender, MouseEventArgs e) {

            // check if button pressed
            if (e.Button == MouseButtons.Left) {

                // get the border drag states
                int[] dragStates = getViewPosWindowDragMode(e);

                // store the click offset
                mouseDownLocation = e.Location;

                // set the drag flags accordingly
                draggingViewWindow = false;
                leftSizing = false;
                rightSizing = false;
                topSizing = false;
                bottomSizing = false;
                if (dragStates[0] == 0 && dragStates[1] == 0) {
                    picViewPosWindow.Cursor = Cursors.SizeAll;
                    draggingViewWindow = true;

                } else if (dragStates[0] == 0 && dragStates[1] == 1) {
                    picViewPosWindow.Cursor = Cursors.SizeNS;
                    topSizing = true;

                } else if (dragStates[0] == 0 && dragStates[1] == 2) {
                    picViewPosWindow.Cursor = Cursors.SizeNS;
                    bottomSizing = true;

                } else if (dragStates[0] == 1 && dragStates[1] == 0) {
                    picViewPosWindow.Cursor = Cursors.SizeWE;
                    leftSizing = true;

                } else if (dragStates[0] == 1 && dragStates[1] == 1) {
                    picViewPosWindow.Cursor = Cursors.SizeNWSE;
                    leftSizing = true;
                    topSizing = true;

                } else if (dragStates[0] == 1 && dragStates[1] == 2) {
                    picViewPosWindow.Cursor = Cursors.SizeNESW;
                    leftSizing = true;
                    bottomSizing = true;

                } else if (dragStates[0] == 2 && dragStates[1] == 0) {
                    picViewPosWindow.Cursor = Cursors.SizeWE;
                    rightSizing = true;

                } else if (dragStates[0] == 2 && dragStates[1] == 1) {
                    picViewPosWindow.Cursor = Cursors.SizeNESW;
                    rightSizing = true;
                    topSizing = true;

                } else if (dragStates[0] == 2 && dragStates[1] == 2) {
                    picViewPosWindow.Cursor = Cursors.SizeNWSE;
                    rightSizing = true;
                    bottomSizing = true;

                }

                // set the limitations
                if (draggingViewWindow) {

                    pnlViewLimitLeft = picViewPosScreen.Left;
                    pnlViewLimitRight = picViewPosScreen.Left + picViewPosScreen.Width - picViewPosWindow.Width;
                    pnlViewLimitTop = picViewPosScreen.Top;
                    pnlViewLimitBottom = picViewPosScreen.Top + picViewPosScreen.Height - picViewPosWindow.Height;

                } else {

                    if (leftSizing) {
                        pnlViewLimitLeft = picViewPosScreen.Left;
                        if (view.hasBorder()) {
                            pnlViewLimitRight = picViewPosWindow.Left + picViewPosWindow.Width - (int)Math.Round(200 * toPnlFactor);
                        } else {
                            pnlViewLimitRight = picViewPosWindow.Left + picViewPosWindow.Width - 10;
                        }
                    } else if (rightSizing) {
                        pnlViewWidth = picViewPosWindow.Width;
                        if (view.hasBorder()) {
                            pnlViewLimitLeft = (int)Math.Round(200 * toPnlFactor);
                        } else {
                            pnlViewLimitLeft = 10;
                        }
                        pnlViewLimitRight = picViewPosScreen.Left + picViewPosScreen.Width - picViewPosWindow.Left;
                    } else {
                        pnlViewLimitLeft = 0;
                        pnlViewLimitRight = 0;
                    }


                    if (topSizing) {
                        pnlViewLimitTop = picViewPosScreen.Top;
                        if (view.hasBorder()) {
                            pnlViewLimitBottom = picViewPosWindow.Top + picViewPosWindow.Height - (int)Math.Round(100 * toPnlFactor);
                        } else {
                            pnlViewLimitBottom = picViewPosWindow.Top + picViewPosWindow.Height - 10;
                        }
                    } else if (bottomSizing) {
                        pnlViewHeight = picViewPosWindow.Height;
                        if (view.hasBorder()) {
                            pnlViewLimitTop = (int)Math.Round(100 * toPnlFactor);
                        } else {
                            pnlViewLimitTop = 10;
                        }
                        pnlViewLimitBottom = picViewPosScreen.Top + picViewPosScreen.Height - picViewPosWindow.Top;
                    } else {
                        pnlViewLimitTop = 0;
                        pnlViewLimitBottom = 0;
                    }

                }


            }

        }

        private void picViewPosWindow_MouseMove(object sender, MouseEventArgs e) {


            // check if we are dragging
            if (draggingViewWindow || leftSizing || rightSizing || topSizing || bottomSizing) {
                // dragging

                int newX = 0;
                int newY = 0;
                
                // calculate the new x
                if (draggingViewWindow || leftSizing) {
                    newX = e.X + picViewPosWindow.Left - mouseDownLocation.X;
                } else if (rightSizing) {
                    newX = e.X + pnlViewWidth - mouseDownLocation.X;
                }
                if (draggingViewWindow || topSizing) {
                    newY = e.Y + picViewPosWindow.Top - mouseDownLocation.Y;
                } else if (bottomSizing) {
                    newY = e.Y + pnlViewHeight - mouseDownLocation.Y;
                }
                
                // limit the x and y
                if (newX < pnlViewLimitLeft)           newX = pnlViewLimitLeft;
                else if (newX > pnlViewLimitRight)     newX = pnlViewLimitRight;
                if (newY < pnlViewLimitTop)            newY = pnlViewLimitTop;
                else if (newY > pnlViewLimitBottom)    newY = pnlViewLimitBottom;
                

                if (draggingViewWindow) {

                    // set new x and y positions
                    picViewPosWindow.Left = newX;
                    picViewPosWindow.Top = newY;


                    // calculate position of the view

                    repViewWindow.X = (int)Math.Round((picViewPosWindow.Left - picViewPosScreen.Left) * toViewFactor);
                    repViewWindow.Y = (int)Math.Round((picViewPosWindow.Top - picViewPosScreen.Top) * toViewFactor);


                    // set the new position
                    view.setWindowLocation( repViewWindow.X,
                                            repViewWindow.Y);

                }
                if (leftSizing) {

                    // store the position of the right side for both the view on the panel and the actual view
                    int viewRight = repViewWindow.X + repViewWindow.Width;
                    int panelViewRight = picViewPosWindow.Left + picViewPosWindow.Width;

                    // set the new X of the view
                    repViewWindow.X = (int)Math.Round((newX - picViewPosScreen.Left) * toViewFactor);

                    // calculate the new width
                    repViewWindow.Width = viewRight - repViewWindow.X;

                    // suspend painting (while resizing and relocating)
                    SendMessage(pnlViewPos.Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);

                    // resize and relocate the view representation in the panel
                    picViewPosWindow.Left = newX;
                    picViewPosWindow.Width = panelViewRight - picViewPosWindow.Left;

                    // resume painting and refresh
                    SendMessage(pnlViewPos.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                    pnlViewPos.Refresh();
                    
                    // resize and relocate the view
                    view.setWindowLocationAndSize(  repViewWindow.X,
                                                    repViewWindow.Y,
                                                    repViewWindow.Width,
                                                    repViewWindow.Height);

                } else if (rightSizing) {

                    // set new width of the view representation in the panel
                    picViewPosWindow.Width = newX;

                    // calculate the new width of the view
                    repViewWindow.Width = (int)Math.Round(picViewPosWindow.Width * toViewFactor);

                    // resize the view
                    view.setWindowSize( repViewWindow.Width,
                                        repViewWindow.Height);

                }

                if (topSizing) {

                    // store the position of the bottom side for both the view on the panel and the actual view
                    int viewBottom = repViewWindow.Y + repViewWindow.Height;
                    int panelViewBottom = picViewPosWindow.Top + picViewPosWindow.Height;

                    // set the new Y of the view
                    repViewWindow.Y = (int)Math.Round((newY - picViewPosScreen.Top) * toViewFactor);

                    // calculate the new width
                    repViewWindow.Height = viewBottom - repViewWindow.Y;

                    // suspend painting (while resizing and relocating)
                    SendMessage(pnlViewPos.Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);

                    // resize and relocate the view representation in the panel
                    picViewPosWindow.Top = newY;
                    picViewPosWindow.Height = panelViewBottom - picViewPosWindow.Top;

                    // resume painting and refresh
                    SendMessage(pnlViewPos.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                    pnlViewPos.Refresh();

                    // resize and relocate the view
                    view.setWindowLocationAndSize(  repViewWindow.X,
                                                    repViewWindow.Y,
                                                    repViewWindow.Width,
                                                    repViewWindow.Height);

                } else if (bottomSizing) {

                    // set new width of the view representation in the panel
                    picViewPosWindow.Height = newY;

                    // calculate the new width of the view
                    repViewWindow.Height = (int)Math.Round(picViewPosWindow.Height * toViewFactor);

                    // resize the view
                    view.setWindowSize( repViewWindow.Width,
                                        repViewWindow.Height);

                }




            } else {
                // not dragging

                // get the border drag states
                int[] dragStates = getViewPosWindowDragMode(e);

                // set the cursor accordingly
                if (dragStates[0] == 0 && dragStates[1] == 0) {
                    picViewPosWindow.Cursor = Cursors.SizeAll;
                } else if ((dragStates[0] == 0 && dragStates[1] == 1) || (dragStates[0] == 0 && dragStates[1] == 2)) {
                    picViewPosWindow.Cursor = Cursors.SizeNS;
                } else if ((dragStates[0] == 1 && dragStates[1] == 0) || (dragStates[0] == 2 && dragStates[1] == 0)) {
                    picViewPosWindow.Cursor = Cursors.SizeWE;
                } else if ((dragStates[0] == 1 && dragStates[1] == 1) || (dragStates[0] == 2 && dragStates[1] == 2)) {
                    picViewPosWindow.Cursor = Cursors.SizeNWSE;
                } else if ((dragStates[0] == 1 && dragStates[1] == 2) || (dragStates[0] == 2 && dragStates[1] == 1)) {
                    picViewPosWindow.Cursor = Cursors.SizeNESW;
                }

            }


        }

        private void picViewPosWindow_MouseUp(object sender, MouseEventArgs e) {
            draggingViewWindow = false;
            leftSizing = false;
            rightSizing = false;
            topSizing = false;
            bottomSizing = false;
        }

        private void GUIMain_KeyDown(object sender, KeyEventArgs e) {
            if (e.Shift && e.KeyCode == Keys.PageDown) {
                this.Top += 10;
                e.SuppressKeyPress = true;

            } else if (e.Shift && e.KeyCode == Keys.PageUp) {
                this.Top -= 10;
                e.SuppressKeyPress = true;
            }

        }

        private void picViewPosWindow_DoubleClick(object sender, EventArgs e) {
            Screen screen = Screen.FromPoint(new Point(view.getWindowX(), view.getWindowY()));
            Rectangle screenRect = screen.WorkingArea;
            view.setWindowLocationAndSize(screenRect.Left, screenRect.Top, screenRect.Width, screenRect.Height);
        }

        private void txtConsole_ScrolledToBottom(object sender, EventArgs e) {
            if (!rtbTarget.AutoScroll)
                rtbTarget.AutoScroll = true;

        }

        private void txtConsole_ScrolledNotBottom(object sender, EventArgs e) {
            if (rtbTarget.AutoScroll)
                rtbTarget.AutoScroll = false;

        }

    }

    class NoBorderTabControl : TabControl {
        private const int TCM_ADJUSTRECT = 0x1328;

        protected override void WndProc(ref Message m) {

            //Hide the tab headers at run-time
            if (m.Msg == TCM_ADJUSTRECT) {

                RECT rect = (RECT)(m.GetLParam(typeof(RECT)));
                rect.Left = this.Left - this.Margin.Left;
                rect.Right = this.Right + this.Margin.Right;

                rect.Top = this.Top - this.Margin.Top;
                rect.Bottom = this.Bottom + this.Margin.Bottom;
                Marshal.StructureToPtr(rect, m.LParam, true);

            }
            
            // call the base class implementation
            base.WndProc(ref m);
        }

        private struct RECT {
            public int Left, Top, Right, Bottom;
        }
    }
    public class RTFScrolledBottom : RichTextBox {
        public event EventHandler ScrolledToBottom;
        public event EventHandler ScrolledNotBottom;

        private const int WM_VSCROLL = 0x115;
        private const int WM_MOUSEWHEEL = 0x20A;
        private const int WM_USER = 0x400;
        private const int SB_VERT = 1;
        private const int EM_SETSCROLLPOS = WM_USER + 222;
        private const int EM_GETSCROLLPOS = WM_USER + 221;

        [DllImport("user32.dll")]
        private static extern bool GetScrollRange(IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, ref Point lParam);

        public bool IsAtMaxScroll() {
            int minScroll;
            int maxScroll;
            GetScrollRange(this.Handle, SB_VERT, out minScroll, out maxScroll);
            Point rtfPoint = Point.Empty;
            SendMessage(this.Handle, EM_GETSCROLLPOS, 0, ref rtfPoint);

            return (rtfPoint.Y + this.ClientSize.Height >= maxScroll);
        }

        protected virtual void OnScrolledToBottom(EventArgs e) {
            if (ScrolledToBottom != null)
                ScrolledToBottom(this, e);
        }

        protected virtual void OnScrolledNotBottom(EventArgs e) {
            if (ScrolledNotBottom != null)
                ScrolledNotBottom(this, e);
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            if (IsAtMaxScroll())
                OnScrolledToBottom(EventArgs.Empty);
            else
                OnScrolledNotBottom(EventArgs.Empty);

            base.OnKeyUp(e);
        }

        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_VSCROLL || m.Msg == WM_MOUSEWHEEL) {
                if (IsAtMaxScroll())
                    OnScrolledToBottom(EventArgs.Empty);
                else
                    OnScrolledNotBottom(EventArgs.Empty);
            }

            base.WndProc(ref m);
        }

    }


    public class PictureBoxWithInterpolationMode : PictureBox {
        public InterpolationMode InterpolationMode { get; set; }

        protected override void OnPaint(PaintEventArgs paintEventArgs)
        {
            paintEventArgs.Graphics.InterpolationMode = InterpolationMode;
            base.OnPaint(paintEventArgs);
        }
    }



}
