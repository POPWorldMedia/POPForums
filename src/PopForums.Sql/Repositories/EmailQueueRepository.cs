using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using PopForums.Email;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class EmailQueueRepository : IEmailQueueRepository
	{
		private readonly ISqlObjectFactory _sqlObjectFactory;

		public EmailQueueRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		public async Task Enqueue(EmailQueuePayload payload)
		{
			var serializedPayload = JsonSerializer.Serialize(payload);
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_EmailQueue (Payload) VALUES (@Payload)", new { Payload = serializedPayload }));
		}

		public async Task<EmailQueuePayload> Dequeue()
		{
			Task<string> serializedPayload = null;
			var sql = @"WITH cte AS (
SELECT TOP(1) Payload
FROM pf_EmailQueue WITH (ROWLOCK, READPAST)
ORDER BY Id)
DELETE FROM cte
OUTPUT DELETED.Payload;";
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				serializedPayload = connection.QuerySingleOrDefaultAsync<string>(sql));
			if (string.IsNullOrEmpty(serializedPayload.Result))
				return null;
			var payload = JsonSerializer.Deserialize<EmailQueuePayload>(serializedPayload.Result);
			return payload;
		}
	}
}