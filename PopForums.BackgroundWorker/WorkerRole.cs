using System.Diagnostics;
using System.Net;
using Microsoft.WindowsAzure.ServiceRuntime;
using PopForums.Data.Azure;
using PopForums.Web;

namespace PopForums.BackgroundWorker
{
	public class WorkerRole : RoleEntryPoint
	{
		public override void Run()
		{
			Trace.TraceInformation("PopForums.BackgroundWorker entry point called");
			StartServices();
			while (true)
			{
				Trace.TraceInformation("Working");
			}
		}

		private void StartServices()
		{
			PopForumsActivation.InitializeOutOfWeb();
			PopForumsActivation.Container.Configure(x => x.AddRegistry(new AzureInjectionRegistry()));
			PopForumsActivation.StartServicesOutOfWeb();
		}

		public override bool OnStart()
		{
			// Set the maximum number of concurrent connections 
			ServicePointManager.DefaultConnectionLimit = 12;

			// For information on handling configuration changes
			// see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

			return base.OnStart();
		}
	}
}