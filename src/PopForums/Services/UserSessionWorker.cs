namespace PopForums.Services;

public class UserSessionWorker
{
	private static readonly object _syncRoot = new Object();
	private static UserSessionWorker _instance;

	private UserSessionWorker()
	{
		// only allow Instance to create a new instance
	}

	public void CleanUpExpiredSessions(IUserSessionService sessionService, IErrorLog errorLog)
	{
		if (!Monitor.TryEnter(_syncRoot))
        {
            return;
        }

        try
		{
			sessionService.CleanUpExpiredSessions();
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

	public static UserSessionWorker Instance
	{
		get
		{
			_instance ??= new UserSessionWorker();
			return _instance;
		}
	}
}