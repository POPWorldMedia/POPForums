namespace PopForums.Services;

public interface ISubscribeNotificationWorker
{
	void Execute();
}

public class SubscribeNotificationWorker(ISubscribeNotificationRepository subscribeNotificationRepository, ISubscribedTopicsService subscribedTopicsService, INotificationAdapter notificationAdapter, IErrorLog errorLog) : ISubscribeNotificationWorker
{
	public async void Execute()
	{
		try
		{
			var payload = await subscribeNotificationRepository.Dequeue();
			if (payload == null)
				return;
			var userIDs = await subscribedTopicsService.GetSubscribedUserIDs(payload.TopicID);
			var filteredUserIDs = userIDs.Where(x => x != payload.PostingUserID);
			foreach (var userID in filteredUserIDs)
				await notificationAdapter.Reply(payload.PostingUserName, payload.TopicTitle, payload.TopicID, userID, payload.TenantID);
		}
		catch (Exception exc)
		{
			errorLog.Log(exc, ErrorSeverity.Error);
		}
	}
}