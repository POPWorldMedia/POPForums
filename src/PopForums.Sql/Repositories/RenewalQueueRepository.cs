namespace PopForums.Sql.Repositories;

public class RenewalQueueRepository : IRenewalQueueRepository
{
	private readonly ISqlObjectFactory _sqlObjectFactory;

	public RenewalQueueRepository(ISqlObjectFactory sqlObjectFactory)
	{
		_sqlObjectFactory = sqlObjectFactory;
	}

	public async Task Enqueue(RenewalQueuePayload payload)
	{
		var serializedPayload = JsonSerializer.Serialize(payload);
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("INSERT INTO pf_RenewalQueue (Payload) VALUES (@Payload)", new { Payload = serializedPayload }));
	}

	public async Task<RenewalQueuePayload> Dequeue()
	{
		Task<string> serializedPayload = null;
		var sql = @"WITH cte AS (
SELECT TOP(1) Payload
FROM pf_RenewalQueue WITH (ROWLOCK, READPAST)
ORDER BY Id)
DELETE FROM cte
OUTPUT DELETED.Payload;";
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			serializedPayload = connection.QuerySingleOrDefaultAsync<string>(sql));
		if (string.IsNullOrEmpty(serializedPayload.Result))
			return null;
		var payload = JsonSerializer.Deserialize<RenewalQueuePayload>(serializedPayload.Result);
		return payload;
	}
}
