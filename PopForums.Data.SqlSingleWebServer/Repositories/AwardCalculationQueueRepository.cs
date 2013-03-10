using System;
using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer.Repositories
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
			var id = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT TOP 1 ID, EventDefinitionID, UserID FROM pf_AwardCalculationQueue ORDER BY TimeStamp")
				.ExecuteReader()
				.ReadOne(r =>
				{ 
					pair = new KeyValuePair<string, int>(r.GetString(1), r.GetInt32(2));
					id = r.GetInt32(0);
				}));
			if (id != 0)
				_sqlObjectFactory.GetConnection().Using(connection =>
					connection.Command("DELETE FROM pf_AwardCalculationQueue WHERE ID = @ID")
					.AddParameter("@ID", id)
					.ExecuteNonQuery());
			return pair;
		}
	}
}
