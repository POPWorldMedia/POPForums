namespace PopForums.Messaging;

public interface INotificationTunnel
{
	void SendNotificationForUserAward(string title, int userID, string tenantID);
	void SendNotificationForReply(string postName, string title, int topicID, int userID, string tenantID);
	void SendNotificationForSubscriptionRenewed(int userID, string skuName, string tenantID);
	void SendNotificationForSubscriptionRenewalFailed(int userID, string skuName, string tenantID);
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

	public async void SendNotificationForReply(string postName, string title, int topicID, int userID, string tenantID)
	{
		await _notificationAdapter.Reply(postName, title, topicID, userID, tenantID);
	}

	public async void SendNotificationForSubscriptionRenewed(int userID, string skuName, string tenantID)
	{
		await _notificationAdapter.SubscriptionRenewed(userID, skuName, tenantID);
	}

	public async void SendNotificationForSubscriptionRenewalFailed(int userID, string skuName, string tenantID)
	{
		await _notificationAdapter.SubscriptionRenewalFailed(userID, skuName, tenantID);
	}
}