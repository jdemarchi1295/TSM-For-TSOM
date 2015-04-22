using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Troy.PortMonitor.Core.XmlConfiguration;

namespace AddSecurePortUtility
{
    public partial class frmMainForm : Form
    {
        static Dictionary<string, string> troyPorts = new Dictionary<string, string>();
        static Dictionary<string, string> configPaths = new Dictionary<string, string>();
        static Dictionary<string, string> portPath = new Dictionary<string, string>();
        private string filePath;
        private const string AddNewPortString =  "<Add new TROYPORT>";
        private int currentPortNumber = 1;

        public frmMainForm()
        {
            InitializeComponent();
        }

        private void InitForm()
        {
            troyPorts.Clear();
            configPaths.Clear();
            portPath.Clear();
            cboTroyPort.Items.Clear();
            cboTroyPort.Text = "";
            txtPrintPath.Text = "";
            txtConfigPath.Text = "Default";

            string portPathValue;
            ReadMainPortConfig();
            cboTroyPort.Items.Add(AddNewPortString);


            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine;
            Microsoft.Win32.RegistryKey pmKey = registryKey.OpenSubKey
            ("System\\CurrentControlSet\\Control\\Print\\Monitors\\TroySecurePortMonitor\\Ports", false);

            foreach (string portName in pmKey.GetValueNames())
            {
                if (!troyPorts.ContainsValue(portName))
                {
                    cboTroyPort.Items.Add(portName);
                    portPathValue = pmKey.GetValue(portName).ToString();
                    portPath.Add(portName, portPathValue);
                }
            }


        }

        private void frmMainForm_Load(object sender, EventArgs e)
        {

            string copyrightString = "\u00A9 Copyright  TROY Group Inc. 2012";
            lblCopyrightInfo.Text = copyrightString;
            
            InitForm();
        }
        private void ReadMainPortConfig()
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

                string portString,portConfigPath, portMonName, portPath;

                XmlSerializer dser = new XmlSerializer(typeof(PortMonitorServiceConfigurationTsom.TroyPMServiceConfiguration));
                FileStream fs = new FileStream(filePath + "TroyPMServiceConfiguration.xml", FileMode.Open);
                PortMonitorServiceConfigurationTsom.TroyPMServiceConfiguration tpmsc;
                tpmsc = (PortMonitorServiceConfigurationTsom.TroyPMServiceConfiguration)dser.Deserialize(fs);
                fs.Close();

                foreach (PortMonitorServiceConfigurationTsom.Port port in tpmsc.PortList)
                {
                    portString = port.PortName;

                    portMonName = port.PortMonitorName;
                    portConfigPath = port.ConfigurationPath;

                    troyPorts.Add(portString, portMonName);
                    configPaths.Add(portString, portConfigPath);
                }

                string defaultPortName;
                for (int cntr = 1; cntr < 1000; cntr++)
                {
                    defaultPortName = "Troy Secure Port " + cntr.ToString();
                    if (!troyPorts.ContainsKey(defaultPortName))
                    {
                        txtNewPortName.Text = defaultPortName;
                        currentPortNumber = cntr;
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Opening Port Monitor Configuration. Application will close. Path: " + filePath, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                MessageBox.Show(ex.Message, "Exception Message", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

        private void cboTroyPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnMainOK.Enabled = true;
            btnApply.Enabled = true;
            if (cboTroyPort.Text == AddNewPortString)
            {
                AddNewPortMonitor newDialog = new AddNewPortMonitor();
                Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine;
                Microsoft.Win32.RegistryKey pmKey = registryKey.OpenSubKey
                ("System\\CurrentControlSet\\Control\\Print\\Monitors\\TroySecurePortMonitor\\Ports", false);

                string TroyPortName;
                int cntr;
                for (cntr = 1; cntr < 100; cntr++)
                {
                    TroyPortName = "TROYPORT" + cntr.ToString() + ":";
                    if (pmKey.GetValue(TroyPortName) == null)
                    {
                        break;
                    }
                }

                newDialog.nextValue = cntr;
                newDialog.ShowDialog();

                if (newDialog.newPortName != "")
                {
                    InitForm();
                    cboTroyPort.Text = newDialog.newPortName;
                }
                else
                {
                    cboTroyPort.Text = "";
                }

            }
            else
            {
                txtPrintPath.Text = portPath[cboTroyPort.Text];
            }

        }

        private void btnMainCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnMainOK_Click(object sender, EventArgs e)
        {
            if (cboTroyPort.Text == "")
            {
                this.Close();
            }
            else
            {
                if (SaveSettings())
                {
                    this.Close();
                }

            }
        }

        private bool ValidateSettings()
        {
            if (txtNewPortName.Text.Length < 1)
            {
                MessageBox.Show("Invalid port name.  Can not Save changes.");
                return false;
            }
           
            if (!cboTroyPort.Text.StartsWith("TROYPORT"))
            {
                MessageBox.Show("Invalid TROYPORT value.  Can not Save changes.");
                return false;
            }

            if (txtConfigPath.Text.Length < 1)
            {
                MessageBox.Show("Invalid Configuration path value.");
                return false;
            }


            return true;


        }

        private bool SaveSettings()
        {
            string configPath;
            DialogResult retVal;
            retVal = MessageBox.Show("Save secure port settings?", "Are you sure?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (retVal == DialogResult.Yes)
            {
                if (!ValidateSettings())
                {
                    return false;
                }
                if (txtConfigPath.Text.ToUpper() == "DEFAULT")
                {
                 //   string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                 //   string pathFromReg = portPath[cboTroyPort.Text.ToString()];
                 //   configPath = portPath + @"\TROY Group\Port Monitor\" + pathFromReg + @"\Config\";
                    txtConfigPath.Text = "default";
                }
                else
                {
                    if (!txtConfigPath.Text.EndsWith("\\"))
                    {
                        txtConfigPath.Text += "\\";
                    }
                    configPath = txtConfigPath.Text;
                }

                try
                {
                    XmlSerializer dser = new XmlSerializer(typeof(PortMonitorServiceConfigurationTsom.TroyPMServiceConfiguration));
                    FileStream fs = new FileStream(filePath + "TroyPMServiceConfiguration.xml", FileMode.Open);
                    PortMonitorServiceConfigurationTsom.TroyPMServiceConfiguration tpmsc;
                    tpmsc = (PortMonitorServiceConfigurationTsom.TroyPMServiceConfiguration)dser.Deserialize(fs);
                    fs.Close();

                    PortMonitorServiceConfigurationTsom.Port newport = new PortMonitorServiceConfigurationTsom.Port();
                    newport.PortName = txtNewPortName.Text;
                    newport.PortMonitorName = cboTroyPort.Text;
                    newport.ConfigurationPath = txtConfigPath.Text;
                    newport.MonitoredPort = !chkMonitored.Checked;

                    tpmsc.PortList.Add(newport);

                    XmlSerializer xser = new XmlSerializer(typeof(PortMonitorServiceConfigurationTsom.TroyPMServiceConfiguration));
                    TextWriter writer = new StreamWriter(filePath + "TroyPMServiceConfiguration.xml");
                    xser.Serialize(writer, tpmsc);
                    writer.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving new port.  Error: " + ex.Message);
                    return false;
                }

                MessageBox.Show("New secure port was added.");

                InitForm();
                btnApply.Enabled = false;

                return true;
            }
            else if (retVal == DialogResult.Cancel)
            {
                cboTroyPort.Text = "";
                txtPrintPath.Text = "";
                return false;
            }
            else
            {
                MessageBox.Show("New secure port was not added.");
                return true;
            }


        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

  

        private void btnConfigPath_Click(object sender, EventArgs e)
        {
            string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            folderBrowserDialog1.SelectedPath = progFiles + @"\TROY Group\Port Monitor";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtConfigPath.Text = folderBrowserDialog1.SelectedPath;
            }

        }

        private void btnAddMultiple_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Add " + numericUpDown1.Value.ToString() + " ports to the system?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                MessageBox.Show("Ports were not added to system.");
                return;
            }

            XDocument xDoc = XDocument.Load(filePath + "TroyPMServiceConfiguration.xml");

            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine;
            Microsoft.Win32.RegistryKey pmKey = registryKey.OpenSubKey
                ("System\\CurrentControlSet\\Control\\Print\\Monitors\\TroySecurePortMonitor\\Ports", true);

            //string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        

            string newPortName;
            for (int cntr = currentPortNumber; cntr < currentPortNumber + numericUpDown1.Value; cntr++)
            {

                newPortName = "TROYPORT" + cntr + ":";
                string newFilePath;

                //newFilePath = progFiles + @"\TROY Group\Port Monitor\PrintPort" + cntr + @"\";
                newFilePath = filePath + "PrintPort" + cntr + @"\";
                DirectoryInfo dirInfo = new DirectoryInfo(newFilePath);
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }

                DirectoryInfo configDir = new DirectoryInfo(newFilePath + "\\Config\\");
                if (!configDir.Exists)
                {
                    configDir.Create();
                    DirectoryInfo filesCopy = new DirectoryInfo(filePath + @"Configuration\");
                    foreach (FileInfo fInfo in filesCopy.GetFiles())
                    {
                        fInfo.CopyTo(configDir.FullName + fInfo.Name, true);
                    }
                }

                pmKey.SetValue(newPortName, newFilePath);

                xDoc.Element("TroyPortMonitorServiceConfiguration").Add(new XElement("Port",
                                   new XElement("PortName", "Troy Secure Port " + cntr),
                                   new XElement("PortMonitorName", "TROYPORT" + cntr + ":"),
                                   new XElement("ConfigurationPath", "default")));

            }
            pmKey.Close();

            System.ServiceProcess.ServiceController myService = new System.ServiceProcess.ServiceController("Print Spooler");
            if (myService.Status == System.ServiceProcess.ServiceControllerStatus.Running)
            {
                myService.Stop();
            }
            System.Threading.Thread.Sleep(1000);
            myService.Start();

            xDoc.Save(filePath + "TroyPMServiceConfiguration.xml");

            InitForm();

            MessageBox.Show(numericUpDown1.Value.ToString() + " ports have been added to the system.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }



    }
}
