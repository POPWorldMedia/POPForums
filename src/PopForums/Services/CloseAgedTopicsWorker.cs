using System;
using System.Threading;
using PopForums.Configuration;

namespace PopForums.Services
{
	public class CloseAgedTopicsWorker
	{
		private static readonly object _syncRoot = new object();

		private CloseAgedTopicsWorker()
		{
		}

		public void CloseOldTopics(ITopicService topicService, IErrorLog errorLog)
		{
			if (!Monitor.TryEnter(_syncRoot)) return;
			try
			{
				topicService.CloseAgedTopics();
			}
			catch (Exception exc)
			{
				errorLog.Log(exc, ErrorSeverity.Error);
			}
			finally
			{
				Monitor.Exit(_syncRoot);
			}
		}

		private static CloseAgedTopicsWorker _instance;
		public static CloseAgedTopicsWorker Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new CloseAgedTopicsWorker();
				}
				return _instance;
			}
		}
	}
}