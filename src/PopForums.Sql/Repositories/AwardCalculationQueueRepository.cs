using System.Collections.Generic;
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

		public void Enqueue(AwardCalculationPayload payload)
		{
			var serializedPayload = JsonConvert.SerializeObject(payload);
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_AwardCalculationQueue (Payload) VALUES (@Payload)", new { Payload = serializedPayload }));
		}

		public KeyValuePair<string, int> Dequeue()
		{
			string serializedPayload = null;
			var sql = @"WITH cte AS (
SELECT TOP(1) Payload
FROM pf_AwardCalculationQueue WITH (ROWLOCK, READPAST)
ORDER BY Id)
DELETE FROM cte
OUTPUT DELETED.Payload;";
			_sqlObjectFactory.GetConnection().Using(connection =>
				serializedPayload = connection.QuerySingleOrDefault<string>(sql));
			if (string.IsNullOrEmpty(serializedPayload))
				return new KeyValuePair<string, int>();
			var payload = JsonConvert.DeserializeObject<AwardCalculationPayload>(serializedPayload);
			return new KeyValuePair<string, int>(payload.EventDefinitionID, payload.UserID);
		}
	}
}
