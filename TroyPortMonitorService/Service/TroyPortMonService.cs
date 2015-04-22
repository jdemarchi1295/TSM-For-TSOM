using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

//Added for Port Monitor
using System.IO;  //File Watcher
using System.Threading;  //Threading
using System.Windows.Forms; //MessageBox
using System.Xml;
using System.Xml.Serialization;

namespace TroyPortMonitorService
{
    public partial class TroyPortMonService : ServiceBase
    {
 
        public TroyPortMonService()
        {
            InitializeComponent();
        }


        protected override void OnStart(string[] args)
        {
            StartService(args);
        }

        public void StartService(string[] args)
        {
            Setup.Initialization.SetupPorts();
        }

        protected override void OnStop()
        {
            foreach (FileSystemWatcher fileWatcher in Globals.FileWatchers.fileWatchers)
            {
                if (fileWatcher != null)
                {
                    fileWatcher.EnableRaisingEvents = false;
                    fileWatcher.Dispose();
                    ServiceStop serviceStop = new ServiceStop(this.ServiceName);
                }
            }
        }




    }
}
