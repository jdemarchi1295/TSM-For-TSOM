using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TroyPortMonitorService.Testing
{
    public partial class Tester : Form
    {
        public Tester()
        {
            InitializeComponent();
        }

        private void btnInit_Click(object sender, EventArgs e)
        {
            if (Setup.Initialization.SetupPorts())
            {
                txtStatus.Text = "Initialization Complete";
            }
            else
            {
                txtStatus.Text = "Initialization FAILED!";
            }
        }
    }
}
