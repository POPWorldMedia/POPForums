using System;
using System.Web;
using Ninject;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.Web
{
	public class PopForumsLoggingModule : IHttpModule
	{
		public void Init(HttpApplication application)
		{
			application.Error += ErrorHandler;
			application.BeginRequest += BeginRequest;
			SetupLogging();
			lock (_syncRoot)
				_moduleInstanceCount++;
		}

		private void BeginRequest(object sender, EventArgs e)
		{
			if (!_isInitialized)
			{
				SetupLogging();
			}
		}

		private void SetupLogging()
		{
			lock (_syncRoot)
			{
				try
				{
					Kernel = PopForumsActivation.Kernel;
					SettingsManager = Kernel.Get<ISettingsManager>();
					ErrorLog = Kernel.Get<IErrorLog>();
					var setupService = Kernel.Get<ISetupService>();
					if (!setupService.IsDatabaseSetup())
						return;
					_isInitialized = true;
				}
				catch (Exception exc)
				{
					throw new ErrorLogException("PopForums couldn't initialize an instance of PopForumsServiceModule: " + exc.Message);
				}
			}
		}

		private readonly static object _syncRoot = new object();
		private static int _moduleInstanceCount;
		private static bool _isInitialized;

		private void ErrorHandler(object sender, EventArgs e)
		{
			var exception = HttpContext.Current.Server.GetLastError().GetBaseException();
			if (exception is ErrorLogException)
				throw exception;
			if (SettingsManager.IsLoaded() && SettingsManager.Current.LogErrors)
			{
				if (exception.Message.IndexOf("The file") == -1 && exception.Message.IndexOf("does not exist") == -1 && exception.Message.IndexOf("not found") == -1)
					ErrorLog.Log(exception, ErrorSeverity.Error);
			}
			else
			{
				throw new ErrorLogException("PopForums encountered the following error prior to settings being initialized: " + exception.Message);
			}
		}

		public IKernel Kernel { get; private set; }
		public ISettingsManager SettingsManager { get; private set; }
		public IErrorLog ErrorLog { get; private set; }
		public static int ModuleInstanceCount
		{
			get { return _moduleInstanceCount; }
		}

		public void Dispose()
		{
			lock (_syncRoot)
			{
				_moduleInstanceCount--;
			}
		}
	}
}