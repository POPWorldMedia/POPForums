namespace PopForums.Messaging;

public interface INotificationManager
{
	Task MarkNotificationRead(int userID, NotificationType notificationType, int? contextID);
	Task ProcessNotification(int userID, NotificationType notificationType, int? contextID, dynamic data);
	Task ProcessNotification(int userID, NotificationType notificationType, int? contextID, dynamic data, string tenantID);
	Task<List<Notification>> GetNotifications(int userID);
	Task<int> GetUnreadNotificationCount(int userID);
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

	public async Task ProcessNotification(int userID, NotificationType notificationType, int? contextID, dynamic data)
	{
		await ProcessNotification(userID, notificationType, contextID, data, null);
	}

	public async Task ProcessNotification(int userID, NotificationType notificationType, int? contextID, dynamic data, string tenantID)
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

		if (!notification.ContextID.HasValue)
			await _notificationRepository.CreateNotification(notification);
		else
		{
			var recordsUpdated = await _notificationRepository.UpdateNotification(notification);
			if (recordsUpdated == 0)
				await _notificationRepository.CreateNotification(notification);
		}

		if (tenantID == null || string.IsNullOrWhiteSpace(tenantID))
			_broker.NotifyUser(notification);
		else
			_broker.NotifyUser(notification, tenantID);
	}

	public async Task MarkNotificationRead(int userID, NotificationType notificationType, int? contextID)
	{
		await _notificationRepository.MarkNotificationRead(userID, notificationType, contextID);
	}

	public async Task<List<Notification>> GetNotifications(int userID)
	{
		return await _notificationRepository.GetNotifications(userID);
	}

	public async Task<int> GetUnreadNotificationCount(int userID)
	{
		return await _notificationRepository.GetUnreadNotificationCount(userID);
	}
}