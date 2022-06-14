namespace PopForums.Messaging;

public interface INotificationTunnel
{
	void SendNotificationForUserAward(string title, int userID, string tenantID);
}

public class NotificationTunnel : INotificationTunnel
{
	public static string HeaderName = "PfApi";

	private readonly INotificationAdapter _notificationAdapter;

	public NotificationTunnel(INotificationAdapter notificationAdapter)
	{
		_notificationAdapter = notificationAdapter;
	}

	public async void SendNotificationForUserAward(string title, int userID, string tenantID)
	{
		await _notificationAdapter.Award(title, userID, tenantID);
	}
}