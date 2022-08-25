namespace PopForums.Messaging;

public interface INotificationManager
{
	Task MarkNotificationRead(int userID, NotificationType notificationType, long contextID);
	Task ProcessNotification(int userID, NotificationType notificationType, long contextID, dynamic data);
	Task ProcessNotification(int userID, NotificationType notificationType, long contextID, dynamic data, string tenantID);
	Task<int> GetUnreadNotificationCount(int userID);
	Task MarkAllRead(int userID);
	Task<List<Notification>> GetNotifications(int userID, DateTime afterDateTime);
}

public class NotificationManager : INotificationManager
{
	private readonly INotificationRepository _notificationRepository;
	private readonly IBroker _broker;
	private const int PageSize = 20;
	private const int MaxNotificationCount = 100;

	public NotificationManager(INotificationRepository notificationRepository, IBroker broker)
	{
		_notificationRepository = notificationRepository;
		_broker = broker;
	}

	public async Task ProcessNotification(int userID, NotificationType notificationType, long contextID, dynamic data)
	{
		await ProcessNotification(userID, notificationType, contextID, data, null);
	}

	public async Task ProcessNotification(int userID, NotificationType notificationType, long contextID, dynamic data, string tenantID)
	{
		var serializedData = JsonSerializer.SerializeToElement(data, new JsonSerializerOptions{ PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
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

		if (tenantID == null || string.IsNullOrWhiteSpace(tenantID))
			_broker.NotifyUser(notification);
		else
			_broker.NotifyUser(notification, tenantID);
	}

	public async Task MarkNotificationRead(int userID, NotificationType notificationType, long contextID)
	{
		await _notificationRepository.MarkNotificationRead(userID, notificationType, contextID);
	}

	public async Task<List<Notification>> GetNotifications(int userID, DateTime afterDateTime)
	{
		return await _notificationRepository.GetNotifications(userID, afterDateTime, PageSize);
	}

	public async Task<int> GetUnreadNotificationCount(int userID)
	{
		var count = await _notificationRepository.GetUnreadNotificationCount(userID);
		return count > MaxNotificationCount ? MaxNotificationCount : count;
	}

	public async Task MarkAllRead(int userID)
	{
		await _notificationRepository.MarkAllRead(userID);
	}
}