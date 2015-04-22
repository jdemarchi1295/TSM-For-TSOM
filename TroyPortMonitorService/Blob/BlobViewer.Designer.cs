namespace TroyPortMonitorService.Blob
{
    partial class BlobViewer
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
            this.label2 = new System.Windows.Forms.Label();
            this.txtBlobFont = new System.Windows.Forms.TextBox();
            this.txtFontGlyphMap = new System.Windows.Forms.TextBox();
            this.btnPickAFile = new System.Windows.Forms.Button();
            this.btnPickAFont = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.fontDialog1 = new System.Windows.Forms.FontDialog();
            this.label3 = new System.Windows.Forms.Label();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.btnPickBlobFile = new System.Windows.Forms.Button();
            this.btnReadBlob = new System.Windows.Forms.Button();
            this.tvResults = new System.Windows.Forms.TreeView();
            this.label4 = new System.Windows.Forms.Label();
            this.lstResults = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtConfig = new System.Windows.Forms.TextBox();
            this.btnPickConfig = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(301, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Blob Font:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(235, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(121, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Glyph To Ascii Map File:";
            // 
            // txtBlobFont
            // 
            this.txtBlobFont.Location = new System.Drawing.Point(362, 16);
            this.txtBlobFont.Name = "txtBlobFont";
            this.txtBlobFont.Size = new System.Drawing.Size(175, 20);
            this.txtBlobFont.TabIndex = 2;
            this.txtBlobFont.Text = "Microsoft Sans ";
            // 
            // txtFontGlyphMap
            // 
            this.txtFontGlyphMap.Location = new System.Drawing.Point(362, 49);
            this.txtFontGlyphMap.Name = "txtFontGlyphMap";
            this.txtFontGlyphMap.Size = new System.Drawing.Size(175, 20);
            this.txtFontGlyphMap.TabIndex = 3;
            this.txtFontGlyphMap.Text = "TimesNewRoman.csv";
            // 
            // btnPickAFile
            // 
            this.btnPickAFile.Location = new System.Drawing.Point(543, 46);
            this.btnPickAFile.Name = "btnPickAFile";
            this.btnPickAFile.Size = new System.Drawing.Size(26, 23);
            this.btnPickAFile.TabIndex = 4;
            this.btnPickAFile.Text = "...";
            this.btnPickAFile.UseVisualStyleBackColor = true;
            this.btnPickAFile.Click += new System.EventHandler(this.btnPickAFile_Click);
            // 
            // btnPickAFont
            // 
            this.btnPickAFont.Location = new System.Drawing.Point(543, 14);
            this.btnPickAFont.Name = "btnPickAFont";
            this.btnPickAFont.Size = new System.Drawing.Size(26, 23);
            this.btnPickAFont.TabIndex = 5;
            this.btnPickAFont.Text = "...";
            this.btnPickAFont.UseVisualStyleBackColor = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 105);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "File Containing Blob:";
            // 
            // txtFile
            // 
            this.txtFile.Location = new System.Drawing.Point(122, 105);
            this.txtFile.Name = "txtFile";
            this.txtFile.Size = new System.Drawing.Size(415, 20);
            this.txtFile.TabIndex = 7;
            // 
            // btnPickBlobFile
            // 
            this.btnPickBlobFile.Location = new System.Drawing.Point(539, 105);
            this.btnPickBlobFile.Name = "btnPickBlobFile";
            this.btnPickBlobFile.Size = new System.Drawing.Size(30, 23);
            this.btnPickBlobFile.TabIndex = 8;
            this.btnPickBlobFile.Text = "...";
            this.btnPickBlobFile.UseVisualStyleBackColor = true;
            this.btnPickBlobFile.Click += new System.EventHandler(this.btnPickBlobFile_Click);
            // 
            // btnReadBlob
            // 
            this.btnReadBlob.Location = new System.Drawing.Point(12, 187);
            this.btnReadBlob.Name = "btnReadBlob";
            this.btnReadBlob.Size = new System.Drawing.Size(86, 28);
            this.btnReadBlob.TabIndex = 9;
            this.btnReadBlob.Text = "Read Blob";
            this.btnReadBlob.UseVisualStyleBackColor = true;
            this.btnReadBlob.Click += new System.EventHandler(this.btnReadBlob_Click);
            // 
            // tvResults
            // 
            this.tvResults.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tvResults.Location = new System.Drawing.Point(10, 221);
            this.tvResults.Name = "tvResults";
            this.tvResults.Size = new System.Drawing.Size(564, 226);
            this.tvResults.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(9, 469);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 17);
            this.label4.TabIndex = 13;
            this.label4.Text = "Status";
            // 
            // lstResults
            // 
            this.lstResults.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstResults.FormattingEnabled = true;
            this.lstResults.HorizontalScrollbar = true;
            this.lstResults.Location = new System.Drawing.Point(9, 489);
            this.lstResults.Name = "lstResults";
            this.lstResults.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstResults.Size = new System.Drawing.Size(564, 82);
            this.lstResults.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 146);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(171, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "PortMonitorConfigurationTsom File:";
            // 
            // txtConfig
            // 
            this.txtConfig.Location = new System.Drawing.Point(186, 143);
            this.txtConfig.Name = "txtConfig";
            this.txtConfig.Size = new System.Drawing.Size(350, 20);
            this.txtConfig.TabIndex = 15;
            // 
            // btnPickConfig
            // 
            this.btnPickConfig.Location = new System.Drawing.Point(539, 141);
            this.btnPickConfig.Name = "btnPickConfig";
            this.btnPickConfig.Size = new System.Drawing.Size(30, 23);
            this.btnPickConfig.TabIndex = 16;
            this.btnPickConfig.Text = "...";
            this.btnPickConfig.UseVisualStyleBackColor = true;
            this.btnPickConfig.Click += new System.EventHandler(this.btnPickConfig_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(235, 166);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(231, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "(Optioinal.  If blank, typical settings will be used)";
            // 
            // BlobViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 592);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnPickConfig);
            this.Controls.Add(this.txtConfig);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lstResults);
            this.Controls.Add(this.tvResults);
            this.Controls.Add(this.btnReadBlob);
            this.Controls.Add(this.btnPickBlobFile);
            this.Controls.Add(this.txtFile);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnPickAFont);
            this.Controls.Add(this.btnPickAFile);
            this.Controls.Add(this.txtFontGlyphMap);
            this.Controls.Add(this.txtBlobFont);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "BlobViewer";
            this.Text = "BlobViewer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBlobFont;
        private System.Windows.Forms.TextBox txtFontGlyphMap;
        private System.Windows.Forms.Button btnPickAFile;
        private System.Windows.Forms.Button btnPickAFont;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FontDialog fontDialog1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.Button btnPickBlobFile;
        private System.Windows.Forms.Button btnReadBlob;
        private System.Windows.Forms.TreeView tvResults;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox lstResults;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtConfig;
        private System.Windows.Forms.Button btnPickConfig;
        private System.Windows.Forms.Label label6;
    }
}