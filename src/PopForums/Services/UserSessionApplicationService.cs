namespace PopForums.Services;

public class UserSessionApplicationService : ApplicationServiceBase
{
	private IUserSessionService _userSessionService;

	public override void Start(IServiceProvider serviceProvider)
	{
		_userSessionService = serviceProvider.GetService<IUserSessionService>();
		base.Start(serviceProvider);
	}

	protected override void ServiceAction()
	{
		UserSessionWorker.Instance.CleanUpExpiredSessions(_userSessionService, ErrorLog);
	}

	protected override int GetInterval()
	{
		return 10000;
	}
}