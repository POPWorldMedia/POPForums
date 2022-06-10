using PopForums.Messaging.Models;

namespace PopForums.Messaging;

public interface INotificationAdapter
{
	Task Reply(string postName, string title, int topicID, int userID);
}

public class NotificationAdapter : INotificationAdapter
{
	private readonly INotificationManager _notificationManager;

	public NotificationAdapter(INotificationManager notificationManager)
	{
		_notificationManager = notificationManager;
	}

	public async Task Reply(string postName, string title, int topicID, int userID)
	{
		var replyData = new ReplyData
		{
			PostName = postName,
			Title = title,
			TopicID = topicID
		};
		await _notificationManager.ProcessNotification(userID, NotificationType.NewReply, replyData.TopicID, replyData);
	}
}