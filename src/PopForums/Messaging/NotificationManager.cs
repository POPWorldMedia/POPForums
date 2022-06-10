namespace PopForums.Messaging;

public interface INotificationManager
{
	Task ProcessNotification(int userID, NotificationType notificationType, int contextID, dynamic data);
}

public class NotificationManager : INotificationManager
{
	private readonly INotificationRepository _notificationRepository;
	private readonly IBroker _broker;

	public NotificationManager(INotificationRepository notificationRepository, IBroker broker)
	{
		_notificationRepository = notificationRepository;
		_broker = broker;
	}

	public async Task ProcessNotification(int userID, NotificationType notificationType, int contextID, dynamic data)
	{
		var serializedData = JsonSerializer.SerializeToElement(data);
		var notification = new Notification
		{
			UserID = userID,
			TimeStamp = DateTime.UtcNow,
			IsRead = false,
			NotificationType = notificationType,
			ContextID = contextID,
			Data = serializedData
		};

		var recordsUpdated = await _notificationRepository.UpdateNotification(notification);
		if (recordsUpdated == 0)
			await _notificationRepository.CreateNotification(notification);

		_broker.NotifyUser(notification);
	}
}