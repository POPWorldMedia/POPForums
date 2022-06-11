namespace PopForums.Mvc.Areas.Forums.Messaging;

public class NotificationHub : Hub
{
	private readonly INotificationManager _notificationManager;
	private readonly ITenantService _tenantService;

	public NotificationHub(INotificationManager notificationManager, ITenantService tenantService)
	{
		_notificationManager = notificationManager;
		_tenantService = tenantService;
	}

	public async void MarkNotificationRead(int contextID, NotificationType notificationType)
	{
		var tenantID = _tenantService.GetTenant();
		var userIDstring = PopForumsUserIdProvider.GetBaseUserID(tenantID, Context.ConnectionId);
		var userID = Convert.ToInt32(userIDstring);
		await _notificationManager.MarkNotificationRead(userID, notificationType, contextID);
	}
}