using System.Collections.Generic;
using System.Linq;
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

		public List<AwardCondition> GetConditions(string awardDefinitionID)
		{
			var list = new List<AwardCondition>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<AwardCondition>("SELECT AwardDefinitionID, EventDefinitionID, EventCount FROM pf_AwardCondition WHERE AwardDefinitionID = @AwardDefinitionID", new { AwardDefinitionID  = awardDefinitionID }).ToList());
			return list;
		}

		public void DeleteConditions(string awardDefinitionID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_AwardCondition WHERE AwardDefinitionID = @AwardDefinitionID", new { AwardDefinitionID = awardDefinitionID }));
		}

		public void DeleteCondition(string awardDefinitionID, string eventDefinitionID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_AwardCondition WHERE AwardDefinitionID = @AwardDefinitionID AND EventDefinitionID = @EventDefinitionID", new { AwardDefinitionID = awardDefinitionID, EventDefinitionID = eventDefinitionID }));
		}

		public void DeleteConditionsByEventDefinitionID(string eventDefinitionID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_AwardCondition WHERE EventDefinitionID = @EventDefinitionID", new { EventDefinitionID = eventDefinitionID }));
		}

		public void SaveConditions(List<AwardCondition> conditions)
		{
			foreach (var condition in conditions)
			{
				_sqlObjectFactory.GetConnection().Using(connection =>
					connection.Execute("INSERT INTO pf_AwardCondition (AwardDefinitionID, EventDefinitionID, EventCount) VALUES (@AwardDefinitionID, @EventDefinitionID, @EventCount)", new { condition.AwardDefinitionID, condition.EventDefinitionID, condition.EventCount }));
			}
		}
	}
}
