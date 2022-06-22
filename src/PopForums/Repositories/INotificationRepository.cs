namespace PopForums.Repositories;

public interface INotificationRepository
{
	Task<int> UpdateNotification(Notification notification);
	Task CreateNotification(Notification notification);
	Task MarkNotificationRead(int userID, NotificationType notificationType, long contextID);
	Task<List<Notification>> GetNotifications(int userID);
	Task<int> GetUnreadNotificationCount(int userID);
}