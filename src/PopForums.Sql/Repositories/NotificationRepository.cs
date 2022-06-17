using PopForums.Messaging;

namespace PopForums.Sql.Repositories;

public class NotificationRepository : INotificationRepository
{
	private readonly ISqlObjectFactory _sqlObjectFactory;

	public NotificationRepository(ISqlObjectFactory sqlObjectFactory)
	{
		_sqlObjectFactory = sqlObjectFactory;
		SqlMapper.AddTypeHandler(new JsonElementTypeHandler());
	}

	public async Task<int> UpdateNotification(Notification notification)
	{
		Task<int> total = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			total = connection.ExecuteAsync("UPDATE pf_Notifications SET TimeStamp = @TimeStamp, IsRead = @IsRead, Data = @Data WHERE UserID = @UserID AND NotificationType = @NotificationType AND ContextID = @ContextID", notification));
		return await total;
	}

	public async Task CreateNotification(Notification notification)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("INSERT INTO pf_Notifications (UserID, NotificationType, ContextID, TimeStamp, IsRead, Data) VALUES (@UserID, @NotificationType, @ContextID, @TimeStamp, @IsRead, @Data)", notification));
	}

	public async Task MarkNotificationRead(int userID, NotificationType notificationType, int? contextID)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("UPDATE pf_Notifications SET IsRead = 1 WHERE UserID = @userID AND NotificationType = @notificationType AND ContextID = @contextID", new { userID, notificationType, contextID }));
	}

	public async Task<List<Notification>> GetNotifications(int userID)
	{
		Task<IEnumerable<Notification>> notifications = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			notifications = connection.QueryAsync<Notification>("SELECT * FROM pf_Notifications WHERE UserID = @userID ORDER BY TimeStamp DESC", new { userID }));
		return notifications.Result.ToList();
	}
}