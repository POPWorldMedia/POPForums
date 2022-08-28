namespace PopForums.Mvc.Areas.Forums.Messaging;

public class NotificationHub : Hub
{
	private readonly INotificationManager _notificationManager;
	private readonly IPrivateMessageService _privateMessageService;

	public NotificationHub(INotificationManager notificationManager, IPrivateMessageService privateMessageService)
	{
		_notificationManager = notificationManager;
		_privateMessageService = privateMessageService;
	}

	private int GetUserID()
	{
		var userIDstring = Context.User?.Claims.Single(x => x.Type == PopForumsAuthorizationDefaults.ForumsUserIDType);
		if (userIDstring == null)
			throw new Exception("No forum user ID claim found in hub context of User");
		var userID = Convert.ToInt32(userIDstring.Value);
		return userID;
	}

	public async Task MarkNotificationRead(long contextID, int notificationType)
	{
		var notificationTypeEnum = (NotificationType)notificationType;
		var userID = GetUserID();
		await _notificationManager.MarkNotificationRead(userID, notificationTypeEnum, contextID);
	}

	public async Task MarkAllRead()
	{
		var userID = GetUserID();
		await _notificationManager.MarkAllRead(userID);
	}

	public async Task<List<Notification>> GetNotifications(DateTime afterDateTime)
	{
		var userID = GetUserID();
		var notifications = await _notificationManager.GetNotifications(userID, afterDateTime);
		return notifications;
	}

	public async Task<int> GetNotificationCount()
	{
		var userID = GetUserID();
		var count = await _notificationManager.GetUnreadNotificationCount(userID);
		return count;
	}

	public async Task<int> GetPMCount()
	{
		var userID = GetUserID();
		var count = await _privateMessageService.GetUnreadCount(userID);
		return count;
	}
}