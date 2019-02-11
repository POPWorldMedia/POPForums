using Dapper;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Sql.Repositories
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
				connection.Execute("INSERT INTO pf_PointLedger (UserID, EventDefinitionID, Points, TimeStamp) VALUES (@UserID, @EventDefinitionID, @Points, @TimeStamp)", new { entry.UserID, entry.EventDefinitionID, entry.Points, entry.TimeStamp }));
		}

		public int GetPointTotal(int userID)
		{
			var total = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				total = connection.ExecuteScalar<int>("SELECT SUM(Points) FROM pf_PointLedger WHERE UserID = @UserID", new { UserID = userID }));
			return total;
		}

		public int GetEntryCount(int userID, string eventDefinitionID)
		{
			var total = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				total = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM pf_PointLedger WHERE UserID = @UserID AND EventDefinitionID = @EventDefinitionID", new { UserID = userID, EventDefinitionID = eventDefinitionID }));
			return total;
		}
	}
}
