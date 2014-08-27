using System;
using System.Diagnostics;
using System.Net;
using Microsoft.Owin.Hosting;
using Microsoft.WindowsAzure.ServiceRuntime;
using PopForums.Data.Azure;
using PopForums.Web;

namespace PopForums.BackgroundWorker
{
	public class WorkerRole : RoleEntryPoint
	{
		private IDisposable _app = null;

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
			ServicePointManager.DefaultConnectionLimit = 12;
			var endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Monitoring"];
			var baseUri = String.Format("{0}://{1}", endpoint.Protocol, endpoint.IPEndpoint);
			Trace.TraceInformation(String.Format("Starting OWIN at {0}", baseUri), "Information");
			_app = WebApp.Start<Startup>(new StartOptions(baseUri));
			return base.OnStart();
		}

		public override void OnStop()
		{
			if (_app != null)
				_app.Dispose();
			base.OnStop();
		}
	}
}