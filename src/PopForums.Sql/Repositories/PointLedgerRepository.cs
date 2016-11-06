using System;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Data.Sql.Repositories
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
				connection.Command(_sqlObjectFactory, "INSERT INTO pf_PointLedger (UserID, EventDefinitionID, Points, TimeStamp) VALUES (@UserID, @EventDefinitionID, @Points, @TimeStamp)")
				.AddParameter(_sqlObjectFactory, "@UserID", entry.UserID)
				.AddParameter(_sqlObjectFactory, "@EventDefinitionID", entry.EventDefinitionID)
				.AddParameter(_sqlObjectFactory, "@Points", entry.Points)
				.AddParameter(_sqlObjectFactory, "@TimeStamp", entry.TimeStamp)
				.ExecuteNonQuery());
		}

		public int GetPointTotal(int userID)
		{
			var total = 0;
			_sqlObjectFactory.GetConnection().Using(connection => total = Convert.ToInt32(connection.Command(_sqlObjectFactory, "SELECT SUM(Points) FROM pf_PointLedger WHERE UserID = @UserID")
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.ExecuteScalar()));
			return total;
		}

		public int GetEntryCount(int userID, string eventDefinitionID)
		{
			var total = 0;
			_sqlObjectFactory.GetConnection().Using(connection => total = Convert.ToInt32(connection.Command(_sqlObjectFactory, "SELECT COUNT(*) FROM pf_PointLedger WHERE UserID = @UserID AND EventDefinitionID = @EventDefinitionID")
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.AddParameter(_sqlObjectFactory, "@EventDefinitionID", eventDefinitionID)
				.ExecuteScalar()));
			return total;
		}
	}
}
