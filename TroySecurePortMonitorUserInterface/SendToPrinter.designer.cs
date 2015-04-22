namespace TroySecurePortMonitorUserInterface
{
    partial class SendToPrinter
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
            this.label1 = new System.Windows.Forms.Label();
            this.cboPrinter = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPrinterLogin = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtConfirm = new System.Windows.Forms.TextBox();
            this.btnUpdatePrinter = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radAlways = new System.Windows.Forms.RadioButton();
            this.radJobByJob = new System.Windows.Forms.RadioButton();
            this.radNoChange = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Printer:";
            // 
            // cboPrinter
            // 
            this.cboPrinter.FormattingEnabled = true;
            this.cboPrinter.Location = new System.Drawing.Point(87, 30);
            this.cboPrinter.Name = "cboPrinter";
            this.cboPrinter.Size = new System.Drawing.Size(305, 21);
            this.cboPrinter.TabIndex = 1;
            this.cboPrinter.SelectedIndexChanged += new System.EventHandler(this.cboPrinter_SelectedIndexChanged);
            this.cboPrinter.TextUpdate += new System.EventHandler(this.cboPrinter_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(150, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Printer Admin Login Password:";
            // 
            // txtPrinterLogin
            // 
            this.txtPrinterLogin.Location = new System.Drawing.Point(196, 82);
            this.txtPrinterLogin.Name = "txtPrinterLogin";
            this.txtPrinterLogin.Size = new System.Drawing.Size(186, 20);
            this.txtPrinterLogin.TabIndex = 3;
            this.txtPrinterLogin.UseSystemPasswordChar = true;
            this.txtPrinterLogin.TextChanged += new System.EventHandler(this.txtPrinterLogin_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(40, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Confim Admin Login Password:";
            // 
            // txtConfirm
            // 
            this.txtConfirm.Location = new System.Drawing.Point(198, 113);
            this.txtConfirm.Name = "txtConfirm";
            this.txtConfirm.Size = new System.Drawing.Size(186, 20);
            this.txtConfirm.TabIndex = 5;
            this.txtConfirm.UseSystemPasswordChar = true;
            this.txtConfirm.TextChanged += new System.EventHandler(this.txtConfirm_TextChanged);
            // 
            // btnUpdatePrinter
            // 
            this.btnUpdatePrinter.Enabled = false;
            this.btnUpdatePrinter.Location = new System.Drawing.Point(43, 163);
            this.btnUpdatePrinter.Name = "btnUpdatePrinter";
            this.btnUpdatePrinter.Size = new System.Drawing.Size(96, 30);
            this.btnUpdatePrinter.TabIndex = 6;
            this.btnUpdatePrinter.Text = "Update Printer";
            this.btnUpdatePrinter.UseVisualStyleBackColor = true;
            this.btnUpdatePrinter.Click += new System.EventHandler(this.btnUpdatePrinter_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(473, 187);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(82, 24);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radAlways);
            this.groupBox1.Controls.Add(this.radJobByJob);
            this.groupBox1.Controls.Add(this.radNoChange);
            this.groupBox1.Location = new System.Drawing.Point(431, 30);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(124, 112);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Decryption Mode";
            // 
            // radAlways
            // 
            this.radAlways.AutoSize = true;
            this.radAlways.Location = new System.Drawing.Point(11, 82);
            this.radAlways.Name = "radAlways";
            this.radAlways.Size = new System.Drawing.Size(58, 17);
            this.radAlways.TabIndex = 2;
            this.radAlways.Text = "Always";
            this.radAlways.UseVisualStyleBackColor = true;
            // 
            // radJobByJob
            // 
            this.radJobByJob.AutoSize = true;
            this.radJobByJob.Location = new System.Drawing.Point(11, 51);
            this.radJobByJob.Name = "radJobByJob";
            this.radJobByJob.Size = new System.Drawing.Size(77, 17);
            this.radJobByJob.TabIndex = 1;
            this.radJobByJob.Text = "Job By Job";
            this.radJobByJob.UseVisualStyleBackColor = true;
            // 
            // radNoChange
            // 
            this.radNoChange.AutoSize = true;
            this.radNoChange.Checked = true;
            this.radNoChange.Location = new System.Drawing.Point(11, 20);
            this.radNoChange.Name = "radNoChange";
            this.radNoChange.Size = new System.Drawing.Size(79, 17);
            this.radNoChange.TabIndex = 0;
            this.radNoChange.TabStop = true;
            this.radNoChange.Text = "No Change";
            this.radNoChange.UseVisualStyleBackColor = true;
            // 
            // SendToPrinter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(583, 223);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnUpdatePrinter);
            this.Controls.Add(this.txtConfirm);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtPrinterLogin);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboPrinter);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SendToPrinter";
            this.Text = "Send Encryption Password To Printer";
            this.Load += new System.EventHandler(this.SendToPrinter_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboPrinter;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPrinterLogin;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtConfirm;
        private System.Windows.Forms.Button btnUpdatePrinter;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radAlways;
        private System.Windows.Forms.RadioButton radJobByJob;
        private System.Windows.Forms.RadioButton radNoChange;
    }
}