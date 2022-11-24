using System;

namespace RETIF4.GUI {

    partial class GUIMain {

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            try {
                if (disposing && (components != null)) {
                    components.Dispose();
                }
                base.Dispose(disposing);
            } catch (Exception) { }
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.tmrViewUpdate = new System.Windows.Forms.Timer(this.components);
            this.tmrExperimentUpdate = new System.Windows.Forms.Timer(this.components);
            this.tmrMatlabUpdate = new System.Windows.Forms.Timer(this.components);
            this.tmrGUI = new System.Windows.Forms.Timer(this.components);
            this.tmrSessionUpdate = new System.Windows.Forms.Timer(this.components);
            this.tabControl = new RETIF4.GUI.NoBorderTabControl();
            this.tabExperiment = new System.Windows.Forms.TabPage();
            this.grpConsole = new System.Windows.Forms.GroupBox();
            this.txtConsole = new RETIF4.GUI.RTFScrolledBottom();
            this.grpExp = new System.Windows.Forms.GroupBox();
            this.lblPhaseStarttype = new System.Windows.Forms.Label();
            this.cmbPhaseStarttype = new System.Windows.Forms.ComboBox();
            this.chkPhaseAutoContinue = new System.Windows.Forms.CheckBox();
            this.lblCurrentStatus = new System.Windows.Forms.Label();
            this.lblCurrentPhase = new System.Windows.Forms.Label();
            this.lblCurrentStatusLabel = new System.Windows.Forms.Label();
            this.lblCurrentPhaseLabel = new System.Windows.Forms.Label();
            this.btnPhaseStart = new System.Windows.Forms.Button();
            this.lblPhase = new System.Windows.Forms.Label();
            this.cmbPhase = new System.Windows.Forms.ComboBox();
            this.tabView = new System.Windows.Forms.TabPage();
            this.grpViewInfo = new System.Windows.Forms.GroupBox();
            this.txtViewInfo = new System.Windows.Forms.TextBox();
            this.grpViewPosSize = new System.Windows.Forms.GroupBox();
            this.lblViewWindowBorder = new System.Windows.Forms.Label();
            this.chkViewWindowBorder = new System.Windows.Forms.CheckBox();
            this.pnlViewPos = new System.Windows.Forms.Panel();
            this.picViewPosWindow = new System.Windows.Forms.PictureBox();
            this.picViewPosScreen = new System.Windows.Forms.PictureBox();
            this.btnViewSetWindow = new System.Windows.Forms.Button();
            this.lblViewContent = new System.Windows.Forms.Label();
            this.txtViewContentHeight = new System.Windows.Forms.TextBox();
            this.lblViewContentHeight = new System.Windows.Forms.Label();
            this.txtViewContentWidth = new System.Windows.Forms.TextBox();
            this.lblViewContentWidth = new System.Windows.Forms.Label();
            this.lblViewWindow = new System.Windows.Forms.Label();
            this.btnViewSetContent = new System.Windows.Forms.Button();
            this.txtViewWindowHeight = new System.Windows.Forms.TextBox();
            this.lblViewWindowHeight = new System.Windows.Forms.Label();
            this.txtViewWindowWidth = new System.Windows.Forms.TextBox();
            this.lblViewWindowWidth = new System.Windows.Forms.Label();
            this.txtViewWindowY = new System.Windows.Forms.TextBox();
            this.lblViewWindowY = new System.Windows.Forms.Label();
            this.txtViewWindowX = new System.Windows.Forms.TextBox();
            this.lblViewWindowX = new System.Windows.Forms.Label();
            this.btnViewStart = new System.Windows.Forms.Button();
            this.tabSession = new System.Windows.Forms.TabPage();
            this.tabSessionVariables = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.grpSessionVolumeInfo = new System.Windows.Forms.GroupBox();
            this.btnSessionVolumeInfoFilenameBrowse = new System.Windows.Forms.Button();
            this.lblSessionVolumeInfoCondition = new System.Windows.Forms.Label();
            this.lblSessionVolumeInfoConditionLabel = new System.Windows.Forms.Label();
            this.lblSessionVolumeInfoIndex = new System.Windows.Forms.Label();
            this.lblSessionVolumeInfoIndexLabel = new System.Windows.Forms.Label();
            this.lblSessionVolumeInfoDateTime = new System.Windows.Forms.Label();
            this.lblSessionVolumeInfoDateTimeLabel = new System.Windows.Forms.Label();
            this.lblSessionVolumeInfoID = new System.Windows.Forms.Label();
            this.lblSessionVolumeInfoFilename = new System.Windows.Forms.Label();
            this.lblSessionVolumeInfoIDLabel = new System.Windows.Forms.Label();
            this.lblSessionVolumeInfoFilenameLabel = new System.Windows.Forms.Label();
            this.lstSessionVolumes = new System.Windows.Forms.ListBox();
            this.btnSessionVolumeVariableImport = new System.Windows.Forms.Button();
            this.lstSessionVolumeVariables = new System.Windows.Forms.ListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lstSessionTrialVariables = new System.Windows.Forms.ListBox();
            this.btnSessionTrialsVariableImport = new System.Windows.Forms.Button();
            this.lblSessionTimedata = new System.Windows.Forms.Label();
            this.picSessionTimedata = new System.Windows.Forms.PictureBox();
            this.tabData = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.tabMatlab = new System.Windows.Forms.TabPage();
            this.button2 = new System.Windows.Forms.Button();
            this.grpMatlab = new System.Windows.Forms.GroupBox();
            this.lblMatlabRunning = new System.Windows.Forms.Label();
            this.lblMatlabRunningLabel = new System.Windows.Forms.Label();
            this.btnMatlabStartStop = new System.Windows.Forms.Button();
            this.lblMatlabVersion = new System.Windows.Forms.Label();
            this.lblMatlabVersionLabel = new System.Windows.Forms.Label();
            this.lblMatlabState = new System.Windows.Forms.Label();
            this.lblMatlabStateLabel = new System.Windows.Forms.Label();
            this.grpMatlabConsole = new System.Windows.Forms.GroupBox();
            this.txtMatlabConsole = new System.Windows.Forms.RichTextBox();
            this.tabControl.SuspendLayout();
            this.tabExperiment.SuspendLayout();
            this.grpConsole.SuspendLayout();
            this.grpExp.SuspendLayout();
            this.tabView.SuspendLayout();
            this.grpViewInfo.SuspendLayout();
            this.grpViewPosSize.SuspendLayout();
            this.pnlViewPos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picViewPosWindow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picViewPosScreen)).BeginInit();
            this.tabSession.SuspendLayout();
            this.tabSessionVariables.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.grpSessionVolumeInfo.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSessionTimedata)).BeginInit();
            this.tabData.SuspendLayout();
            this.tabMatlab.SuspendLayout();
            this.grpMatlab.SuspendLayout();
            this.grpMatlabConsole.SuspendLayout();
            this.SuspendLayout();
            // 
            // tmrViewUpdate
            // 
            this.tmrViewUpdate.Interval = 200;
            this.tmrViewUpdate.Tick += new System.EventHandler(this.tmrViewUpdate_Tick);
            // 
            // tmrExperimentUpdate
            // 
            this.tmrExperimentUpdate.Interval = 200;
            this.tmrExperimentUpdate.Tick += new System.EventHandler(this.tmrExperimentUpdate_Tick);
            // 
            // tmrMatlabUpdate
            // 
            this.tmrMatlabUpdate.Interval = 200;
            this.tmrMatlabUpdate.Tick += new System.EventHandler(this.tmrMatlabUpdate_Tick);
            // 
            // tmrGUI
            // 
            this.tmrGUI.Interval = 200;
            this.tmrGUI.Tick += new System.EventHandler(this.tmrGUI_Tick);
            // 
            // tmrSessionUpdate
            // 
            this.tmrSessionUpdate.Interval = 200;
            this.tmrSessionUpdate.Tick += new System.EventHandler(this.tmrSessionUpdate_Tick);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabExperiment);
            this.tabControl.Controls.Add(this.tabView);
            this.tabControl.Controls.Add(this.tabSession);
            this.tabControl.Controls.Add(this.tabData);
            this.tabControl.Controls.Add(this.tabMatlab);
            this.tabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.tabControl.Location = new System.Drawing.Point(-1, 0);
            this.tabControl.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1181, 816);
            this.tabControl.TabIndex = 0;
            this.tabControl.TabStop = false;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // tabExperiment
            // 
            this.tabExperiment.BackColor = System.Drawing.SystemColors.Control;
            this.tabExperiment.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabExperiment.Controls.Add(this.grpConsole);
            this.tabExperiment.Controls.Add(this.grpExp);
            this.tabExperiment.Location = new System.Drawing.Point(0, 23);
            this.tabExperiment.Margin = new System.Windows.Forms.Padding(4);
            this.tabExperiment.Name = "tabExperiment";
            this.tabExperiment.Padding = new System.Windows.Forms.Padding(4);
            this.tabExperiment.Size = new System.Drawing.Size(1181, 793);
            this.tabExperiment.TabIndex = 0;
            this.tabExperiment.Text = "Experiment";
            // 
            // grpConsole
            // 
            this.grpConsole.Controls.Add(this.txtConsole);
            this.grpConsole.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.grpConsole.Location = new System.Drawing.Point(15, 208);
            this.grpConsole.Margin = new System.Windows.Forms.Padding(4);
            this.grpConsole.Name = "grpConsole";
            this.grpConsole.Padding = new System.Windows.Forms.Padding(4);
            this.grpConsole.Size = new System.Drawing.Size(1147, 564);
            this.grpConsole.TabIndex = 9;
            this.grpConsole.TabStop = false;
            this.grpConsole.Text = "Output";
            // 
            // txtConsole
            // 
            this.txtConsole.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.txtConsole.Location = new System.Drawing.Point(20, 32);
            this.txtConsole.Margin = new System.Windows.Forms.Padding(4);
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.Size = new System.Drawing.Size(1105, 513);
            this.txtConsole.TabIndex = 0;
            this.txtConsole.Text = "";
            // 
            // grpExp
            // 
            this.grpExp.Controls.Add(this.lblPhaseStarttype);
            this.grpExp.Controls.Add(this.cmbPhaseStarttype);
            this.grpExp.Controls.Add(this.chkPhaseAutoContinue);
            this.grpExp.Controls.Add(this.lblCurrentStatus);
            this.grpExp.Controls.Add(this.lblCurrentPhase);
            this.grpExp.Controls.Add(this.lblCurrentStatusLabel);
            this.grpExp.Controls.Add(this.lblCurrentPhaseLabel);
            this.grpExp.Controls.Add(this.btnPhaseStart);
            this.grpExp.Controls.Add(this.lblPhase);
            this.grpExp.Controls.Add(this.cmbPhase);
            this.grpExp.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.grpExp.Location = new System.Drawing.Point(15, 5);
            this.grpExp.Margin = new System.Windows.Forms.Padding(4);
            this.grpExp.Name = "grpExp";
            this.grpExp.Padding = new System.Windows.Forms.Padding(4);
            this.grpExp.Size = new System.Drawing.Size(1147, 195);
            this.grpExp.TabIndex = 8;
            this.grpExp.TabStop = false;
            // 
            // lblPhaseStarttype
            // 
            this.lblPhaseStarttype.AutoSize = true;
            this.lblPhaseStarttype.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblPhaseStarttype.Location = new System.Drawing.Point(288, 139);
            this.lblPhaseStarttype.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPhaseStarttype.Name = "lblPhaseStarttype";
            this.lblPhaseStarttype.Size = new System.Drawing.Size(23, 16);
            this.lblPhaseStarttype.TabIndex = 17;
            this.lblPhaseStarttype.Text = "-->";
            // 
            // cmbPhaseStarttype
            // 
            this.cmbPhaseStarttype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPhaseStarttype.Enabled = false;
            this.cmbPhaseStarttype.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.cmbPhaseStarttype.FormattingEnabled = true;
            this.cmbPhaseStarttype.Items.AddRange(new object[] {
            "IMMEDIATE",
            "AT INCOMING_WITH_SCENE_NOW,",
            "AT_INCOMING_WITH_SCENE_AT_START,",
            "AFTER_CURRENT_WITH_SCENE_NOW,",
            "AFTER_CURRENT_WITH_SCENE_AT_START"});
            this.cmbPhaseStarttype.Location = new System.Drawing.Point(325, 136);
            this.cmbPhaseStarttype.Margin = new System.Windows.Forms.Padding(4);
            this.cmbPhaseStarttype.Name = "cmbPhaseStarttype";
            this.cmbPhaseStarttype.Size = new System.Drawing.Size(786, 24);
            this.cmbPhaseStarttype.TabIndex = 16;
            // 
            // chkPhaseAutoContinue
            // 
            this.chkPhaseAutoContinue.AutoSize = true;
            this.chkPhaseAutoContinue.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkPhaseAutoContinue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.chkPhaseAutoContinue.Location = new System.Drawing.Point(869, 58);
            this.chkPhaseAutoContinue.Margin = new System.Windows.Forms.Padding(4);
            this.chkPhaseAutoContinue.Name = "chkPhaseAutoContinue";
            this.chkPhaseAutoContinue.Size = new System.Drawing.Size(242, 24);
            this.chkPhaseAutoContinue.TabIndex = 9;
            this.chkPhaseAutoContinue.Tag = "Don\'t continue automatically to the next phase";
            this.chkPhaseAutoContinue.Text = "Don\'t continue automatically";
            this.chkPhaseAutoContinue.UseVisualStyleBackColor = true;
            // 
            // lblCurrentStatus
            // 
            this.lblCurrentStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblCurrentStatus.Location = new System.Drawing.Point(169, 61);
            this.lblCurrentStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCurrentStatus.Name = "lblCurrentStatus";
            this.lblCurrentStatus.Size = new System.Drawing.Size(649, 22);
            this.lblCurrentStatus.TabIndex = 15;
            this.lblCurrentStatus.Text = "stopped";
            // 
            // lblCurrentPhase
            // 
            this.lblCurrentPhase.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblCurrentPhase.Location = new System.Drawing.Point(169, 92);
            this.lblCurrentPhase.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCurrentPhase.Name = "lblCurrentPhase";
            this.lblCurrentPhase.Size = new System.Drawing.Size(944, 22);
            this.lblCurrentPhase.TabIndex = 14;
            this.lblCurrentPhase.Text = "-";
            // 
            // lblCurrentStatusLabel
            // 
            this.lblCurrentStatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblCurrentStatusLabel.Location = new System.Drawing.Point(15, 61);
            this.lblCurrentStatusLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCurrentStatusLabel.Name = "lblCurrentStatusLabel";
            this.lblCurrentStatusLabel.Size = new System.Drawing.Size(145, 20);
            this.lblCurrentStatusLabel.TabIndex = 12;
            this.lblCurrentStatusLabel.Text = "Current status:";
            this.lblCurrentStatusLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblCurrentPhaseLabel
            // 
            this.lblCurrentPhaseLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblCurrentPhaseLabel.Location = new System.Drawing.Point(19, 92);
            this.lblCurrentPhaseLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCurrentPhaseLabel.Name = "lblCurrentPhaseLabel";
            this.lblCurrentPhaseLabel.Size = new System.Drawing.Size(141, 20);
            this.lblCurrentPhaseLabel.TabIndex = 11;
            this.lblCurrentPhaseLabel.Text = "Current phase:";
            this.lblCurrentPhaseLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btnPhaseStart
            // 
            this.btnPhaseStart.Enabled = false;
            this.btnPhaseStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.btnPhaseStart.Location = new System.Drawing.Point(25, 132);
            this.btnPhaseStart.Margin = new System.Windows.Forms.Padding(4);
            this.btnPhaseStart.Name = "btnPhaseStart";
            this.btnPhaseStart.Size = new System.Drawing.Size(255, 32);
            this.btnPhaseStart.TabIndex = 10;
            this.btnPhaseStart.Text = "Start";
            this.btnPhaseStart.UseVisualStyleBackColor = true;
            this.btnPhaseStart.Click += new System.EventHandler(this.btnStartPhase_Click);
            // 
            // lblPhase
            // 
            this.lblPhase.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblPhase.Location = new System.Drawing.Point(19, 29);
            this.lblPhase.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPhase.Name = "lblPhase";
            this.lblPhase.Size = new System.Drawing.Size(141, 20);
            this.lblPhase.TabIndex = 9;
            this.lblPhase.Text = "Phases:";
            this.lblPhase.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmbPhase
            // 
            this.cmbPhase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPhase.Enabled = false;
            this.cmbPhase.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.cmbPhase.FormattingEnabled = true;
            this.cmbPhase.Location = new System.Drawing.Point(169, 27);
            this.cmbPhase.Margin = new System.Windows.Forms.Padding(4);
            this.cmbPhase.Name = "cmbPhase";
            this.cmbPhase.Size = new System.Drawing.Size(943, 25);
            this.cmbPhase.TabIndex = 8;
            this.cmbPhase.SelectedIndexChanged += new System.EventHandler(this.cmbPhase_SelectedIndexChanged);
            // 
            // tabView
            // 
            this.tabView.BackColor = System.Drawing.SystemColors.Control;
            this.tabView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabView.Controls.Add(this.grpViewInfo);
            this.tabView.Controls.Add(this.grpViewPosSize);
            this.tabView.Controls.Add(this.btnViewStart);
            this.tabView.Location = new System.Drawing.Point(0, 23);
            this.tabView.Margin = new System.Windows.Forms.Padding(4);
            this.tabView.Name = "tabView";
            this.tabView.Padding = new System.Windows.Forms.Padding(4);
            this.tabView.Size = new System.Drawing.Size(1181, 793);
            this.tabView.TabIndex = 1;
            this.tabView.Text = "View";
            // 
            // grpViewInfo
            // 
            this.grpViewInfo.Controls.Add(this.txtViewInfo);
            this.grpViewInfo.Enabled = false;
            this.grpViewInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.grpViewInfo.Location = new System.Drawing.Point(15, 521);
            this.grpViewInfo.Margin = new System.Windows.Forms.Padding(4);
            this.grpViewInfo.Name = "grpViewInfo";
            this.grpViewInfo.Padding = new System.Windows.Forms.Padding(4);
            this.grpViewInfo.Size = new System.Drawing.Size(1145, 249);
            this.grpViewInfo.TabIndex = 2;
            this.grpViewInfo.TabStop = false;
            this.grpViewInfo.Text = "View information";
            // 
            // txtViewInfo
            // 
            this.txtViewInfo.BackColor = System.Drawing.SystemColors.Control;
            this.txtViewInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtViewInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.txtViewInfo.Location = new System.Drawing.Point(36, 36);
            this.txtViewInfo.Margin = new System.Windows.Forms.Padding(4);
            this.txtViewInfo.Multiline = true;
            this.txtViewInfo.Name = "txtViewInfo";
            this.txtViewInfo.ReadOnly = true;
            this.txtViewInfo.Size = new System.Drawing.Size(1068, 193);
            this.txtViewInfo.TabIndex = 0;
            // 
            // grpViewPosSize
            // 
            this.grpViewPosSize.Controls.Add(this.lblViewWindowBorder);
            this.grpViewPosSize.Controls.Add(this.chkViewWindowBorder);
            this.grpViewPosSize.Controls.Add(this.pnlViewPos);
            this.grpViewPosSize.Controls.Add(this.btnViewSetWindow);
            this.grpViewPosSize.Controls.Add(this.lblViewContent);
            this.grpViewPosSize.Controls.Add(this.txtViewContentHeight);
            this.grpViewPosSize.Controls.Add(this.lblViewContentHeight);
            this.grpViewPosSize.Controls.Add(this.txtViewContentWidth);
            this.grpViewPosSize.Controls.Add(this.lblViewContentWidth);
            this.grpViewPosSize.Controls.Add(this.lblViewWindow);
            this.grpViewPosSize.Controls.Add(this.btnViewSetContent);
            this.grpViewPosSize.Controls.Add(this.txtViewWindowHeight);
            this.grpViewPosSize.Controls.Add(this.lblViewWindowHeight);
            this.grpViewPosSize.Controls.Add(this.txtViewWindowWidth);
            this.grpViewPosSize.Controls.Add(this.lblViewWindowWidth);
            this.grpViewPosSize.Controls.Add(this.txtViewWindowY);
            this.grpViewPosSize.Controls.Add(this.lblViewWindowY);
            this.grpViewPosSize.Controls.Add(this.txtViewWindowX);
            this.grpViewPosSize.Controls.Add(this.lblViewWindowX);
            this.grpViewPosSize.Enabled = false;
            this.grpViewPosSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.grpViewPosSize.Location = new System.Drawing.Point(15, 66);
            this.grpViewPosSize.Margin = new System.Windows.Forms.Padding(4);
            this.grpViewPosSize.Name = "grpViewPosSize";
            this.grpViewPosSize.Padding = new System.Windows.Forms.Padding(4);
            this.grpViewPosSize.Size = new System.Drawing.Size(1147, 441);
            this.grpViewPosSize.TabIndex = 1;
            this.grpViewPosSize.TabStop = false;
            this.grpViewPosSize.Text = "Position and size";
            // 
            // lblViewWindowBorder
            // 
            this.lblViewWindowBorder.AutoSize = true;
            this.lblViewWindowBorder.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblViewWindowBorder.Location = new System.Drawing.Point(38, 196);
            this.lblViewWindowBorder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblViewWindowBorder.Name = "lblViewWindowBorder";
            this.lblViewWindowBorder.Size = new System.Drawing.Size(53, 18);
            this.lblViewWindowBorder.TabIndex = 19;
            this.lblViewWindowBorder.Text = "Border";
            // 
            // chkViewWindowBorder
            // 
            this.chkViewWindowBorder.AutoSize = true;
            this.chkViewWindowBorder.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.chkViewWindowBorder.Location = new System.Drawing.Point(99, 198);
            this.chkViewWindowBorder.Margin = new System.Windows.Forms.Padding(4);
            this.chkViewWindowBorder.Name = "chkViewWindowBorder";
            this.chkViewWindowBorder.Size = new System.Drawing.Size(18, 17);
            this.chkViewWindowBorder.TabIndex = 17;
            this.chkViewWindowBorder.UseVisualStyleBackColor = true;
            this.chkViewWindowBorder.CheckedChanged += new System.EventHandler(this.chkViewWindowBorder_CheckedChanged);
            // 
            // pnlViewPos
            // 
            this.pnlViewPos.BackColor = System.Drawing.Color.Gray;
            this.pnlViewPos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlViewPos.Controls.Add(this.picViewPosWindow);
            this.pnlViewPos.Controls.Add(this.picViewPosScreen);
            this.pnlViewPos.Location = new System.Drawing.Point(291, 38);
            this.pnlViewPos.Margin = new System.Windows.Forms.Padding(4);
            this.pnlViewPos.Name = "pnlViewPos";
            this.pnlViewPos.Size = new System.Drawing.Size(825, 382);
            this.pnlViewPos.TabIndex = 16;
            // 
            // picViewPosWindow
            // 
            this.picViewPosWindow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picViewPosWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.picViewPosWindow.Location = new System.Drawing.Point(239, 93);
            this.picViewPosWindow.Name = "picViewPosWindow";
            this.picViewPosWindow.Size = new System.Drawing.Size(133, 61);
            this.picViewPosWindow.TabIndex = 6;
            this.picViewPosWindow.TabStop = false;
            this.picViewPosWindow.Visible = false;
            this.picViewPosWindow.DoubleClick += new System.EventHandler(this.picViewPosWindow_DoubleClick);
            this.picViewPosWindow.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picViewPosWindow_MouseDown);
            this.picViewPosWindow.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picViewPosWindow_MouseMove);
            this.picViewPosWindow.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picViewPosWindow_MouseUp);
            // 
            // picViewPosScreen
            // 
            this.picViewPosScreen.BackColor = System.Drawing.Color.Silver;
            this.picViewPosScreen.Location = new System.Drawing.Point(181, 61);
            this.picViewPosScreen.Name = "picViewPosScreen";
            this.picViewPosScreen.Size = new System.Drawing.Size(476, 296);
            this.picViewPosScreen.TabIndex = 1;
            this.picViewPosScreen.TabStop = false;
            // 
            // btnViewSetWindow
            // 
            this.btnViewSetWindow.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.btnViewSetWindow.Location = new System.Drawing.Point(99, 227);
            this.btnViewSetWindow.Margin = new System.Windows.Forms.Padding(4);
            this.btnViewSetWindow.Name = "btnViewSetWindow";
            this.btnViewSetWindow.Size = new System.Drawing.Size(133, 29);
            this.btnViewSetWindow.TabIndex = 15;
            this.btnViewSetWindow.Text = "Set";
            this.btnViewSetWindow.UseVisualStyleBackColor = true;
            this.btnViewSetWindow.Click += new System.EventHandler(this.btnViewSetWindow_Click);
            // 
            // lblViewContent
            // 
            this.lblViewContent.AutoSize = true;
            this.lblViewContent.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblViewContent.Location = new System.Drawing.Point(128, 286);
            this.lblViewContent.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblViewContent.Name = "lblViewContent";
            this.lblViewContent.Size = new System.Drawing.Size(60, 18);
            this.lblViewContent.TabIndex = 14;
            this.lblViewContent.Text = "Content";
            this.lblViewContent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtViewContentHeight
            // 
            this.txtViewContentHeight.BackColor = System.Drawing.Color.White;
            this.txtViewContentHeight.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.txtViewContentHeight.Location = new System.Drawing.Point(99, 345);
            this.txtViewContentHeight.Margin = new System.Windows.Forms.Padding(4);
            this.txtViewContentHeight.MaxLength = 8;
            this.txtViewContentHeight.Name = "txtViewContentHeight";
            this.txtViewContentHeight.Size = new System.Drawing.Size(132, 23);
            this.txtViewContentHeight.TabIndex = 13;
            this.txtViewContentHeight.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtViewContentHeight_KeyDown);
            // 
            // lblViewContentHeight
            // 
            this.lblViewContentHeight.AutoSize = true;
            this.lblViewContentHeight.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblViewContentHeight.Location = new System.Drawing.Point(41, 348);
            this.lblViewContentHeight.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblViewContentHeight.Name = "lblViewContentHeight";
            this.lblViewContentHeight.Size = new System.Drawing.Size(50, 18);
            this.lblViewContentHeight.TabIndex = 12;
            this.lblViewContentHeight.Text = "Height";
            // 
            // txtViewContentWidth
            // 
            this.txtViewContentWidth.BackColor = System.Drawing.Color.White;
            this.txtViewContentWidth.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.txtViewContentWidth.Location = new System.Drawing.Point(99, 313);
            this.txtViewContentWidth.Margin = new System.Windows.Forms.Padding(4);
            this.txtViewContentWidth.MaxLength = 8;
            this.txtViewContentWidth.Name = "txtViewContentWidth";
            this.txtViewContentWidth.Size = new System.Drawing.Size(132, 23);
            this.txtViewContentWidth.TabIndex = 11;
            this.txtViewContentWidth.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtViewContentWidth_KeyDown);
            // 
            // lblViewContentWidth
            // 
            this.lblViewContentWidth.AutoSize = true;
            this.lblViewContentWidth.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblViewContentWidth.Location = new System.Drawing.Point(45, 316);
            this.lblViewContentWidth.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblViewContentWidth.Name = "lblViewContentWidth";
            this.lblViewContentWidth.Size = new System.Drawing.Size(46, 18);
            this.lblViewContentWidth.TabIndex = 10;
            this.lblViewContentWidth.Text = "Width";
            // 
            // lblViewWindow
            // 
            this.lblViewWindow.AutoSize = true;
            this.lblViewWindow.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblViewWindow.Location = new System.Drawing.Point(131, 37);
            this.lblViewWindow.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblViewWindow.Name = "lblViewWindow";
            this.lblViewWindow.Size = new System.Drawing.Size(62, 18);
            this.lblViewWindow.TabIndex = 9;
            this.lblViewWindow.Text = "Window";
            this.lblViewWindow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnViewSetContent
            // 
            this.btnViewSetContent.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.btnViewSetContent.Location = new System.Drawing.Point(99, 381);
            this.btnViewSetContent.Margin = new System.Windows.Forms.Padding(4);
            this.btnViewSetContent.Name = "btnViewSetContent";
            this.btnViewSetContent.Size = new System.Drawing.Size(133, 29);
            this.btnViewSetContent.TabIndex = 8;
            this.btnViewSetContent.Text = "Set";
            this.btnViewSetContent.UseVisualStyleBackColor = true;
            this.btnViewSetContent.Click += new System.EventHandler(this.btnViewSetContent_Click);
            // 
            // txtViewWindowHeight
            // 
            this.txtViewWindowHeight.BackColor = System.Drawing.SystemColors.Control;
            this.txtViewWindowHeight.Enabled = false;
            this.txtViewWindowHeight.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.txtViewWindowHeight.Location = new System.Drawing.Point(99, 161);
            this.txtViewWindowHeight.Margin = new System.Windows.Forms.Padding(4);
            this.txtViewWindowHeight.MaxLength = 8;
            this.txtViewWindowHeight.Name = "txtViewWindowHeight";
            this.txtViewWindowHeight.Size = new System.Drawing.Size(132, 23);
            this.txtViewWindowHeight.TabIndex = 7;
            this.txtViewWindowHeight.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtViewWindowHeight_KeyDown);
            // 
            // lblViewWindowHeight
            // 
            this.lblViewWindowHeight.AutoSize = true;
            this.lblViewWindowHeight.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblViewWindowHeight.Location = new System.Drawing.Point(41, 164);
            this.lblViewWindowHeight.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblViewWindowHeight.Name = "lblViewWindowHeight";
            this.lblViewWindowHeight.Size = new System.Drawing.Size(50, 18);
            this.lblViewWindowHeight.TabIndex = 6;
            this.lblViewWindowHeight.Text = "Height";
            // 
            // txtViewWindowWidth
            // 
            this.txtViewWindowWidth.BackColor = System.Drawing.SystemColors.Control;
            this.txtViewWindowWidth.Enabled = false;
            this.txtViewWindowWidth.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.txtViewWindowWidth.Location = new System.Drawing.Point(99, 129);
            this.txtViewWindowWidth.Margin = new System.Windows.Forms.Padding(4);
            this.txtViewWindowWidth.MaxLength = 8;
            this.txtViewWindowWidth.Name = "txtViewWindowWidth";
            this.txtViewWindowWidth.Size = new System.Drawing.Size(132, 23);
            this.txtViewWindowWidth.TabIndex = 5;
            this.txtViewWindowWidth.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtViewWindowWidth_KeyDown);
            // 
            // lblViewWindowWidth
            // 
            this.lblViewWindowWidth.AutoSize = true;
            this.lblViewWindowWidth.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblViewWindowWidth.Location = new System.Drawing.Point(45, 132);
            this.lblViewWindowWidth.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblViewWindowWidth.Name = "lblViewWindowWidth";
            this.lblViewWindowWidth.Size = new System.Drawing.Size(46, 18);
            this.lblViewWindowWidth.TabIndex = 4;
            this.lblViewWindowWidth.Text = "Width";
            // 
            // txtViewWindowY
            // 
            this.txtViewWindowY.BackColor = System.Drawing.Color.White;
            this.txtViewWindowY.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.txtViewWindowY.Location = new System.Drawing.Point(99, 97);
            this.txtViewWindowY.Margin = new System.Windows.Forms.Padding(4);
            this.txtViewWindowY.MaxLength = 8;
            this.txtViewWindowY.Name = "txtViewWindowY";
            this.txtViewWindowY.Size = new System.Drawing.Size(132, 23);
            this.txtViewWindowY.TabIndex = 3;
            this.txtViewWindowY.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtViewWindowY_KeyDown);
            // 
            // lblViewWindowY
            // 
            this.lblViewWindowY.AutoSize = true;
            this.lblViewWindowY.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblViewWindowY.Location = new System.Drawing.Point(74, 100);
            this.lblViewWindowY.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblViewWindowY.Name = "lblViewWindowY";
            this.lblViewWindowY.Size = new System.Drawing.Size(17, 18);
            this.lblViewWindowY.TabIndex = 2;
            this.lblViewWindowY.Text = "Y";
            // 
            // txtViewWindowX
            // 
            this.txtViewWindowX.BackColor = System.Drawing.Color.White;
            this.txtViewWindowX.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.txtViewWindowX.Location = new System.Drawing.Point(99, 65);
            this.txtViewWindowX.Margin = new System.Windows.Forms.Padding(4);
            this.txtViewWindowX.MaxLength = 8;
            this.txtViewWindowX.Name = "txtViewWindowX";
            this.txtViewWindowX.Size = new System.Drawing.Size(132, 23);
            this.txtViewWindowX.TabIndex = 1;
            this.txtViewWindowX.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtViewWindowX_KeyDown);
            // 
            // lblViewWindowX
            // 
            this.lblViewWindowX.AutoSize = true;
            this.lblViewWindowX.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblViewWindowX.Location = new System.Drawing.Point(73, 68);
            this.lblViewWindowX.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblViewWindowX.Name = "lblViewWindowX";
            this.lblViewWindowX.Size = new System.Drawing.Size(18, 18);
            this.lblViewWindowX.TabIndex = 0;
            this.lblViewWindowX.Text = "X";
            // 
            // btnViewStart
            // 
            this.btnViewStart.Enabled = false;
            this.btnViewStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.btnViewStart.Location = new System.Drawing.Point(15, 14);
            this.btnViewStart.Margin = new System.Windows.Forms.Padding(4);
            this.btnViewStart.Name = "btnViewStart";
            this.btnViewStart.Size = new System.Drawing.Size(1147, 42);
            this.btnViewStart.TabIndex = 0;
            this.btnViewStart.Text = "Start";
            this.btnViewStart.UseVisualStyleBackColor = true;
            this.btnViewStart.Click += new System.EventHandler(this.btnViewStart_Click);
            // 
            // tabSession
            // 
            this.tabSession.BackColor = System.Drawing.SystemColors.Control;
            this.tabSession.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabSession.Controls.Add(this.tabSessionVariables);
            this.tabSession.Controls.Add(this.lblSessionTimedata);
            this.tabSession.Controls.Add(this.picSessionTimedata);
            this.tabSession.Location = new System.Drawing.Point(0, 23);
            this.tabSession.Margin = new System.Windows.Forms.Padding(4);
            this.tabSession.Name = "tabSession";
            this.tabSession.Padding = new System.Windows.Forms.Padding(4);
            this.tabSession.Size = new System.Drawing.Size(1181, 793);
            this.tabSession.TabIndex = 4;
            this.tabSession.Text = "Session";
            // 
            // tabSessionVariables
            // 
            this.tabSessionVariables.Controls.Add(this.tabPage1);
            this.tabSessionVariables.Controls.Add(this.tabPage2);
            this.tabSessionVariables.Location = new System.Drawing.Point(10, 20);
            this.tabSessionVariables.Name = "tabSessionVariables";
            this.tabSessionVariables.SelectedIndex = 0;
            this.tabSessionVariables.Size = new System.Drawing.Size(1160, 426);
            this.tabSessionVariables.TabIndex = 21;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.grpSessionVolumeInfo);
            this.tabPage1.Controls.Add(this.lstSessionVolumes);
            this.tabPage1.Controls.Add(this.btnSessionVolumeVariableImport);
            this.tabPage1.Controls.Add(this.lstSessionVolumeVariables);
            this.tabPage1.Location = new System.Drawing.Point(4, 27);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1152, 395);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Volumes";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // grpSessionVolumeInfo
            // 
            this.grpSessionVolumeInfo.Controls.Add(this.btnSessionVolumeInfoFilenameBrowse);
            this.grpSessionVolumeInfo.Controls.Add(this.lblSessionVolumeInfoCondition);
            this.grpSessionVolumeInfo.Controls.Add(this.lblSessionVolumeInfoConditionLabel);
            this.grpSessionVolumeInfo.Controls.Add(this.lblSessionVolumeInfoIndex);
            this.grpSessionVolumeInfo.Controls.Add(this.lblSessionVolumeInfoIndexLabel);
            this.grpSessionVolumeInfo.Controls.Add(this.lblSessionVolumeInfoDateTime);
            this.grpSessionVolumeInfo.Controls.Add(this.lblSessionVolumeInfoDateTimeLabel);
            this.grpSessionVolumeInfo.Controls.Add(this.lblSessionVolumeInfoID);
            this.grpSessionVolumeInfo.Controls.Add(this.lblSessionVolumeInfoFilename);
            this.grpSessionVolumeInfo.Controls.Add(this.lblSessionVolumeInfoIDLabel);
            this.grpSessionVolumeInfo.Controls.Add(this.lblSessionVolumeInfoFilenameLabel);
            this.grpSessionVolumeInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.grpSessionVolumeInfo.Location = new System.Drawing.Point(17, 256);
            this.grpSessionVolumeInfo.Margin = new System.Windows.Forms.Padding(4);
            this.grpSessionVolumeInfo.Name = "grpSessionVolumeInfo";
            this.grpSessionVolumeInfo.Padding = new System.Windows.Forms.Padding(4);
            this.grpSessionVolumeInfo.Size = new System.Drawing.Size(1124, 128);
            this.grpSessionVolumeInfo.TabIndex = 19;
            this.grpSessionVolumeInfo.TabStop = false;
            this.grpSessionVolumeInfo.Text = "Volume info";
            // 
            // btnSessionVolumeInfoFilenameBrowse
            // 
            this.btnSessionVolumeInfoFilenameBrowse.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.btnSessionVolumeInfoFilenameBrowse.Location = new System.Drawing.Point(1054, 103);
            this.btnSessionVolumeInfoFilenameBrowse.Margin = new System.Windows.Forms.Padding(4);
            this.btnSessionVolumeInfoFilenameBrowse.Name = "btnSessionVolumeInfoFilenameBrowse";
            this.btnSessionVolumeInfoFilenameBrowse.Size = new System.Drawing.Size(62, 17);
            this.btnSessionVolumeInfoFilenameBrowse.TabIndex = 15;
            this.btnSessionVolumeInfoFilenameBrowse.Text = "...";
            this.btnSessionVolumeInfoFilenameBrowse.UseVisualStyleBackColor = true;
            // 
            // lblSessionVolumeInfoCondition
            // 
            this.lblSessionVolumeInfoCondition.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblSessionVolumeInfoCondition.Location = new System.Drawing.Point(143, 82);
            this.lblSessionVolumeInfoCondition.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSessionVolumeInfoCondition.Name = "lblSessionVolumeInfoCondition";
            this.lblSessionVolumeInfoCondition.Size = new System.Drawing.Size(253, 21);
            this.lblSessionVolumeInfoCondition.TabIndex = 14;
            this.lblSessionVolumeInfoCondition.Text = "-";
            // 
            // lblSessionVolumeInfoConditionLabel
            // 
            this.lblSessionVolumeInfoConditionLabel.AutoSize = true;
            this.lblSessionVolumeInfoConditionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblSessionVolumeInfoConditionLabel.Location = new System.Drawing.Point(60, 82);
            this.lblSessionVolumeInfoConditionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSessionVolumeInfoConditionLabel.Name = "lblSessionVolumeInfoConditionLabel";
            this.lblSessionVolumeInfoConditionLabel.Size = new System.Drawing.Size(75, 18);
            this.lblSessionVolumeInfoConditionLabel.TabIndex = 13;
            this.lblSessionVolumeInfoConditionLabel.Text = "Condition:";
            this.lblSessionVolumeInfoConditionLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblSessionVolumeInfoIndex
            // 
            this.lblSessionVolumeInfoIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblSessionVolumeInfoIndex.Location = new System.Drawing.Point(143, 40);
            this.lblSessionVolumeInfoIndex.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSessionVolumeInfoIndex.Name = "lblSessionVolumeInfoIndex";
            this.lblSessionVolumeInfoIndex.Size = new System.Drawing.Size(253, 21);
            this.lblSessionVolumeInfoIndex.TabIndex = 12;
            this.lblSessionVolumeInfoIndex.Text = "-";
            // 
            // lblSessionVolumeInfoIndexLabel
            // 
            this.lblSessionVolumeInfoIndexLabel.AutoSize = true;
            this.lblSessionVolumeInfoIndexLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblSessionVolumeInfoIndexLabel.Location = new System.Drawing.Point(19, 40);
            this.lblSessionVolumeInfoIndexLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSessionVolumeInfoIndexLabel.Name = "lblSessionVolumeInfoIndexLabel";
            this.lblSessionVolumeInfoIndexLabel.Size = new System.Drawing.Size(117, 18);
            this.lblSessionVolumeInfoIndexLabel.TabIndex = 11;
            this.lblSessionVolumeInfoIndexLabel.Text = "Index in session:";
            this.lblSessionVolumeInfoIndexLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblSessionVolumeInfoDateTime
            // 
            this.lblSessionVolumeInfoDateTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblSessionVolumeInfoDateTime.Location = new System.Drawing.Point(143, 61);
            this.lblSessionVolumeInfoDateTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSessionVolumeInfoDateTime.Name = "lblSessionVolumeInfoDateTime";
            this.lblSessionVolumeInfoDateTime.Size = new System.Drawing.Size(419, 21);
            this.lblSessionVolumeInfoDateTime.TabIndex = 10;
            this.lblSessionVolumeInfoDateTime.Text = "-";
            // 
            // lblSessionVolumeInfoDateTimeLabel
            // 
            this.lblSessionVolumeInfoDateTimeLabel.AutoSize = true;
            this.lblSessionVolumeInfoDateTimeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblSessionVolumeInfoDateTimeLabel.Location = new System.Drawing.Point(54, 61);
            this.lblSessionVolumeInfoDateTimeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSessionVolumeInfoDateTimeLabel.Name = "lblSessionVolumeInfoDateTimeLabel";
            this.lblSessionVolumeInfoDateTimeLabel.Size = new System.Drawing.Size(80, 18);
            this.lblSessionVolumeInfoDateTimeLabel.TabIndex = 9;
            this.lblSessionVolumeInfoDateTimeLabel.Text = "Date/Time:";
            this.lblSessionVolumeInfoDateTimeLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblSessionVolumeInfoID
            // 
            this.lblSessionVolumeInfoID.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblSessionVolumeInfoID.Location = new System.Drawing.Point(143, 20);
            this.lblSessionVolumeInfoID.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSessionVolumeInfoID.Name = "lblSessionVolumeInfoID";
            this.lblSessionVolumeInfoID.Size = new System.Drawing.Size(253, 20);
            this.lblSessionVolumeInfoID.TabIndex = 8;
            this.lblSessionVolumeInfoID.Text = "-";
            // 
            // lblSessionVolumeInfoFilename
            // 
            this.lblSessionVolumeInfoFilename.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblSessionVolumeInfoFilename.Location = new System.Drawing.Point(143, 103);
            this.lblSessionVolumeInfoFilename.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSessionVolumeInfoFilename.Name = "lblSessionVolumeInfoFilename";
            this.lblSessionVolumeInfoFilename.Size = new System.Drawing.Size(903, 17);
            this.lblSessionVolumeInfoFilename.TabIndex = 7;
            this.lblSessionVolumeInfoFilename.Text = "-";
            // 
            // lblSessionVolumeInfoIDLabel
            // 
            this.lblSessionVolumeInfoIDLabel.AutoSize = true;
            this.lblSessionVolumeInfoIDLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblSessionVolumeInfoIDLabel.Location = new System.Drawing.Point(106, 20);
            this.lblSessionVolumeInfoIDLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSessionVolumeInfoIDLabel.Name = "lblSessionVolumeInfoIDLabel";
            this.lblSessionVolumeInfoIDLabel.Size = new System.Drawing.Size(26, 18);
            this.lblSessionVolumeInfoIDLabel.TabIndex = 6;
            this.lblSessionVolumeInfoIDLabel.Text = "ID:";
            this.lblSessionVolumeInfoIDLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblSessionVolumeInfoFilenameLabel
            // 
            this.lblSessionVolumeInfoFilenameLabel.AutoSize = true;
            this.lblSessionVolumeInfoFilenameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblSessionVolumeInfoFilenameLabel.Location = new System.Drawing.Point(62, 103);
            this.lblSessionVolumeInfoFilenameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSessionVolumeInfoFilenameLabel.Name = "lblSessionVolumeInfoFilenameLabel";
            this.lblSessionVolumeInfoFilenameLabel.Size = new System.Drawing.Size(72, 18);
            this.lblSessionVolumeInfoFilenameLabel.TabIndex = 4;
            this.lblSessionVolumeInfoFilenameLabel.Text = "Filename:";
            this.lblSessionVolumeInfoFilenameLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lstSessionVolumes
            // 
            this.lstSessionVolumes.FormattingEnabled = true;
            this.lstSessionVolumes.ItemHeight = 18;
            this.lstSessionVolumes.Location = new System.Drawing.Point(202, 10);
            this.lstSessionVolumes.Name = "lstSessionVolumes";
            this.lstSessionVolumes.Size = new System.Drawing.Size(927, 238);
            this.lstSessionVolumes.TabIndex = 18;
            this.lstSessionVolumes.SelectedIndexChanged += new System.EventHandler(this.lstSessionVolumes_SelectedIndexChanged);
            // 
            // btnSessionVolumeVariableImport
            // 
            this.btnSessionVolumeVariableImport.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.btnSessionVolumeVariableImport.Location = new System.Drawing.Point(12, 216);
            this.btnSessionVolumeVariableImport.Margin = new System.Windows.Forms.Padding(4);
            this.btnSessionVolumeVariableImport.Name = "btnSessionVolumeVariableImport";
            this.btnSessionVolumeVariableImport.Size = new System.Drawing.Size(177, 29);
            this.btnSessionVolumeVariableImport.TabIndex = 17;
            this.btnSessionVolumeVariableImport.Text = "Import from file";
            this.btnSessionVolumeVariableImport.UseVisualStyleBackColor = true;
            this.btnSessionVolumeVariableImport.Click += new System.EventHandler(this.btnSessionVolumeVariableImport_Click);
            // 
            // lstSessionVolumeVariables
            // 
            this.lstSessionVolumeVariables.FormattingEnabled = true;
            this.lstSessionVolumeVariables.ItemHeight = 18;
            this.lstSessionVolumeVariables.Location = new System.Drawing.Point(11, 10);
            this.lstSessionVolumeVariables.Name = "lstSessionVolumeVariables";
            this.lstSessionVolumeVariables.Size = new System.Drawing.Size(178, 202);
            this.lstSessionVolumeVariables.TabIndex = 1;
            this.lstSessionVolumeVariables.SelectedIndexChanged += new System.EventHandler(this.lstSessionVolumeVariables_SelectedIndexChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lstSessionTrialVariables);
            this.tabPage2.Controls.Add(this.btnSessionTrialsVariableImport);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1152, 397);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Trials";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lstSessionTrialVariables
            // 
            this.lstSessionTrialVariables.FormattingEnabled = true;
            this.lstSessionTrialVariables.ItemHeight = 18;
            this.lstSessionTrialVariables.Location = new System.Drawing.Point(11, 10);
            this.lstSessionTrialVariables.Name = "lstSessionTrialVariables";
            this.lstSessionTrialVariables.Size = new System.Drawing.Size(178, 202);
            this.lstSessionTrialVariables.TabIndex = 20;
            // 
            // btnSessionTrialsVariableImport
            // 
            this.btnSessionTrialsVariableImport.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.btnSessionTrialsVariableImport.Location = new System.Drawing.Point(11, 219);
            this.btnSessionTrialsVariableImport.Margin = new System.Windows.Forms.Padding(4);
            this.btnSessionTrialsVariableImport.Name = "btnSessionTrialsVariableImport";
            this.btnSessionTrialsVariableImport.Size = new System.Drawing.Size(178, 29);
            this.btnSessionTrialsVariableImport.TabIndex = 19;
            this.btnSessionTrialsVariableImport.Text = "Import trials from file";
            this.btnSessionTrialsVariableImport.UseVisualStyleBackColor = true;
            this.btnSessionTrialsVariableImport.Click += new System.EventHandler(this.btnSessionTrialsVariableImport_Click);
            // 
            // lblSessionTimedata
            // 
            this.lblSessionTimedata.AutoSize = true;
            this.lblSessionTimedata.Location = new System.Drawing.Point(30, 460);
            this.lblSessionTimedata.Name = "lblSessionTimedata";
            this.lblSessionTimedata.Size = new System.Drawing.Size(45, 18);
            this.lblSessionTimedata.TabIndex = 20;
            this.lblSessionTimedata.Text = "Time:";
            // 
            // picSessionTimedata
            // 
            this.picSessionTimedata.Location = new System.Drawing.Point(31, 487);
            this.picSessionTimedata.Name = "picSessionTimedata";
            this.picSessionTimedata.Size = new System.Drawing.Size(1125, 238);
            this.picSessionTimedata.TabIndex = 19;
            this.picSessionTimedata.TabStop = false;
            // 
            // tabData
            // 
            this.tabData.BackColor = System.Drawing.SystemColors.Control;
            this.tabData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabData.Controls.Add(this.button1);
            this.tabData.Location = new System.Drawing.Point(0, 23);
            this.tabData.Margin = new System.Windows.Forms.Padding(4);
            this.tabData.Name = "tabData";
            this.tabData.Size = new System.Drawing.Size(1181, 793);
            this.tabData.TabIndex = 2;
            this.tabData.Text = "Data";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.button1.Location = new System.Drawing.Point(60, 77);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(209, 63);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tabMatlab
            // 
            this.tabMatlab.BackColor = System.Drawing.SystemColors.Control;
            this.tabMatlab.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabMatlab.Controls.Add(this.button2);
            this.tabMatlab.Controls.Add(this.grpMatlab);
            this.tabMatlab.Controls.Add(this.grpMatlabConsole);
            this.tabMatlab.Location = new System.Drawing.Point(0, 23);
            this.tabMatlab.Margin = new System.Windows.Forms.Padding(4);
            this.tabMatlab.Name = "tabMatlab";
            this.tabMatlab.Size = new System.Drawing.Size(1181, 793);
            this.tabMatlab.TabIndex = 3;
            this.tabMatlab.Text = "Matlab";
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.button2.Location = new System.Drawing.Point(103, 143);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(116, 34);
            this.button2.TabIndex = 22;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // grpMatlab
            // 
            this.grpMatlab.Controls.Add(this.lblMatlabRunning);
            this.grpMatlab.Controls.Add(this.lblMatlabRunningLabel);
            this.grpMatlab.Controls.Add(this.btnMatlabStartStop);
            this.grpMatlab.Controls.Add(this.lblMatlabVersion);
            this.grpMatlab.Controls.Add(this.lblMatlabVersionLabel);
            this.grpMatlab.Controls.Add(this.lblMatlabState);
            this.grpMatlab.Controls.Add(this.lblMatlabStateLabel);
            this.grpMatlab.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.grpMatlab.Location = new System.Drawing.Point(15, 4);
            this.grpMatlab.Margin = new System.Windows.Forms.Padding(4);
            this.grpMatlab.Name = "grpMatlab";
            this.grpMatlab.Padding = new System.Windows.Forms.Padding(4);
            this.grpMatlab.Size = new System.Drawing.Size(1127, 108);
            this.grpMatlab.TabIndex = 21;
            this.grpMatlab.TabStop = false;
            // 
            // lblMatlabRunning
            // 
            this.lblMatlabRunning.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblMatlabRunning.Location = new System.Drawing.Point(133, 75);
            this.lblMatlabRunning.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMatlabRunning.Name = "lblMatlabRunning";
            this.lblMatlabRunning.Size = new System.Drawing.Size(469, 22);
            this.lblMatlabRunning.TabIndex = 30;
            this.lblMatlabRunning.Text = "-";
            // 
            // lblMatlabRunningLabel
            // 
            this.lblMatlabRunningLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblMatlabRunningLabel.Location = new System.Drawing.Point(15, 75);
            this.lblMatlabRunningLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMatlabRunningLabel.Name = "lblMatlabRunningLabel";
            this.lblMatlabRunningLabel.Size = new System.Drawing.Size(111, 20);
            this.lblMatlabRunningLabel.TabIndex = 29;
            this.lblMatlabRunningLabel.Text = "Running:";
            this.lblMatlabRunningLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btnMatlabStartStop
            // 
            this.btnMatlabStartStop.Enabled = false;
            this.btnMatlabStartStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.btnMatlabStartStop.Location = new System.Drawing.Point(865, 39);
            this.btnMatlabStartStop.Margin = new System.Windows.Forms.Padding(4);
            this.btnMatlabStartStop.Name = "btnMatlabStartStop";
            this.btnMatlabStartStop.Size = new System.Drawing.Size(235, 37);
            this.btnMatlabStartStop.TabIndex = 27;
            this.btnMatlabStartStop.Text = "Start";
            this.btnMatlabStartStop.UseVisualStyleBackColor = true;
            this.btnMatlabStartStop.Click += new System.EventHandler(this.btnMatlabStartStop_Click);
            // 
            // lblMatlabVersion
            // 
            this.lblMatlabVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblMatlabVersion.Location = new System.Drawing.Point(133, 48);
            this.lblMatlabVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMatlabVersion.Name = "lblMatlabVersion";
            this.lblMatlabVersion.Size = new System.Drawing.Size(469, 22);
            this.lblMatlabVersion.TabIndex = 24;
            this.lblMatlabVersion.Text = "-";
            // 
            // lblMatlabVersionLabel
            // 
            this.lblMatlabVersionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblMatlabVersionLabel.Location = new System.Drawing.Point(15, 48);
            this.lblMatlabVersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMatlabVersionLabel.Name = "lblMatlabVersionLabel";
            this.lblMatlabVersionLabel.Size = new System.Drawing.Size(111, 20);
            this.lblMatlabVersionLabel.TabIndex = 23;
            this.lblMatlabVersionLabel.Text = "Version:";
            this.lblMatlabVersionLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblMatlabState
            // 
            this.lblMatlabState.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblMatlabState.Location = new System.Drawing.Point(133, 21);
            this.lblMatlabState.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMatlabState.Name = "lblMatlabState";
            this.lblMatlabState.Size = new System.Drawing.Size(469, 22);
            this.lblMatlabState.TabIndex = 22;
            this.lblMatlabState.Text = "-";
            // 
            // lblMatlabStateLabel
            // 
            this.lblMatlabStateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.lblMatlabStateLabel.Location = new System.Drawing.Point(15, 21);
            this.lblMatlabStateLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMatlabStateLabel.Name = "lblMatlabStateLabel";
            this.lblMatlabStateLabel.Size = new System.Drawing.Size(111, 20);
            this.lblMatlabStateLabel.TabIndex = 21;
            this.lblMatlabStateLabel.Text = "State:";
            this.lblMatlabStateLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // grpMatlabConsole
            // 
            this.grpMatlabConsole.Controls.Add(this.txtMatlabConsole);
            this.grpMatlabConsole.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.grpMatlabConsole.Location = new System.Drawing.Point(15, 222);
            this.grpMatlabConsole.Margin = new System.Windows.Forms.Padding(4);
            this.grpMatlabConsole.Name = "grpMatlabConsole";
            this.grpMatlabConsole.Padding = new System.Windows.Forms.Padding(4);
            this.grpMatlabConsole.Size = new System.Drawing.Size(1147, 550);
            this.grpMatlabConsole.TabIndex = 10;
            this.grpMatlabConsole.TabStop = false;
            this.grpMatlabConsole.Text = "Console";
            // 
            // txtMatlabConsole
            // 
            this.txtMatlabConsole.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.txtMatlabConsole.Location = new System.Drawing.Point(20, 32);
            this.txtMatlabConsole.Margin = new System.Windows.Forms.Padding(4);
            this.txtMatlabConsole.Name = "txtMatlabConsole";
            this.txtMatlabConsole.ReadOnly = true;
            this.txtMatlabConsole.Size = new System.Drawing.Size(1105, 501);
            this.txtMatlabConsole.TabIndex = 0;
            this.txtMatlabConsole.Text = "";
            // 
            // GUIMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1179, 815);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GUIMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "RETIF-4";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GUI_FormClosing);
            this.Load += new System.EventHandler(this.GUI_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GUIMain_KeyDown);
            this.tabControl.ResumeLayout(false);
            this.tabExperiment.ResumeLayout(false);
            this.grpConsole.ResumeLayout(false);
            this.grpExp.ResumeLayout(false);
            this.grpExp.PerformLayout();
            this.tabView.ResumeLayout(false);
            this.grpViewInfo.ResumeLayout(false);
            this.grpViewInfo.PerformLayout();
            this.grpViewPosSize.ResumeLayout(false);
            this.grpViewPosSize.PerformLayout();
            this.pnlViewPos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picViewPosWindow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picViewPosScreen)).EndInit();
            this.tabSession.ResumeLayout(false);
            this.tabSession.PerformLayout();
            this.tabSessionVariables.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.grpSessionVolumeInfo.ResumeLayout(false);
            this.grpSessionVolumeInfo.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picSessionTimedata)).EndInit();
            this.tabData.ResumeLayout(false);
            this.tabMatlab.ResumeLayout(false);
            this.grpMatlab.ResumeLayout(false);
            this.grpMatlabConsole.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private NoBorderTabControl tabControl;
        private System.Windows.Forms.TabPage tabExperiment;
        private System.Windows.Forms.TabPage tabView;
        private System.Windows.Forms.TabPage tabData;
        private System.Windows.Forms.GroupBox grpExp;
        private System.Windows.Forms.Label lblCurrentStatus;
        private System.Windows.Forms.Label lblCurrentPhase;
        private System.Windows.Forms.Label lblCurrentStatusLabel;
        private System.Windows.Forms.Label lblCurrentPhaseLabel;
        private System.Windows.Forms.Button btnPhaseStart;
        private System.Windows.Forms.Label lblPhase;
        private System.Windows.Forms.ComboBox cmbPhase;
        private System.Windows.Forms.CheckBox chkPhaseAutoContinue;
        private System.Windows.Forms.Button btnViewStart;
        private System.Windows.Forms.GroupBox grpViewPosSize;
        private System.Windows.Forms.Button btnViewSetContent;
        private System.Windows.Forms.TextBox txtViewWindowHeight;
        private System.Windows.Forms.Label lblViewWindowHeight;
        private System.Windows.Forms.TextBox txtViewWindowWidth;
        private System.Windows.Forms.Label lblViewWindowWidth;
        private System.Windows.Forms.TextBox txtViewWindowY;
        private System.Windows.Forms.Label lblViewWindowY;
        private System.Windows.Forms.TextBox txtViewWindowX;
        private System.Windows.Forms.Label lblViewWindowX;
        private System.Windows.Forms.Label lblViewContent;
        private System.Windows.Forms.TextBox txtViewContentHeight;
        private System.Windows.Forms.Label lblViewContentHeight;
        private System.Windows.Forms.TextBox txtViewContentWidth;
        private System.Windows.Forms.Label lblViewContentWidth;
        private System.Windows.Forms.Label lblViewWindow;
        private System.Windows.Forms.Button btnViewSetWindow;
        private System.Windows.Forms.GroupBox grpConsole;
        private RTFScrolledBottom txtConsole;
        private System.Windows.Forms.Timer tmrViewUpdate;
        private System.Windows.Forms.Label lblViewWindowBorder;
        private System.Windows.Forms.CheckBox chkViewWindowBorder;
        private System.Windows.Forms.Panel pnlViewPos;
        private System.Windows.Forms.GroupBox grpViewInfo;
        private System.Windows.Forms.TextBox txtViewInfo;
        private System.Windows.Forms.ComboBox cmbPhaseStarttype;
        private System.Windows.Forms.Timer tmrExperimentUpdate;
        private System.Windows.Forms.Label lblPhaseStarttype;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabPage tabMatlab;
        private System.Windows.Forms.Timer tmrMatlabUpdate;
        private System.Windows.Forms.Timer tmrGUI;
        private System.Windows.Forms.GroupBox grpMatlab;
        private System.Windows.Forms.Button btnMatlabStartStop;
        private System.Windows.Forms.Label lblMatlabVersion;
        private System.Windows.Forms.Label lblMatlabVersionLabel;
        private System.Windows.Forms.Label lblMatlabState;
        private System.Windows.Forms.Label lblMatlabStateLabel;
        private System.Windows.Forms.GroupBox grpMatlabConsole;
        private System.Windows.Forms.RichTextBox txtMatlabConsole;
        private System.Windows.Forms.Label lblMatlabRunning;
        private System.Windows.Forms.Label lblMatlabRunningLabel;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TabPage tabSession;
        private System.Windows.Forms.Timer tmrSessionUpdate;
        private System.Windows.Forms.PictureBox picViewPosScreen;
        private System.Windows.Forms.PictureBox picViewPosWindow;
        private System.Windows.Forms.Label lblSessionTimedata;
        private System.Windows.Forms.PictureBox picSessionTimedata;
        private System.Windows.Forms.TabControl tabSessionVariables;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button btnSessionVolumeVariableImport;
        private System.Windows.Forms.ListBox lstSessionVolumeVariables;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListBox lstSessionVolumes;
        private System.Windows.Forms.Button btnSessionTrialsVariableImport;
        private System.Windows.Forms.GroupBox grpSessionVolumeInfo;
        private System.Windows.Forms.Button btnSessionVolumeInfoFilenameBrowse;
        private System.Windows.Forms.Label lblSessionVolumeInfoCondition;
        private System.Windows.Forms.Label lblSessionVolumeInfoConditionLabel;
        private System.Windows.Forms.Label lblSessionVolumeInfoIndex;
        private System.Windows.Forms.Label lblSessionVolumeInfoIndexLabel;
        private System.Windows.Forms.Label lblSessionVolumeInfoDateTime;
        private System.Windows.Forms.Label lblSessionVolumeInfoDateTimeLabel;
        private System.Windows.Forms.Label lblSessionVolumeInfoID;
        private System.Windows.Forms.Label lblSessionVolumeInfoFilename;
        private System.Windows.Forms.Label lblSessionVolumeInfoIDLabel;
        private System.Windows.Forms.Label lblSessionVolumeInfoFilenameLabel;
        private System.Windows.Forms.ListBox lstSessionTrialVariables;
    }
}