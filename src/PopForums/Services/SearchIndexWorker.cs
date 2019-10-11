using System;
using System.Threading;
using PopForums.Configuration;

namespace PopForums.Services
{
	public class SearchIndexWorker
	{
		private static readonly object _syncRoot = new object();

		private SearchIndexWorker()
		{
			// only allow Instance to create a new instance
		}

		public void IndexNextTopic(IErrorLog errorLog, ISearchIndexSubsystem searchIndexSubsystem, ISearchService searchService, ITenantService tenantService)
		{
			if (!Monitor.TryEnter(_syncRoot, 5000)) return;
			try
			{
				var payload = searchService.GetNextTopicForIndexing().Result;
				if (payload == null)
					return;
				searchIndexSubsystem.DoIndex(payload.TopicID, payload.TenantID, payload.IsForRemoval);
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