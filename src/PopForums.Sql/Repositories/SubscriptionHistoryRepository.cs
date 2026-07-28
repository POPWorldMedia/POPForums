namespace PopForums.Sql.Repositories;

public class SubscriptionHistoryRepository : ISubscriptionHistoryRepository
{
	public SubscriptionHistoryRepository(ISqlObjectFactory sqlObjectFactory)
	{
		_sqlObjectFactory = sqlObjectFactory;
	}

	private readonly ISqlObjectFactory _sqlObjectFactory;

	public async Task Create(SubscriptionHistory subscriptionHistory)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("INSERT INTO pf_SubscriptionHistory (UserID, TimeStamp, SkuID, Message) VALUES (@UserID, @TimeStamp, @SkuID, @Message)", new { subscriptionHistory.UserID, subscriptionHistory.TimeStamp, subscriptionHistory.SkuID, Message = subscriptionHistory.Message.NullToEmpty() }));
	}

	public async Task<List<SubscriptionHistory>> GetByUserID(int userID)
	{
		Task<IEnumerable<SubscriptionHistory>> result = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			result = connection.QueryAsync<SubscriptionHistory>("SELECT SubscriptionHistoryID, UserID, TimeStamp, SkuID, Message FROM pf_SubscriptionHistory WHERE UserID = @UserID ORDER BY TimeStamp", new { UserID = userID }));
		return result.Result.ToList();
	}
}
