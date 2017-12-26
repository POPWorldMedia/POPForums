using System.Collections.Generic;
using PopForums.Data.Sql;
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
				connection.Command(_sqlObjectFactory, "SELECT AwardDefinitionID, EventDefinitionID, EventCount FROM pf_AwardCondition WHERE AwardDefinitionID = @AwardDefinitionID")
				.AddParameter(_sqlObjectFactory, "@AwardDefinitionID", awardDefinitionID)
				.ExecuteReader()
				.ReadAll(r => list.Add(new AwardCondition
					{
						AwardDefinitionID = r.GetString(0),
						EventDefinitionID = r.GetString(1),
						EventCount = r.GetInt32(2)
					})));
			return list;
		}

		public void DeleteConditions(string awardDefinitionID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_AwardCondition WHERE AwardDefinitionID = @AwardDefinitionID")
				.AddParameter(_sqlObjectFactory, "@AwardDefinitionID", awardDefinitionID)
				.ExecuteNonQuery());
		}

		public void DeleteCondition(string awardDefinitionID, string eventDefinitionID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_AwardCondition WHERE AwardDefinitionID = @AwardDefinitionID AND EventDefinitionID = @EventDefinitionID")
				.AddParameter(_sqlObjectFactory, "@AwardDefinitionID", awardDefinitionID)
				.AddParameter(_sqlObjectFactory, "@EventDefinitionID", eventDefinitionID)
				.ExecuteNonQuery());
		}

		public void DeleteConditionsByEventDefinitionID(string eventDefinitionID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_AwardCondition WHERE EventDefinitionID = @EventDefinitionID")
				.AddParameter(_sqlObjectFactory, "@EventDefinitionID", eventDefinitionID)
				.ExecuteNonQuery());
		}

		public void SaveConditions(List<AwardCondition> conditions)
		{
			foreach (var condition in conditions)
			{
				_sqlObjectFactory.GetConnection().Using(connection =>
					connection.Command(_sqlObjectFactory, "INSERT INTO pf_AwardCondition (AwardDefinitionID, EventDefinitionID, EventCount) VALUES (@AwardDefinitionID, @EventDefinitionID, @EventCount)")
					.AddParameter(_sqlObjectFactory, "@AwardDefinitionID", condition.AwardDefinitionID)
					.AddParameter(_sqlObjectFactory, "@EventDefinitionID", condition.EventDefinitionID)
					.AddParameter(_sqlObjectFactory, "@EventCount", condition.EventCount)
					.ExecuteNonQuery());
			}
		}
	}
}
