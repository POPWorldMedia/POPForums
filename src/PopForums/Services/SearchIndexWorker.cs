using System;
using System.Threading;
using PopForums.Configuration;

namespace PopForums.Services
{
	public class SearchIndexWorker
	{
		private static readonly object _syncRoot = new Object();

		private SearchIndexWorker()
		{
			// only allow Instance to create a new instance
		}

		public void IndexNextTopic(ISearchService searchService, ISettingsManager settingsManager, IPostService postService, IErrorLog errorLog, ISearchIndexSubsystem searchIndexSubsystem, IConfig config, ITopicService topicService)
		{
			if (!Monitor.TryEnter(_syncRoot)) return;
			try
			{
				searchIndexSubsystem.DoIndex(searchService, settingsManager, postService, config, topicService, errorLog);
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

		private static SearchIndexWorker _instance;
		public static SearchIndexWorker Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new SearchIndexWorker();
				}
				return _instance;
			}
		}
	}
}