using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Sql.Repositories
{
	public class AwardConditionRepository : IAwardConditionRepository
	{
		public AwardConditionRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public async Task<List<AwardCondition>> GetConditions(string awardDefinitionID)
		{
			Task<IEnumerable<AwardCondition>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QueryAsync<AwardCondition>("SELECT AwardDefinitionID, EventDefinitionID, EventCount FROM pf_AwardCondition WHERE AwardDefinitionID = @AwardDefinitionID", new { AwardDefinitionID  = awardDefinitionID }));
			var list = result.Result.ToList();
			return list;
		}

		public async Task DeleteConditions(string awardDefinitionID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_AwardCondition WHERE AwardDefinitionID = @AwardDefinitionID", new { AwardDefinitionID = awardDefinitionID }));
		}

		public async Task DeleteCondition(string awardDefinitionID, string eventDefinitionID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_AwardCondition WHERE AwardDefinitionID = @AwardDefinitionID AND EventDefinitionID = @EventDefinitionID", new { AwardDefinitionID = awardDefinitionID, EventDefinitionID = eventDefinitionID }));
		}

		public async Task DeleteConditionsByEventDefinitionID(string eventDefinitionID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_AwardCondition WHERE EventDefinitionID = @EventDefinitionID", new { EventDefinitionID = eventDefinitionID }));
		}

		public async Task SaveConditions(List<AwardCondition> conditions)
		{
			foreach (var condition in conditions)
			{
				await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
					connection.ExecuteAsync("INSERT INTO pf_AwardCondition (AwardDefinitionID, EventDefinitionID, EventCount) VALUES (@AwardDefinitionID, @EventDefinitionID, @EventCount)", new { condition.AwardDefinitionID, condition.EventDefinitionID, condition.EventCount }));
			}
		}
	}
}
