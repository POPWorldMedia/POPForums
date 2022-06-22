namespace PopForums.Mvc.Areas.Forums.Messaging;

public class NotificationHub : Hub
{
	private readonly INotificationManager _notificationManager;

	public NotificationHub(INotificationManager notificationManager)
	{
		_notificationManager = notificationManager;
	}

	public async void MarkNotificationRead(long contextID, string notificationType)
	{
		var notificationTypeEnum = Enum.Parse<NotificationType>(notificationType);
		var userIDstring = Context.User?.Claims.Single(x => x.Type == PopForumsAuthorizationDefaults.ForumsUserIDType);
		if (userIDstring == null)
			throw new Exception("No forum user ID claim found in hub context of User");
		var userID = Convert.ToInt32(userIDstring.Value);
		await _notificationManager.MarkNotificationRead(userID, notificationTypeEnum, contextID);
	}
}