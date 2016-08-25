using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Configuration;

namespace PopForums.Services
{
	public abstract class ApplicationServiceBase
	{
		protected abstract void ServiceAction();
		protected abstract int GetInterval();

		protected ApplicationServiceBase()
		{
			Name = GetType().FullName;
			LastExecutionTime = null;
			IsRunning = false;
			ExceptionMessage = String.Empty;
			_errorCount = 0;
			Interval = 1;
		}

		public virtual void Start(IServiceProvider serviceProvider)
		{
			ErrorLog = serviceProvider.GetService<IErrorLog>();
			IsRunning = true;
			TimerCallback callback = Execute;
			Timer = new Timer(callback, this, Interval, Interval);
		}

		public IErrorLog ErrorLog { get; private set; }

		public void Execute(object sender)
		{
			var interval = GetInterval();
			if (interval != Interval)
			{
				Interval = interval;
				Timer.Change(0, interval);
			}
			if (IsRunning)
			{
				var isError = false;
				try
				{
					LastExecutionTime = DateTime.UtcNow;
					ServiceAction();
				}
				catch (Exception exc)
				{
					isError = true;
					_errorCount++;
					ErrorLog.Log(exc, ErrorSeverity.Error);
					ExceptionMessage = exc.Message + "\r\n" + exc.StackTrace;
				}
				if (isError && _errorCount >= 10)
					Stop();
				else if (!isError)
				{
					_errorCount = 0;
					ExceptionMessage = String.Empty;
				}
			}
		}

		public void Stop()
		{
			IsRunning = false;
		}

		private static int _errorCount;

		public DateTime? LastExecutionTime { get; private set; }
		public bool IsRunning { get; private set; }
		public string Name { get; private set; }
		public Timer Timer { get; private set; }
		public int Interval { get; private set; }
		public string ExceptionMessage { get; private set; }

		public void Dispose()
		{
			var e = new Exception(String.Format("Service {0} was disposed of at {1} UTC.", Name, DateTime.UtcNow));
			ErrorLog.Log(e, ErrorSeverity.Warning);
		}
	}
}