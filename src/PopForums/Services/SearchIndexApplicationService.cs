using PopForums.Services.Interfaces;

namespace PopForums.Services;

public class SearchIndexApplicationService : ApplicationServiceBase
{
	private ISettingsManager _settingsManager;
	private ISearchIndexSubsystem _searchIndexSubsystem;
	private ISearchService _searchService;
	private ITenantService _tenantService;

	public override void Start(IServiceProvider serviceProvider)
	{
		_settingsManager = serviceProvider.GetService<ISettingsManager>();
		_searchIndexSubsystem = serviceProvider.GetService<ISearchIndexSubsystem>();
		_searchService = serviceProvider.GetService<ISearchService>();
		_tenantService = serviceProvider.GetService<ITenantService>();
		base.Start(serviceProvider);
	}

	protected override void ServiceAction()
	{
		SearchIndexWorker.Instance.IndexNextTopic(ErrorLog, _searchIndexSubsystem, _searchService, _tenantService);
	}

	protected override int GetInterval()
	{
		return _settingsManager.Current.SearchIndexingInterval;
	}
}