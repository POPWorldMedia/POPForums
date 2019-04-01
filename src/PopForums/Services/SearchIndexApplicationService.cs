using System;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Configuration;

namespace PopForums.Services
{
	public class SearchIndexApplicationService : ApplicationServiceBase
	{
		public override void Start(IServiceProvider serviceProvider)
		{
			_settingsManager = serviceProvider.GetService<ISettingsManager>();
			_searchIndexSubsystem = serviceProvider.GetService<ISearchIndexSubsystem>();
			_searchService = serviceProvider.GetService<ISearchService>();
			_tenantService = serviceProvider.GetService<ITenantService>();
			base.Start(serviceProvider);
		}
		
		private ISettingsManager _settingsManager;
		private ISearchIndexSubsystem _searchIndexSubsystem;
		private ISearchService _searchService;
		private ITenantService _tenantService;

		protected override void ServiceAction()
		{
			SearchIndexWorker.Instance.IndexNextTopic(ErrorLog, _searchIndexSubsystem, _searchService, _tenantService);
		}

		protected override int GetInterval()
		{
			return _settingsManager.Current.SearchIndexingInterval;
		}
	}
}