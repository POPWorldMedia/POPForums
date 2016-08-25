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
			base.Start(serviceProvider);
		}

		private ISearchService _searchService;
		private ISettingsManager _settingsManager;
		private IPostService _postService;

		protected override void ServiceAction()
		{
			SearchIndexWorker.Instance.IndexNextTopic(_searchService, _settingsManager, _postService, ErrorLog);
		}

		protected override int GetInterval()
		{
			return _settingsManager.Current.SearchIndexingInterval;
		}
	}
}