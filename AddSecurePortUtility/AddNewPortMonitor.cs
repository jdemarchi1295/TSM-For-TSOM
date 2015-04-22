using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace AddSecurePortUtility
{
    public partial class AddNewPortMonitor : Form
    {
        public int nextValue;
        public string newPortName;

        public AddNewPortMonitor()
        {
            InitializeComponent();
        }

        private void AddNewPortMonitor_Load(object sender, EventArgs e)
        {
            numericUpDown1.Minimum = nextValue;
            numericUpDown1.Value = nextValue;

        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine;
            Microsoft.Win32.RegistryKey pmKey = registryKey.OpenSubKey
                ("System\\CurrentControlSet\\Control\\Print\\Monitors\\TroySecurePortMonitor\\Ports", true);


            Microsoft.Win32.RegistryKey mainKey = registryKey.OpenSubKey
                ("System\\CurrentControlSet\\Control\\Print\\Monitors\\TroySecurePortMonitor", true);

            string progFiles = mainKey.GetValue("MainConfigurationPath").ToString();

            //string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            newPortName = "TROYPORT" + numericUpDown1.Value.ToString() + ":";
            string filePath;
            if (txtPath.Text.ToUpper() == "DEFAULT")
            {
                filePath = progFiles + @"PrintPort" + numericUpDown1.Value.ToString() + @"\";
                DirectoryInfo dirInfo = new DirectoryInfo(filePath);
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }

                DirectoryInfo configDir = new DirectoryInfo(filePath + "\\Config\\");
                if (!configDir.Exists)
                {
                    configDir.Create();
                    DirectoryInfo filesCopy = new DirectoryInfo(progFiles + @"Configuration\");
                    foreach (FileInfo fInfo in filesCopy.GetFiles())
                    {
                        fInfo.CopyTo(configDir.FullName + fInfo.Name, true);
                    }
                }

            }
            else if (txtPath.Text.Length < 1)
            {
                MessageBox.Show("Path name must be set to valid path.");
                return;
            }
            else
            {
                if (!(txtPath.Text.EndsWith("\\")))
                {
                    filePath = txtPath.Text + "\\";
                }
                else
                {
                    filePath = txtPath.Text;
                }
            }

            if (numericUpDown1.Value < 1)
            {
                MessageBox.Show("Invalid Port Number.");
                return;
            }

            pmKey.SetValue(newPortName, filePath);

            pmKey.Close();



            System.ServiceProcess.ServiceController myService = new System.ServiceProcess.ServiceController("Print Spooler");
            if (myService.Status == System.ServiceProcess.ServiceControllerStatus.Running)
            {
                myService.Stop();
            }
            System.Threading.Thread.Sleep(1000);
            myService.Start();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            folderBrowserDialog1.SelectedPath = progFiles + @"\TROY Group\Port Monitor";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = folderBrowserDialog1.SelectedPath;   
            }

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            newPortName = "";
            this.Close();
        }
    }
}
