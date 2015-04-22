using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;

namespace TroyPortMonitorService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
#if VIEWER
                    [STAThread]
#endif
        static void Main(string[] args)
        {
#if TESTING
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new Testing.Tester());

#elif VIEWER
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new Blob.BlobViewer() );
#elif DEBUG
            TroyPortMonService service = new TroyPortMonService();
            service.StartService(args);
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new TroyPortMonService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
