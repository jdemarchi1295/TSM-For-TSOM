using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;


namespace TroyPortMonitorService
{
    [RunInstaller(true)]
    public partial class TroyPortMonInstaller : Installer
    {
        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;

        public TroyPortMonInstaller()
        {
            InitializeComponent();
        }



    }
}
