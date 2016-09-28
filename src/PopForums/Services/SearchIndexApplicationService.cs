using System;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Configuration;

namespace PopForums.Services
{
	public class SearchIndexApplicationService : ApplicationServiceBase
	{
		public override void Start(IServiceProvider serviceProvider)
		{
			_searchService = serviceProvider.GetService<ISearchService>();
			_settingsManager = serviceProvider.GetService<ISettingsManager>();
			_postService = serviceProvider.GetService<IPostService>();
			_searchIndexSubsystem = serviceProvider.GetService<ISearchIndexSubsystem>();
			_config = serviceProvider.GetService<IConfig>();
			_topicService = serviceProvider.GetService<ITopicService>();
			base.Start(serviceProvider);
		}

		private ISearchService _searchService;
		private ISettingsManager _settingsManager;
		private IPostService _postService;
		private ISearchIndexSubsystem _searchIndexSubsystem;
		private IConfig _config;
		private ITopicService _topicService;

		protected override void ServiceAction()
		{
			SearchIndexWorker.Instance.IndexNextTopic(_searchService, _settingsManager, _postService, ErrorLog, _searchIndexSubsystem, _config, _topicService);
		}

		protected override int GetInterval()
		{
			return _settingsManager.Current.SearchIndexingInterval;
		}
	}
}