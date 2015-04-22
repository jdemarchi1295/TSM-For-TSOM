using System;
using System.Collections.Generic;
using System.Text;

using System.Threading;
using System.ServiceProcess;


namespace TroyPortMonitorService
{
    class ServiceStop
    {
		string serviceName;

		// constructor - passed name of service
		public ServiceStop(string serviceNameArg)
		{
			serviceName = serviceNameArg;
		}

		public void Start()
		{
			// give calling service time to finish starting
			Thread.Sleep (1000);

			// signal service controller to stop service
			ServiceController myController = new ServiceController(serviceName);
			myController.Stop();
		}

    }
}
