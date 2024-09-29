namespace PopForums.Services;

public class BackgroundServices
{
	public static void SetupServices(IServiceProvider serviceProvider)
	{
		var setupService = serviceProvider.GetService<ISetupService>();
		if (!setupService.IsConnectionPossible() || !setupService.IsDatabaseSetup())
			return;
		//_emailService = new EmailApplicationService();
		_userSessionService = new UserSessionApplicationService();
		_searchIndexService = new SearchIndexApplicationService();
		_awardCalcService = new AwardCalculatorApplicationService();
		_closeAgedTopicsService = new CloseAgedTopicsApplicationService();
		_postImageCleanupService = new PostImageCleanupService();
		_subscribeNotificationApplicationService = new SubscribeNotificationApplicationService();
		//_emailService.Start(serviceProvider);
		_userSessionService.Start(serviceProvider);
		_searchIndexService.Start(serviceProvider);
		_awardCalcService.Start(serviceProvider);
		_closeAgedTopicsService.Start(serviceProvider);
		_postImageCleanupService.Start(serviceProvider);
		_subscribeNotificationApplicationService.Start(serviceProvider);
		//ApplicationServices.Add(_emailService);
		ApplicationServices.Add(_userSessionService);
		ApplicationServices.Add(_searchIndexService);
		ApplicationServices.Add(_awardCalcService);
		ApplicationServices.Add(_closeAgedTopicsService);
		ApplicationServices.Add(_postImageCleanupService);
		ApplicationServices.Add(_subscribeNotificationApplicationService);
	}

	//private static EmailApplicationService _emailService;
	private static UserSessionApplicationService _userSessionService;
	private static SearchIndexApplicationService _searchIndexService;
	private static AwardCalculatorApplicationService _awardCalcService;
	private static CloseAgedTopicsApplicationService _closeAgedTopicsService;
	private static PostImageCleanupService _postImageCleanupService;
	private static SubscribeNotificationApplicationService _subscribeNotificationApplicationService;

	public static readonly List<ApplicationServiceBase> ApplicationServices = new List<ApplicationServiceBase>();
}