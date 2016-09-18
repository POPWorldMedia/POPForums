using System;
using System.Collections.Generic;
using PopForums.Repositories;

namespace PopForums.Data.Sql.Repositories
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
				connection.Command("INSERT INTO pf_AwardCalculationQueue (EventDefinitionID, UserID, TimeStamp) VALUES (@EventDefinitionID, @UserID, @TimeStamp)")
				.AddParameter("@EventDefinitionID", eventDefinitionID)
				.AddParameter("@UserID", userID)
				.AddParameter("@TimeStamp", DateTime.UtcNow)
				.ExecuteNonQuery());
		}

		public KeyValuePair<string, int> Dequeue()
		{
			var pair = new KeyValuePair<string, int>();
			var sql = @"WITH cte AS (
SELECT TOP(1) EventDefinitionID, UserID
FROM pf_AwardCalculationQueue WITH (ROWLOCK, READPAST)
ORDER BY ID)
DELETE FROM cte
OUTPUT DELETED.EventDefinitionID, DELETED.UserID";
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(sql)
				.ExecuteReader()
				.ReadOne(r =>
				{ 
					pair = new KeyValuePair<string, int>(r.GetString(0), r.GetInt32(1));
				}));
			return pair;
		}
	}
}
