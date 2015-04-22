using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Troy.PortMonitor.Core.XmlConfiguration;
using Troy.Core.Configs;
using System.Xml.Serialization;

namespace TroyPortMonitorService.Blob
{
    public partial class BlobViewer : Form
    {
        public BlobViewer()
        {
            InitializeComponent();
        }

        private void btnReadBlob_Click(object sender, EventArgs e)
        {
            tvResults.Nodes.Clear();
            if ((txtBlobFont.Text == "") || (txtFontGlyphMap.Text == "") || (txtFile.Text == ""))
            {
                MessageBox.Show("Font, Glyph To Ascii Map File and File Containing Blob fields must be not be blank");
                return;
            }

            if (!File.Exists(txtFile.Text))
            {
                MessageBox.Show("File Containing Blob does not exist.");
                return;
            }

            if (!File.Exists(txtFontGlyphMap.Text))
            {
                MessageBox.Show("Glyph to Ascii file does not exist.");
                return;
            }
            PortMonitorConfigurationTsom currTpmc;
            if (txtConfig.Text != "")
            {
                if (!File.Exists(txtConfig.Text))
                {
                    MessageBox.Show("PortMonitorConfigurationTsom file not found. ");
                    return;
                }

                XmlSerializer dser = new XmlSerializer(typeof(PortMonitorConfigurationTsom));
                string filename = txtConfig.Text;
                FileStream fs = new FileStream(filename, FileMode.Open);
                currTpmc = (PortMonitorConfigurationTsom)dser.Deserialize(fs);
            }
            else
            {
                currTpmc = new PortMonitorConfigurationTsom();
                currTpmc.EndOfPagePcl.Add("/e*p6400Y/f");  //Letter
                currTpmc.EndOfPagePcl.Add("/e*p8200Y/f");  //Legal
                currTpmc.EndOfPagePcl.Add("/e*p6815Y/f");  //A4
                currTpmc.EndOfPagePcl.Add("/e*p6360Y/f");  //Tesco???
                currTpmc.EndOfPagePcl.Add("/e*rB/f");
                currTpmc.EndOfPagePcl.Add("/e*c0P/f");
                currTpmc.EndOfPagePcl.Add("/e*c2P/f");
                currTpmc.TsomSerializationDataFont = txtBlobFont.Text;
                currTpmc.TsomFontCharacterMapFile = txtFontGlyphMap.Text;
            }

            PclParsing.FontConfigSet fontSet = new PclParsing.FontConfigSet();
            fontSet.AddFont(currTpmc.TsomSerializationDataFont, "", "", currTpmc.TsomFontCharacterMapFile, "SERIALIZATION", false);

            PclParsing.PortMonLogging plogger = new PclParsing.PortMonLogging();
            plogger.EnableMessageBoxes = true;

            PclParsing.PrintJobThread printJob = new PclParsing.PrintJobThread();
            printJob.printFileName = txtFile.Text;
            printJob.pmConfig = currTpmc;
            printJob.pmLogging = plogger;
            printJob.fontConfigs = fontSet;

            if (printJob.CallFromBlobViewer())
            {
                if ((printJob.EndOfPageUsedIndex > -1) && (currTpmc.EndOfPagePcl.Count <= printJob.EndOfPageUsedIndex))
                {
                    lstResults.Items.Add("END OF PAGE PCL = " + currTpmc.EndOfPagePcl[printJob.EndOfPageUsedIndex]);
                }
                Blob.BlobProcessor blobProcessor = new Blob.BlobProcessor();
                for (int cntr = 1; cntr <= printJob.NumberOfPages; cntr++)
                {
                    if (printJob.BlobPerPage.ContainsKey(cntr))
                    {
                        tvResults.BeginUpdate();
                        tvResults.Nodes.Add("Page " + cntr.ToString());
                        string PrinterName = "";
                        bool TSOMEnabledDuplex = false;
                        blobProcessor.ParseBlob(printJob.BlobPerPage[cntr], cntr, ref PrinterName, ref TSOMEnabledDuplex);
                        tvResults.Nodes[cntr - 1].Nodes.Add("Printer Name", "Printer Name: " + PrinterName);
                        if (TSOMEnabledDuplex)
                        {
                            tvResults.Nodes[cntr - 1].Nodes.Add("TSOM Duplex Enabled", "TSOM Duplex Enabled = true");
                        }
                        System.Drawing.Font ndFont1 = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Regular);
                        tvResults.Nodes[cntr - 1].Nodes.Add("AreaInfo (AI)");
                        tvResults.Nodes[cntr - 1].Nodes[0].NodeFont = ndFont1;
                        int cntr2 = 0;
                        foreach (AreaInfo ai in blobProcessor.pgConfig.AI)
                        {
                            tvResults.Nodes[cntr - 1].Nodes[0].Nodes.Add("AI[" + cntr.ToString() + "]");
                            tvResults.Nodes[cntr - 1].Nodes[0].Nodes[cntr2].Nodes.Add("XPos", String.Format("XPos: {0,5}", ai.XPos));
                            tvResults.Nodes[cntr - 1].Nodes[0].Nodes[cntr2].Nodes.Add("YPos", String.Format("YPos: {0,5}", ai.YPos));
                            tvResults.Nodes[cntr - 1].Nodes[0].Nodes[cntr2].Nodes.Add("Horizontal", "Horizontal: " + ai.Horizontal);
                            tvResults.Nodes[cntr - 1].Nodes[0].Nodes[cntr2].Nodes.Add("Vertical", "Vertical: " + ai.Vertical);
                            tvResults.Nodes[cntr - 1].Nodes[0].Nodes[cntr2].Nodes.Add("Area Type", "Area Type: " + ai.AreaType.ToString());
                            cntr2++;
                        }
                        tvResults.Nodes[cntr - 1].Nodes.Add("Font Info (FI)");
                        tvResults.Nodes[cntr - 1].Nodes[1].NodeFont = ndFont1;
                        cntr2 = 0;
                        foreach (FontInfo fi in blobProcessor.pgConfig.FI)
                        {
                            tvResults.Nodes[cntr - 1].Nodes[1].Nodes.Add("FI[" + cntr.ToString() + "]");
                            tvResults.Nodes[cntr - 1].Nodes[1].Nodes[cntr2].Nodes.Add("FontData", "FontData: " + fi.FontData);
                            tvResults.Nodes[cntr - 1].Nodes[1].Nodes[cntr2].Nodes.Add("XPos", "XPos: " + fi.XPos);
                            tvResults.Nodes[cntr - 1].Nodes[1].Nodes[cntr2].Nodes.Add("YPos", "YPos: " + fi.YPos);
                            tvResults.Nodes[cntr - 1].Nodes[1].Nodes[cntr2].Nodes.Add("LeadingStr", "LeadingStr: " + fi.LeadingStr);
                            tvResults.Nodes[cntr - 1].Nodes[1].Nodes[cntr2].Nodes.Add("TrailingStr", "TrailingStr: " + fi.TrailingStr);
                            tvResults.Nodes[cntr - 1].Nodes[1].Nodes[cntr2].Nodes.Add("RepeatCount", "RepeatCount: " + fi.RepeatCount);
                            cntr2++;
                        }
                        tvResults.Nodes[cntr - 1].Nodes.Add("Config Info (CI)");
                        tvResults.Nodes[cntr - 1].Nodes[2].NodeFont = ndFont1;
                        cntr2 = 0;
                        foreach (ConfigInfo ci in blobProcessor.pgConfig.CI)
                        {
                            tvResults.Nodes[cntr - 1].Nodes[2].Nodes.Add("CI[" + cntr.ToString() + "]");
                            tvResults.Nodes[cntr - 1].Nodes[2].Nodes[cntr2].Nodes.Add("ConfigString", "ConfigString: " + ci.ConfigString);
                            tvResults.Nodes[cntr - 1].Nodes[2].Nodes[cntr2].Nodes.Add("ConfigType", "ConfigType: " + ci.ConfigType.ToString());
                            cntr2++;
                        }
                        tvResults.Nodes[cntr - 1].Nodes.Add("PJL Strings");
                        foreach (string pjlString in blobProcessor.pgConfig.PJL)
                        {
                            tvResults.Nodes[cntr - 1].Nodes[4].Nodes.Add("PJL String: ", pjlString);
                        }
                        tvResults.EndUpdate();
                    }
                }
            }


        }

        private void btnPickBlobFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.DefaultExt = "prn";
            if (openFileDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                txtFile.Text = openFileDialog1.FileName;
            }
        }

        private void btnPickConfig_Click(object sender, EventArgs e)
        {
            openFileDialog1.DefaultExt = "xml";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtConfig.Text = openFileDialog1.FileName;
            }
        }

        private void btnPickAFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.DefaultExt = "csv";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtFontGlyphMap.Text = openFileDialog1.FileName;
            }
        }


    }
}
