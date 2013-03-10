using System;
using PopForums.Configuration;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Data.SqlSingleWebServer.Repositories
{
	public class PointLedgerRepository : IPointLedgerRepository
	{
		public PointLedgerRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public virtual void RecordEntry(PointLedgerEntry entry)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("INSERT INTO pf_PointLedger (UserID, EventDefinitionID, Points, TimeStamp) VALUES (@UserID, @EventDefinitionID, @Points, @TimeStamp)")
				.AddParameter("@UserID", entry.UserID)
				.AddParameter("@EventDefinitionID", entry.EventDefinitionID)
				.AddParameter("@Points", entry.Points)
				.AddParameter("@TimeStamp", entry.TimeStamp)
				.ExecuteNonQuery());
		}

		public int GetPointTotal(int userID)
		{
			var total = 0;
			_sqlObjectFactory.GetConnection().Using(connection => total = Convert.ToInt32(connection.Command("SELECT SUM(Points) FROM pf_PointLedger WHERE UserID = @UserID")
				.AddParameter("@UserID", userID)
				.ExecuteScalar()));
			return total;
		}

		public int GetEntryCount(int userID, string eventDefinitionID)
		{
			var total = 0;
			_sqlObjectFactory.GetConnection().Using(connection => total = Convert.ToInt32(connection.Command("SELECT COUNT(*) FROM pf_PointLedger WHERE UserID = @UserID AND EventDefinitionID = @EventDefinitionID")
				.AddParameter("@UserID", userID)
				.AddParameter("@EventDefinitionID", eventDefinitionID)
				.ExecuteScalar()));
			return total;
		}
	}
}
