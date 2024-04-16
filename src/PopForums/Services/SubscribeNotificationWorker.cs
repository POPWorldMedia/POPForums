using PopForums.Services.Interfaces;

namespace PopForums.Services;

public class SubscribeNotificationWorker
{
	private SubscribeNotificationWorker()
	{
		// only allow Instance to create a new instance
	}

	public void ProcessCalculation(ISubscribeNotificationRepository subscribeNotificationRepository, ISubscribedTopicsService subscribedTopicsService, INotificationAdapter notificationAdapter, IErrorLog errorLog)
	{
		try
		{
			var payload = subscribeNotificationRepository.Dequeue().Result;
			if (payload == null)
				return;
			var userIDs = subscribedTopicsService.GetSubscribedUserIDs(payload.TopicID).Result;
			var filteredUserIDs = userIDs.Where(x => x != payload.PostingUserID);
			foreach (var userID in filteredUserIDs)
				notificationAdapter.Reply(payload.PostingUserName, payload.TopicTitle, payload.TopicID, userID, payload.TenantID);
		}
		catch (Exception exc)
		{
			errorLog.Log(exc, ErrorSeverity.Error);
		}
	}

	private static SubscribeNotificationWorker _instance;
	public static SubscribeNotificationWorker Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new SubscribeNotificationWorker();
			}
			return _instance;
		}
	}
}