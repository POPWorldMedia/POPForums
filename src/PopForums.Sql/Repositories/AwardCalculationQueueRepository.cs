using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using PopForums.Data.Sql;
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

		public void Enqueue(string eventDefinitionID, int userID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_AwardCalculationQueue (EventDefinitionID, UserID, TimeStamp) VALUES (@EventDefinitionID, @UserID, @TimeStamp)", new { EventDefinitionID = eventDefinitionID, UserID = userID, TimeStamp = DateTime.UtcNow }));
		}

		public KeyValuePair<string, int> Dequeue()
		{
			var pair = new KeyValuePair<string, int>();
			var sql = @"WITH cte AS (
SELECT TOP(1) EventDefinitionID AS [Key], UserID AS [Value]
FROM pf_AwardCalculationQueue WITH (ROWLOCK, READPAST)
ORDER BY ID)
DELETE FROM cte
OUTPUT DELETED.[Key], DELETED.[Value]";
			_sqlObjectFactory.GetConnection().Using(connection =>
				pair = connection.QuerySingleOrDefault<KeyValuePair<string, int>>(sql));
		return pair;
		}
	}
}
