using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class AwardCalculationQueueRepository : IAwardCalculationQueueRepository
	{
		public AwardCalculationQueueRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public async Task Enqueue(AwardCalculationPayload payload)
		{
			var serializedPayload = JsonConvert.SerializeObject(payload);
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_AwardCalculationQueue (Payload) VALUES (@Payload)", new { Payload = serializedPayload }));
		}

		public async Task<KeyValuePair<string, int>> Dequeue()
		{
			Task<string> serializedPayload = null;
			var sql = @"WITH cte AS (
SELECT TOP(1) Payload
FROM pf_AwardCalculationQueue WITH (ROWLOCK, READPAST)
ORDER BY Id)
DELETE FROM cte
OUTPUT DELETED.Payload;";
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				serializedPayload = connection.QuerySingleOrDefaultAsync<string>(sql));
			if (string.IsNullOrEmpty(serializedPayload.Result))
				return new KeyValuePair<string, int>();
			var payload = JsonConvert.DeserializeObject<AwardCalculationPayload>(serializedPayload.Result);
			return new KeyValuePair<string, int>(payload.EventDefinitionID, payload.UserID);
		}
	}
}
