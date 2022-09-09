namespace PopForums.Sql.Repositories;

public class SubscribeNotificationRepository : ISubscribeNotificationRepository
{
	private readonly ISqlObjectFactory _sqlObjectFactory;

	public SubscribeNotificationRepository(ISqlObjectFactory sqlObjectFactory)
	{
		_sqlObjectFactory = sqlObjectFactory;
	}

	public async Task Enqueue(SubscribeNotificationPayload payload)
	{
		var serializedPayload = JsonSerializer.Serialize(payload);
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("INSERT INTO pf_SubNotifyQueue (Payload) VALUES (@Payload)", new { Payload = serializedPayload }));
	}

	public async Task<SubscribeNotificationPayload> Dequeue()
	{
		Task<string> serializedPayload = null;
		var sql = @"WITH cte AS (
SELECT TOP(1) Payload
FROM pf_SubNotifyQueue WITH (ROWLOCK, READPAST)
ORDER BY Id)
DELETE FROM cte
OUTPUT DELETED.Payload;";
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			serializedPayload = connection.QuerySingleOrDefaultAsync<string>(sql));
		if (string.IsNullOrEmpty(serializedPayload.Result))
			return new SubscribeNotificationPayload();
		var payload = JsonSerializer.Deserialize<SubscribeNotificationPayload>(serializedPayload.Result);
		return payload;
	}
}