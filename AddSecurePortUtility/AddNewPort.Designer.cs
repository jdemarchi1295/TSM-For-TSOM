namespace AddSecurePortUtility
{
    partial class frmMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMainForm));
            this.btnConfigPath = new System.Windows.Forms.Button();
            this.txtConfigPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPrintPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboTroyPort = new System.Windows.Forms.ComboBox();
            this.txtNewPortName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnMainCancel = new System.Windows.Forms.Button();
            this.btnMainOK = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblCopyrightInfo = new System.Windows.Forms.Label();
            this.btnAddMultiple = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkMonitored = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnConfigPath
            // 
            this.btnConfigPath.Location = new System.Drawing.Point(577, 190);
            this.btnConfigPath.Name = "btnConfigPath";
            this.btnConfigPath.Size = new System.Drawing.Size(37, 25);
            this.btnConfigPath.TabIndex = 19;
            this.btnConfigPath.Text = "...";
            this.btnConfigPath.UseVisualStyleBackColor = true;
            this.btnConfigPath.Click += new System.EventHandler(this.btnConfigPath_Click);
            // 
            // txtConfigPath
            // 
            this.txtConfigPath.Location = new System.Drawing.Point(130, 193);
            this.txtConfigPath.Name = "txtConfigPath";
            this.txtConfigPath.Size = new System.Drawing.Size(441, 20);
            this.txtConfigPath.TabIndex = 17;
            this.txtConfigPath.Text = "Default";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 196);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "Configuration Path:";
            // 
            // txtPrintPath
            // 
            this.txtPrintPath.Enabled = false;
            this.txtPrintPath.Location = new System.Drawing.Point(89, 115);
            this.txtPrintPath.Name = "txtPrintPath";
            this.txtPrintPath.Size = new System.Drawing.Size(482, 20);
            this.txtPrintPath.TabIndex = 15;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Print Path:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "TROYPORT Port Monitor:";
            // 
            // cboTroyPort
            // 
            this.cboTroyPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTroyPort.FormattingEnabled = true;
            this.cboTroyPort.Location = new System.Drawing.Point(163, 73);
            this.cboTroyPort.Name = "cboTroyPort";
            this.cboTroyPort.Size = new System.Drawing.Size(230, 21);
            this.cboTroyPort.TabIndex = 12;
            this.cboTroyPort.SelectedIndexChanged += new System.EventHandler(this.cboTroyPort_SelectedIndexChanged);
            // 
            // txtNewPortName
            // 
            this.txtNewPortName.Location = new System.Drawing.Point(116, 29);
            this.txtNewPortName.Name = "txtNewPortName";
            this.txtNewPortName.Size = new System.Drawing.Size(277, 20);
            this.txtNewPortName.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "New Port Name:";
            // 
            // btnApply
            // 
            this.btnApply.Enabled = false;
            this.btnApply.Location = new System.Drawing.Point(519, 272);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(87, 25);
            this.btnApply.TabIndex = 22;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnMainCancel
            // 
            this.btnMainCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnMainCancel.Location = new System.Drawing.Point(407, 272);
            this.btnMainCancel.Name = "btnMainCancel";
            this.btnMainCancel.Size = new System.Drawing.Size(87, 25);
            this.btnMainCancel.TabIndex = 21;
            this.btnMainCancel.Text = "Cancel";
            this.btnMainCancel.UseVisualStyleBackColor = true;
            this.btnMainCancel.Click += new System.EventHandler(this.btnMainCancel_Click);
            // 
            // btnMainOK
            // 
            this.btnMainOK.Enabled = false;
            this.btnMainOK.Location = new System.Drawing.Point(290, 272);
            this.btnMainOK.Name = "btnMainOK";
            this.btnMainOK.Size = new System.Drawing.Size(87, 25);
            this.btnMainOK.TabIndex = 20;
            this.btnMainOK.Text = "OK";
            this.btnMainOK.UseVisualStyleBackColor = true;
            this.btnMainOK.Click += new System.EventHandler(this.btnMainOK_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(30, 232);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(181, 58);
            this.pictureBox1.TabIndex = 23;
            this.pictureBox1.TabStop = false;
            // 
            // lblCopyrightInfo
            // 
            this.lblCopyrightInfo.AutoSize = true;
            this.lblCopyrightInfo.Location = new System.Drawing.Point(24, 293);
            this.lblCopyrightInfo.Name = "lblCopyrightInfo";
            this.lblCopyrightInfo.Size = new System.Drawing.Size(170, 13);
            this.lblCopyrightInfo.TabIndex = 24;
            this.lblCopyrightInfo.Text = "Copyright  TROY Group Inc.  2012";
            // 
            // btnAddMultiple
            // 
            this.btnAddMultiple.Location = new System.Drawing.Point(481, 56);
            this.btnAddMultiple.Name = "btnAddMultiple";
            this.btnAddMultiple.Size = new System.Drawing.Size(110, 30);
            this.btnAddMultiple.TabIndex = 25;
            this.btnAddMultiple.Text = "Add Multiple Ports";
            this.btnAddMultiple.UseVisualStyleBackColor = true;
            this.btnAddMultiple.Click += new System.EventHandler(this.btnAddMultiple_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(478, 14);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(126, 13);
            this.label5.TabIndex = 26;
            this.label5.Text = "Number Of Ports To Add:";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(481, 30);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(65, 20);
            this.numericUpDown1.TabIndex = 27;
            this.numericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(451, 1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(168, 95);
            this.groupBox1.TabIndex = 28;
            this.groupBox1.TabStop = false;
            // 
            // chkMonitored
            // 
            this.chkMonitored.AutoSize = true;
            this.chkMonitored.Location = new System.Drawing.Point(30, 154);
            this.chkMonitored.Name = "chkMonitored";
            this.chkMonitored.Size = new System.Drawing.Size(250, 17);
            this.chkMonitored.TabIndex = 29;
            this.chkMonitored.Text = "Use TROYPORT as a Stand-Alone Printer Port ";
            this.chkMonitored.UseVisualStyleBackColor = true;
            // 
            // frmMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 334);
            this.Controls.Add(this.chkMonitored);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnAddMultiple);
            this.Controls.Add(this.lblCopyrightInfo);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnMainCancel);
            this.Controls.Add(this.btnMainOK);
            this.Controls.Add(this.btnConfigPath);
            this.Controls.Add(this.txtConfigPath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtPrintPath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboTroyPort);
            this.Controls.Add(this.txtNewPortName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmMainForm";
            this.Text = "Add TROY SecurePort";
            this.Load += new System.EventHandler(this.frmMainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConfigPath;
        private System.Windows.Forms.TextBox txtConfigPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPrintPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboTroyPort;
        private System.Windows.Forms.TextBox txtNewPortName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnMainCancel;
        private System.Windows.Forms.Button btnMainOK;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblCopyrightInfo;
        private System.Windows.Forms.Button btnAddMultiple;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkMonitored;
    }
}

