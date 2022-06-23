namespace PopForums.Mvc.Areas.Forums.Messaging;

public class NotificationHub : Hub
{
	private readonly INotificationManager _notificationManager;

	public NotificationHub(INotificationManager notificationManager)
	{
		_notificationManager = notificationManager;
	}

	private int GetUserID()
	{
		var userIDstring = Context.User?.Claims.Single(x => x.Type == PopForumsAuthorizationDefaults.ForumsUserIDType);
		if (userIDstring == null)
			throw new Exception("No forum user ID claim found in hub context of User");
		var userID = Convert.ToInt32(userIDstring.Value);
		return userID;
	}

	public async void MarkNotificationRead(long contextID, int notificationType)
	{
		var notificationTypeEnum = (NotificationType)notificationType;
		var userID = GetUserID();
		await _notificationManager.MarkNotificationRead(userID, notificationTypeEnum, contextID);
	}

	public async void MarkAllRead()
	{
		var userID = GetUserID();
		await _notificationManager.MarkAllRead(userID);
	}

	public async Task<List<Notification>> GetNotifications()
	{
		var userID = GetUserID();
		var notifications = await _notificationManager.GetNotifications(userID);
		return notifications;
	}
}