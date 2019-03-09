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
			base.Start(serviceProvider);
		}
		
		private ISettingsManager _settingsManager;
		private ISearchIndexSubsystem _searchIndexSubsystem;

		protected override void ServiceAction()
		{
			SearchIndexWorker.Instance.IndexNextTopic(ErrorLog, _searchIndexSubsystem);
		}

		protected override int GetInterval()
		{
			return _settingsManager.Current.SearchIndexingInterval;
		}
	}
}