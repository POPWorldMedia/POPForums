namespace PopForums.Repositories;

public interface INotificationRepository
{
	Task<int> UpdateNotification(Notification notification);
	Task CreateNotification(Notification notification);
	Task MarkNotificationRead(int userID, NotificationType notificationType, long contextID);
	Task<List<Notification>> GetNotifications(int userID, int startRow, int pageSize);
	Task<int> GetPageCount(int userID, int pageSize);
	Task<int> GetUnreadNotificationCount(int userID);
	Task MarkAllRead(int userID);
}