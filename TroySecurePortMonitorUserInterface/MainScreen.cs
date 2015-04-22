using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using System.Printing;
using Troy.PortMonitor.Core.XmlConfiguration;


namespace TroySecurePortMonitorUserInterface
{
    public partial class MainScreen : Form
    {

        static Dictionary<string,string> portToPath = new Dictionary<string,string>();

        // WARNING: Do not change this password string without also changing the password in the code where decryption occurs
        //          The password used for decryption and encryption must match.
        static byte[] salt = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        static string password = "s82'*4'Kng4#3LS01$e1gf+2";
        byte[] globalPw = new UTF8Encoding(true).GetBytes(password);  //Byte representation of the password

        private string filePath = "<Not loaded>";
        private string encryptPasswordFromXml;

        private const int bit0EnablePantograph = 1;
        private const int bit1EnablePantograph = 2;
        private const int bit2MicroPrintBorder = 4;
        private const int bit3MicroPrintBorder = 8;
        private const int bit4WarningBox = 16;
        private const int bit5SigLine = 32;
        private const int bit6SigLine = 64;
        private const int bit7BackOfPage = 128;
        private const int bit8Interference = 256;

        private bool possibleChange = false;

        private string currentPortName;

        List<int> ppList = new List<int>();


        //Added 
        private PortMonitorConfigurationTsom currTpmc;
        private PortMonitorConfigurationTsom holdTpmc;

        private string LegalStr = "/e*p8200Y/f";
        private string A4Str = "/e*p6815Y/f";
        private string LetterStr = "/e*p6400Y/f";

        private string PortMonConfigTsomFileName = "PortMonitorConfigurationTsom.xml";

        //private bool SaveNeeded = false;
        //private bool ScSaveNeeded = false;
        //private bool ErSaveNeeded = false;

        public MainScreen()
        {
            InitializeComponent();
        }

        private void MainScreen_Load(object sender, EventArgs e)
        {
            
            lblCopyright.Text = "\u00A9 Copyright  TROY Group Inc.  2012  Version 1.2";

            LoadPortMonitorList();

        }

        private bool LoadPortMonitorList()
        {
            try
            {
                Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine;
                Microsoft.Win32.RegistryKey pmKey = registryKey.OpenSubKey
                    ("System\\CurrentControlSet\\Control\\Print\\Monitors\\TroySecurePortMonitor", false);

                filePath = pmKey.GetValue("MainConfigurationPath").ToString();
                if ((filePath.Length > 0) && (!filePath.EndsWith("\\")))
                {
                    filePath += "\\";
                }
                XmlSerializer dser = new XmlSerializer(typeof(PortMonitorServiceConfigurationTsom.TroyPMServiceConfiguration));
                FileStream fs = new FileStream(filePath + "TroyPMServiceConfiguration.xml", FileMode.Open);
                PortMonitorServiceConfigurationTsom.TroyPMServiceConfiguration tpmsc;
                tpmsc = (PortMonitorServiceConfigurationTsom.TroyPMServiceConfiguration)dser.Deserialize(fs);

                Microsoft.Win32.RegistryKey pmPort;

                string portString, portConfigPath, portMonName, portPath;
                foreach (PortMonitorServiceConfigurationTsom.Port port in tpmsc.PortList)
                {
                    if (port.MonitoredPort)
                    {
                        portString = port.PortName;
                        cboTroySecurePortMonitor.Items.Add(portString);

                        portConfigPath = port.ConfigurationPath;
                        if (portConfigPath.ToUpper() == "DEFAULT")
                        {
                            portMonName = port.PortMonitorName;
                            pmPort = pmKey.OpenSubKey("Ports", false);
                            portPath = pmPort.GetValue(portMonName).ToString();
                            portConfigPath = portPath + "Config\\";
                        }
                        else
                        {
                            if ((portConfigPath.Length > 0) && (!portConfigPath.EndsWith("\\")))
                            {
                                portConfigPath += "\\";
                            }
                        }
                        portToPath.Add(portString, portConfigPath);
                    }
                }
                fs.Close();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Opening Port Monitor Configuration. Application will close. Path: " + filePath, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                MessageBox.Show(ex.Message, "Exception Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;

            }
        }

        private bool ReadConfigurationFromXml()
        {
            try
            {
                XmlSerializer dser = new XmlSerializer(typeof(PortMonitorConfigurationTsom));
                string filename = filePath + PortMonConfigTsomFileName;
                FileStream fs = new FileStream(filename, FileMode.Open);
                currTpmc = (PortMonitorConfigurationTsom)dser.Deserialize(fs);
                holdTpmc = currTpmc;
                fs.Close();

                cboDefaultPrinter.Text = currTpmc.DefaultPrinter;

                //CONFIGURATION TAB
                cboFont.Text = currTpmc.TsomSerializationDataFont;
                numDelay.Value = Convert.ToDecimal(currTpmc.FileReadDelay_ms);
                numAttempts.Value = Convert.ToDecimal(currTpmc.ReadAttempts);
                if (currTpmc.EnableDuplex == "0")
                {
                    radSimplex.Checked = true;
                }
                else if (currTpmc.EnableDuplex == "1")
                {
                    radLongEdgeBound.Checked = true;
                }
                else if (currTpmc.EnableDuplex == "2")
                {
                    radShortEdgeBound.Checked = true;
                }
                else
                {
                    radDoNotAdjust.Checked = true;
                }
                txtInsertPoint.Text = currTpmc.InsertPointPcl;
                lstEndOfPage.Items.Clear();
                foreach (string str in currTpmc.EndOfPagePcl)
                {
                    lstEndOfPage.Items.Add(str);
                }
                if (lstEndOfPage.Items.Contains(LetterStr))
                {
                    chkLetter.Checked = true;
                }
                if (lstEndOfPage.Items.Contains(LegalStr))
                {
                    chkLegal.Checked = true;
                }
                if (lstEndOfPage.Items.Contains(A4Str))
                {
                    chkA4.Checked = true;
                }


                //SECURITY TAB
                txtPrinterPin.Text = currTpmc.PrinterPin;
                txtMicrPin.Text = currTpmc.MicrPin;
                txtJobName.Text = currTpmc.TroyJobName;
                txtJobPin.Text = currTpmc.TroyJobPin;
                txtHpPassword.Text = currTpmc.HpJobPassword;
                txtAccountUser.Text = currTpmc.AccountUserName;
                txtAccountPw.Text = currTpmc.AccountPassword;
                txtAltEsc.Text = currTpmc.AlternateEscCharacter;
                chkAutoPageRotate.Checked = currTpmc.AutoPageRotate;
                chkEnableMicr.Checked = currTpmc.EnableMicrMode;
                chkDefaultPaperTrayMapping.Checked = currTpmc.DefaultPaperTrayMapping;
                if (currTpmc.EncryptionType.ToUpper() != "NONE")
                {
                    chkEnableEncryption.Checked = true;
                }
                else
                {
                    chkEnableEncryption.Checked = false;
                }
                if (currTpmc.EncryptPassword != "")
                {
                    DecryptPassword(currTpmc.EncryptPassword);
                    //txtEncryptMainPassword.Text = encryptPasswordFromXml;
                }
                else
                {
                    encryptPasswordFromXml = "";
                    //txtEncryptMainPassword.Text = "";
                }

                //ERROR LOGGING SCREEN
                txtSaveDataFiles.Text = currTpmc.DebugBackupFilesPath;
                txtErrorLogPath.Text = currTpmc.ErrorLogPath;
                chkLogErrorToEventLog.Checked = currTpmc.LogErrorsToEventLog;
                chkEnableErrorMsgBox.Checked= currTpmc.EnableErrorMessageBoxes;
                chkLogErrorDefaultPrinter.Checked = currTpmc.LogErrorDefaultPrinterUsed;
                chkLogErrorNoSerialized.Checked = currTpmc.LogErrorSerializedDataNotFound;
                chkLogWarningMsg.Checked = currTpmc.LogWarningMessages;
                chkFatalErrorNoDefault.Checked = currTpmc.FatalErrorIfNotDefaultPrinter;
                gbErrorLogging.Enabled = true;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Opening Port Monitor Configuration. Application will close. Path: " + filePath, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                MessageBox.Show(ex.Message, "Exception Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;

            }

        }

        private bool EncryptPassword(string pw, ref string encRet)
        {
            try
            {
                byte[] pwBytes = new UTF8Encoding(true).GetBytes(pw);
                int testPwLength8Byte = pwBytes.Length / 8;
                if ((pwBytes.Length % 8) > 0)
                {
                    testPwLength8Byte++;
                }
                int encPwLength = testPwLength8Byte * 8;
                byte[] encPw = new byte[encPwLength];
                TripleDESCryptoServiceProvider tdesPw = new TripleDESCryptoServiceProvider();
                tdesPw.BlockSize = 64;  //8 byte block size
                tdesPw.Padding = PaddingMode.Zeros;

                MemoryStream eStream = new MemoryStream(encPw);
                CryptoStream encStreamPw = new CryptoStream(eStream, tdesPw.CreateEncryptor(globalPw, salt), CryptoStreamMode.Write);
                encStreamPw.Write(pwBytes, 0, pwBytes.Length);
                encStreamPw.FlushFinalBlock();

                string encPwStr = Convert.ToBase64String(encPw);

                encRet = encPwStr;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error encrypting password. " + ex.Message);
                return false;
            }
        }

        private bool DecryptPassword(string pw)
        {
            try
            {
                byte[] testBytes = Convert.FromBase64String(pw);

                TripleDESCryptoServiceProvider tdesPw = new TripleDESCryptoServiceProvider();
                tdesPw.BlockSize = 64;  //8 byte block size
                tdesPw.Padding = PaddingMode.Zeros;
                byte[] tdesKey = new byte[64];
                MemoryStream aStream = new MemoryStream(tdesKey);
                CryptoStream decStreamPw = new CryptoStream(aStream, tdesPw.CreateDecryptor(globalPw, salt), CryptoStreamMode.Write);
                decStreamPw.Write(testBytes, 0, testBytes.Length);
                decStreamPw.FlushFinalBlock();
                string tempStr = new UTF8Encoding(true).GetString(tdesKey);
                int loc = tempStr.IndexOf('\0');
                encryptPasswordFromXml = tempStr.Substring(0, loc);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error decrypting password. " + ex.Message);
                return false;
            }
        }





        private void chkEnableEncryption_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEnableEncryption.Checked)
            {
                DialogResult retVal;
                retVal = MessageBox.Show("Are you sure you want to enable Encryption", "Are you sure?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (retVal == DialogResult.Yes)
                {

                }
                else
                {
                    chkEnableEncryption.Checked = false;
                }
            }
            SetEncryptionControls();
            
        }

        private void SetEncryptionControls()
        {
            if (chkEnableEncryption.Checked)
            {
                if (encryptPasswordFromXml.Length < 1)
                {
                    lblEncryptPw.Enabled = true;
                    lblEncryptPw2.Enabled = true;
                    txtEncryptMainPassword.Enabled = true;
                    txtEncryptValidatePassword.Enabled = true;
                }
                else
                {
                    lblCurrentPassword.Enabled = true;
                    txtCurrentPassword.Enabled = true;
                }
            }
            else
            {
                lblCurrentPassword.Enabled = false;
                txtCurrentPassword.Enabled = false;
                lblEncryptPw.Enabled = false;
                lblEncryptPw2.Enabled = false;
                txtEncryptMainPassword.Enabled = false;
                txtEncryptValidatePassword.Enabled = false;

            }

        }

        private void btnSaveDataFilesPath_Click(object sender, EventArgs e)
        {
            if (folderErrorLogPath.ShowDialog() == DialogResult.OK)
            {
                txtSaveDataFiles.Text = folderErrorLogPath.SelectedPath;
            }

        }

        private bool ValidateValues()
        {

            if ((txtAccountUser.Text == "") && (txtAccountPw.Text != ""))
            {
                tabMain.SelectTab("Security");
                MessageBox.Show("Error! Account Password can only be set if an Account User Name is set.");
                return false;
            }
            if (((txtJobName.Text != "") && (txtJobPin.Text == "")) || ((txtJobName.Text == "") && (txtJobPin.Text != "")))
            {
                tabMain.SelectTab("Security");
                MessageBox.Show("Error! Job name and job pin must both be set to a value.");
                return false;
            }


            if (chkEnableEncryption.Checked)
            {
                if (txtEncryptMainPassword.Text != txtEncryptValidatePassword.Text)
                {
                    tabMain.SelectTab("Security");
                    MessageBox.Show("Error! Encryption New Password and Confirm Password do not match.");
                    return false;

                }
            }

            if (lstEndOfPage.Items.Count < 1)
            {
                tabMain.SelectTab("Configuration");
                MessageBox.Show("Error! End of Page list on Configuration tab can not be empty.  Select Letter, Legal or A4 if unsure what value to enter.");
                return false;
            }

            if (txtInsertPoint.Text == "")
            {
                tabMain.SelectTab("Configuration");
                MessageBox.Show("Error! Insert point setting in the Configuration tab can not be empty.  Enter /e*p0x0Y if unsure what value to enter.");
                return false;
            }

            if ((chkFatalErrorNoDefault.Checked) && (cboDefaultPrinter.Text == ""))
            {
                tabMain.SelectTab("Configuration");
                MessageBox.Show("Error! Fatal Error If No Default Printer is checked in the Error Logging tab and no default printer is defined in the Configuration tab.");
                return false;
            }

            return true;
        }


        private bool SaveSettingsToXml()
        {
            string configFileName = "";
            try
            {
                configFileName = filePath + PortMonConfigTsomFileName;
                FileInfo fileInfo = new FileInfo(configFileName);
                if (!fileInfo.Exists)
                {
                    Exception tmpex = new Exception("Port Monitor Configuration file does not exist.");
                    throw tmpex;
                }

                XmlSerializer xser = new XmlSerializer(typeof(PortMonitorConfigurationTsom));
                TextWriter writer = new StreamWriter(configFileName);
                xser.Serialize(writer, currTpmc);
                writer.Close();

                holdTpmc = currTpmc;

                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving configuration. " + ex.Message);
                return false;
            }

        }

        private void txtCurrentPassword_TextChanged(object sender, EventArgs e)
        {
            if (txtCurrentPassword.Text == encryptPasswordFromXml)
            {
                lblEncryptPw.Enabled = true;
                lblEncryptPw2.Enabled = true;
                txtEncryptMainPassword.Enabled = true;
                txtEncryptValidatePassword.Enabled = true;
            }
            else
            {
                lblEncryptPw.Enabled = false;
                lblEncryptPw2.Enabled = false;
                txtEncryptMainPassword.Enabled = false;
                txtEncryptValidatePassword.Enabled = false;
            }

        }

        private void btnUpdatePrinter_Click(object sender, EventArgs e)
        {
            SendToPrinter sendToPrinter = new SendToPrinter();
            sendToPrinter.newPassword = txtEncryptMainPassword.Text;
            sendToPrinter.ShowDialog();

        }

      

        private void btnApply_Click_1(object sender, EventArgs e)
        {
            SaveSettings(false);
        }

        private void btnMainCancel_Click_1(object sender, EventArgs e)
        {
            currTpmc = holdTpmc;
            this.Close();
        }

        private void cboTroySecurePortMonitor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentPortName == cboTroySecurePortMonitor.Text)
            {
                return;
            }
            if ((possibleChange) && (currentPortName != ""))
            {
                if (MessageBox.Show("Warning.  Changes made to the current Secure Port will be lost if another port is selected.  Select Save before switching Ports.  Continute to switch Ports?", "Warning", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    cboTroySecurePortMonitor.Text = currentPortName;
                    return;
                }
            }
            possibleChange = false;
            currentPortName = cboTroySecurePortMonitor.Text;

            tabMain.Enabled = true;
            btnMainOK.Enabled = true;
            btnApply.Enabled = true;


            filePath = portToPath[cboTroySecurePortMonitor.Text];
            if ((filePath == null) || (filePath == ""))
            {
                MessageBox.Show("Can not find file path associated with the select secure port.");
            }
            else
            {
                if (!ReadConfigurationFromXml())
                {
                    //this.Close();
                    MessageBox.Show("Error opening the configuration for the port.");

                }
                else
                {
                    cboDefaultPrinter.Items.Clear();
                    LocalPrintServer ps = new LocalPrintServer(PrintSystemDesiredAccess.AdministrateServer);
                    ps.Refresh();
                    PrintQueue pq;
                    foreach (string strPrinter in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                    {
                        pq = ps.GetPrintQueue(strPrinter);
                        pq.Refresh();
                        if (pq.QueuePort.Name.Contains("TROYPORT"))
                        {
                            //It's a TroyPort so skip this one
                        }
                        else
                        {
                            cboDefaultPrinter.Items.Add(strPrinter);
                        }
                    }
                    SetEncryptionControls();
                }
            }
        
        }


        private void btnErrorLogPath_Click(object sender, EventArgs e)
        {
            if (folderErrorLogPath.ShowDialog() == DialogResult.OK)
            {
                txtErrorLogPath.Text = folderErrorLogPath.SelectedPath;
            }
        }

        private void btnSaveDataFilesPath_Click_1(object sender, EventArgs e)
        {
            if (folderErrorLogPath.ShowDialog() == DialogResult.OK)
            {
                txtSaveDataFiles.Text = folderErrorLogPath.SelectedPath;
            }

        }

        private bool SaveSettings(bool OkSelected)
        {

            bool retCancel = false;
         
            DialogResult retVal;
            if (OkSelected)
            {
                retVal = MessageBox.Show("Save configuration settings before exiting?", "Are you sure?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            }
            else
            {
                retVal = MessageBox.Show("Save configuration settings?  Changes will take effect immediately.", "Are you sure?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            }
            
            if (retVal == DialogResult.Yes)
            {
                if (ValidateValues())
                {
                    SetNewValues();
                    if (SaveSettingsToXml())
                    {
                        MessageBox.Show("Configuration settings saved.", "Save Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Error occurred while attempting to save changes.  Values not saved.", "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Values were not saved due to errors.", "Values Not Saved", MessageBoxButtons.OK);
                    retCancel = true;
                }
            }
            else if (retVal == DialogResult.Cancel)
            {
                retCancel = true;
            }
            else
            {
                MessageBox.Show("Settings were not saved.", "Save Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                retCancel = true;
            }

            if (!retCancel)
            {
                possibleChange = false;
                this.Cursor = Cursors.WaitCursor;
                System.Threading.Thread.Sleep(500);
                System.ServiceProcess.ServiceController myService = new System.ServiceProcess.ServiceController("Troy Port Monitor Service");
                if (myService.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                {
                    myService.Stop();
                }
                System.Threading.Thread.Sleep(2000);
                myService.Start();
                this.Cursor = Cursors.Arrow;

            }

            return retCancel;

        }

        private void btnMainOK_Click(object sender, EventArgs e)
        {
            if (!SaveSettings(true))
            {
                

                this.Close();
            }
        }

        private void txtEncryptValidatePassword_TextChanged(object sender, EventArgs e)
        {
            if ((txtEncryptValidatePassword.Text == txtEncryptMainPassword.Text) &&
                (txtEncryptValidatePassword.Text.Length > 0))
            {
                btnUpdatePrinter.Enabled = true;
            }
            else
            {
                btnUpdatePrinter.Enabled = false;
            }

        }

        private void txtEncryptMainPassword_TextChanged(object sender, EventArgs e)
        {
            if ((txtEncryptValidatePassword.Text == txtEncryptMainPassword.Text) &&
                (txtEncryptMainPassword.Text.Length > 0))
            {
                btnUpdatePrinter.Enabled = true;
            }
            else
            {
                btnUpdatePrinter.Enabled = false;
            }

        }



        private void cboTroySecurePortMonitor_Leave(object sender, EventArgs e)
        {
            if (cboTroySecurePortMonitor.Text != "")
            {
                currentPortName = cboTroySecurePortMonitor.Text;
                possibleChange = true;
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            SplashForm splash = new SplashForm();
            splash.ShowDialog();
        }

        private void chkLetter_CheckedChanged(object sender, EventArgs e)
        {

            if (lstEndOfPage.Items.Contains(LetterStr))
            {
                if (chkLetter.Checked == false)
                {
                    lstEndOfPage.Items.Remove(LetterStr);
                }
            }
            else
            {
                if (chkLetter.Checked)
                {
                    lstEndOfPage.Items.Add(LetterStr);
                }
            }

        }
        private void chkLegal_CheckedChanged(object sender, EventArgs e)
        {
            

            if (lstEndOfPage.Items.Contains(LegalStr))
            {
                if (chkLegal.Checked == false)
                {
                    lstEndOfPage.Items.Remove(LegalStr);
                }
            }
            else
            {
                if (chkLegal.Checked)
                {
                    lstEndOfPage.Items.Add(LegalStr);
                }
            }

        }

        private void chkA4_CheckedChanged(object sender, EventArgs e)
        {

            if (lstEndOfPage.Items.Contains(A4Str))
            {
                if (chkA4.Checked == false)
                {
                    lstEndOfPage.Items.Remove(A4Str);
                }
            }
            else
            {
                if (chkA4.Checked)
                {
                    lstEndOfPage.Items.Add(A4Str);
                }
            }

        }


        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (txtNewString.Text != "")
            {
                if (!lstEndOfPage.Items.Contains(txtNewString.Text))
                {
                    lstEndOfPage.Items.Add(txtNewString.Text);
                }

            }
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            if (lstEndOfPage.SelectedIndex > -1)
            {
                lstEndOfPage.Items.RemoveAt(lstEndOfPage.SelectedIndex);
            }

        }

        private void SetNewValues()
        {
            //CONFIGURATION TAB
            currTpmc.TsomSerializationDataFont = cboFont.Text;
            currTpmc.FileReadDelay_ms = Convert.ToInt32(numDelay.Value);
            currTpmc.ReadAttempts = Convert.ToInt32(numAttempts.Value);
            currTpmc.DefaultPrinter = cboDefaultPrinter.Text;
            if (radSimplex.Checked == true)
            {
                currTpmc.EnableDuplex = "0";
            }
            else if (radLongEdgeBound.Checked == true)
            {
                currTpmc.EnableDuplex = "1";
            }
            else if (radShortEdgeBound.Checked == true)
            {
                currTpmc.EnableDuplex = "2";
            }
            else
            {
                currTpmc.EnableDuplex = "";
            }
            currTpmc.InsertPointPcl = txtInsertPoint.Text;
            currTpmc.EndOfPagePcl.Clear();
            foreach (string str in lstEndOfPage.Items)
            {
                currTpmc.EndOfPagePcl.Add(str);
            }

            //SECURITY TAB
            currTpmc.PrinterPin = txtPrinterPin.Text;
            currTpmc.MicrPin = txtMicrPin.Text;
            currTpmc.TroyJobName = txtJobName.Text;
            currTpmc.TroyJobPin = txtJobPin.Text;
            currTpmc.HpJobPassword = txtHpPassword.Text;
            currTpmc.AccountUserName = txtAccountUser.Text;
            currTpmc.AccountPassword = txtAccountPw.Text;
            currTpmc.AlternateEscCharacter = txtAltEsc.Text;
            currTpmc.AutoPageRotate = chkAutoPageRotate.Checked;
            currTpmc.EnableMicrMode = chkEnableMicr.Checked;
            currTpmc.DefaultPaperTrayMapping = chkDefaultPaperTrayMapping.Checked;
            if (chkEnableEncryption.Checked == true)
            {
                currTpmc.EncryptionType = "TDES";
                if ((txtEncryptMainPassword.Text != "") && (txtCurrentPassword.Text != txtEncryptMainPassword.Text))
                {
                    string retStr = "";
                    EncryptPassword(txtEncryptMainPassword.Text, ref retStr);
                    currTpmc.EncryptPassword = retStr;
                }
            }
            else
            {
                currTpmc.EncryptionType = "None";
                currTpmc.EncryptPassword = "";
            }

            //ERROR LOGGING SCREEN
            currTpmc.DebugBackupFilesPath = txtSaveDataFiles.Text;
            currTpmc.ErrorLogPath = txtErrorLogPath.Text;
            currTpmc.LogErrorsToEventLog = chkLogErrorToEventLog.Checked;
            currTpmc.EnableErrorMessageBoxes = chkEnableErrorMsgBox.Checked;
            currTpmc.LogErrorDefaultPrinterUsed = chkLogErrorDefaultPrinter.Checked;
            currTpmc.LogErrorSerializedDataNotFound = chkLogErrorNoSerialized.Checked;
            currTpmc.LogWarningMessages = chkLogWarningMsg.Checked;
            currTpmc.FatalErrorIfNotDefaultPrinter = chkFatalErrorNoDefault.Checked;
        }

        private void txtCurrentPassword_TextChanged_1(object sender, EventArgs e)
        {
            if (txtCurrentPassword.Text == encryptPasswordFromXml)
            {
                lblEncryptPw.Enabled = true;
                lblEncryptPw2.Enabled = true;
                txtEncryptMainPassword.Enabled = true;
                txtEncryptValidatePassword.Enabled = true;
            }
            else
            {
                lblEncryptPw.Enabled = false;
                lblEncryptPw2.Enabled = false;
                txtEncryptMainPassword.Enabled = false;
                txtEncryptValidatePassword.Enabled = false;
            }

        }
       

    }
}
