using PopForums.Messaging;

namespace PopForums.Sql.Repositories;

public class NotificationRepository : INotificationRepository
{
	public async Task<int> UpdateNotification(Notification notification)
	{
		return 0;
	}

	public async Task CreateNotification(Notification notification)
	{
	}

	public async Task MarkNotificationRead(int userID, NotificationType notificationType, int? contextID)
	{
	}
}