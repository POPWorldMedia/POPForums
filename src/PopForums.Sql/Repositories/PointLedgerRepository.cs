using System.Threading.Tasks;
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

		public virtual async Task RecordEntry(PointLedgerEntry entry)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_PointLedger (UserID, EventDefinitionID, Points, TimeStamp) VALUES (@UserID, @EventDefinitionID, @Points, @TimeStamp)", new { entry.UserID, entry.EventDefinitionID, entry.Points, entry.TimeStamp }));
		}

		public async Task<int> GetPointTotal(int userID)
		{
			Task<int> total = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				total = connection.ExecuteScalarAsync<int>("SELECT SUM(Points) FROM pf_PointLedger WHERE UserID = @UserID", new { UserID = userID }));
			return await total;
		}

		public async Task<int> GetEntryCount(int userID, string eventDefinitionID)
		{
			Task<int> total = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				total = connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM pf_PointLedger WHERE UserID = @UserID AND EventDefinitionID = @EventDefinitionID", new { UserID = userID, EventDefinitionID = eventDefinitionID }));
			return await total;
		}
	}
}
