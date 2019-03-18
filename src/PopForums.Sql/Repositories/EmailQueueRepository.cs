using Dapper;
using Newtonsoft.Json;
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

		public void Enqueue(EmailQueuePayload payload)
		{
			var serializedPayload = JsonConvert.SerializeObject(payload);
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_EmailQueue (Payload) VALUES (@Payload)", new { Payload = serializedPayload }));
		}

		public EmailQueuePayload Dequeue()
		{
			string serializedPayload = null;
			var sql = @"WITH cte AS (
SELECT TOP(1) Payload
FROM pf_EmailQueue WITH (ROWLOCK, READPAST)
ORDER BY Id)
DELETE FROM cte
OUTPUT DELETED.Payload;";
			_sqlObjectFactory.GetConnection().Using(connection =>
				serializedPayload = connection.QuerySingleOrDefault<string>(sql));
			if (string.IsNullOrEmpty(serializedPayload))
				return null;
			var payload = JsonConvert.DeserializeObject<EmailQueuePayload>(serializedPayload);
			return payload;
		}
	}
}