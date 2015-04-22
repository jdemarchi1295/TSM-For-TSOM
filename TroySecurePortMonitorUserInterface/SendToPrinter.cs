using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Printing;
using System.IO;
using System.Runtime.InteropServices;


namespace TroySecurePortMonitorUserInterface
{



    public partial class SendToPrinter : Form
    {
        public string newPassword = "";

        public SendToPrinter()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnUpdatePrinter_Click(object sender, EventArgs e)
        {
            string uelString = "%-12345X";
		    string pjlEnterLanguage = "@PJL ENTER LANGUAGE=PCL\r\n" + Convert.ToChar(0x1b) + "E";
            string loginString = Convert.ToChar(0x1b) + @"%u5WADMIN" + Convert.ToChar(0x1b)
                                    + @"%p" + txtPrinterLogin.Text.Length.ToString()
                                    + "W" + txtPrinterLogin.Text + Convert.ToChar(0x1b)
                                    + @"%u1S";
            string pDLEnter = Convert.ToChar(0x1b) + @"%v1D";
            string decryptMode;
            if (radJobByJob.Checked)
            {
                decryptMode = Convert.ToChar(0x1b) + @"%c0M";
            }
            else if (radAlways.Checked)
            {
                decryptMode = Convert.ToChar(0x1b) + @"%c1M";
            }
            else
            {
                decryptMode = "";
            }
            string decryptPassword = Convert.ToChar(0x1b) + @"%c" + newPassword.Length.ToString()
                                           + "W" + newPassword;
            string pDLExit = Convert.ToChar(0x1b) + @"%v0D";
            string logOut = Convert.ToChar(0x1b) + @"%u0S";
            string jobEnd = Convert.ToChar(0x1b) + "E" + uelString;

            string finalString = uelString + pjlEnterLanguage + loginString + pDLEnter
                                    + decryptMode + decryptPassword + pDLExit + logOut + jobEnd;

            byte[] toSend = new UTF8Encoding(true).GetBytes(finalString);

            int size = Marshal.SizeOf(toSend[0]) * toSend.Length;
            IntPtr pnt = Marshal.AllocHGlobal(size);
            Marshal.Copy(toSend, 0, pnt, toSend.Length);


            //PrintToSpooler.SendFileToPrinter(printToPrinterName, printerFileName, "Test From Port Monitor");
            PrintToSpooler.SendBytesToPrinter(cboPrinter.Text, pnt, toSend.Length, "Printer Update");

            Marshal.FreeHGlobal(pnt);

        }

        private void SendToPrinter_Load(object sender, EventArgs e)
        {
            cboPrinter.Items.Clear();

            foreach (string strPrinter in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                cboPrinter.Items.Add(strPrinter);
            }

        }

        private void txtPrinterLogin_TextChanged(object sender, EventArgs e)
        {
            if ((cboPrinter.Text.Length > 0) && (txtPrinterLogin.Text.Length > 0) &&
                (txtPrinterLogin.Text == txtConfirm.Text))
            {
                btnUpdatePrinter.Enabled = true;
            }
            else
            {
                btnUpdatePrinter.Enabled = false;
            }
        }

        private void txtConfirm_TextChanged(object sender, EventArgs e)
        {
            if ((cboPrinter.Text.Length > 0) && (txtConfirm.Text.Length > 0) &&
                (txtPrinterLogin.Text == txtConfirm.Text))
            {
                btnUpdatePrinter.Enabled = true;
            }
            else
            {
                btnUpdatePrinter.Enabled = false;
            }

        }

        private void cboPrinter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((cboPrinter.Text.Length > 0) && (txtConfirm.Text.Length > 0) &&
                (txtPrinterLogin.Text == txtConfirm.Text))
            {
                btnUpdatePrinter.Enabled = true;
            }
            else
            {
                btnUpdatePrinter.Enabled = false;
            }

        }

       



    }


 }
