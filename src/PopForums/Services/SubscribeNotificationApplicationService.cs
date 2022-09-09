namespace PopForums.Services;

public class SubscribeNotificationApplicationService : ApplicationServiceBase
{
	public override void Start(IServiceProvider serviceProvider)
	{
		_subscribeNotificationRepository = serviceProvider.GetService<ISubscribeNotificationRepository>();
		_subscribedTopicsService = serviceProvider.GetService<ISubscribedTopicsService>();
		_notificationAdapter = serviceProvider.GetService<INotificationAdapter>();
		_errorLog = serviceProvider.GetService<IErrorLog>();
		base.Start(serviceProvider);
	}

	private ISubscribeNotificationRepository _subscribeNotificationRepository;
	private ISubscribedTopicsService _subscribedTopicsService;
	private INotificationAdapter _notificationAdapter;
	private IErrorLog _errorLog;

	protected override void ServiceAction()
	{
		SubscribeNotificationWorker.Instance.ProcessCalculation(_subscribeNotificationRepository, _subscribedTopicsService, _notificationAdapter, _errorLog);
	}

	protected override int GetInterval()
	{
		// if checking every 5 seconds isn't enough, you should be using Azure Functions
		return 5000;
	}
}