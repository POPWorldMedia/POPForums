using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Data.SqlSingleWebServer.Repositories
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
				connection.Command("SELECT AwardDefinitionID, EventDefinitionID, EventCount FROM pf_AwardCondition WHERE AwardDefinitionID = @AwardDefinitionID")
				.AddParameter("@AwardDefinitionID", awardDefinitionID)
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
				connection.Command("DELETE FROM pf_AwardCondition WHERE AwardDefinitionID = @AwardDefinitionID")
				.AddParameter("@AwardDefinitionID", awardDefinitionID)
				.ExecuteNonQuery());
		}

		public void DeleteCondition(string awardDefinitionID, string eventDefinitionID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_AwardCondition WHERE AwardDefinitionID = @AwardDefinitionID AND EventDefinitionID = @EventDefinitionID")
				.AddParameter("@AwardDefinitionID", awardDefinitionID)
				.AddParameter("@EventDefinitionID", eventDefinitionID)
				.ExecuteNonQuery());
		}

		public void DeleteConditionsByEventDefinitionID(string eventDefinitionID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_AwardCondition WHERE EventDefinitionID = @EventDefinitionID")
				.AddParameter("@EventDefinitionID", eventDefinitionID)
				.ExecuteNonQuery());
		}

		public void SaveConditions(List<AwardCondition> conditions)
		{
			foreach (var condition in conditions)
			{
				_sqlObjectFactory.GetConnection().Using(connection =>
					connection.Command("INSERT INTO pf_AwardCondition (AwardDefinitionID, EventDefinitionID, EventCount) VALUES (@AwardDefinitionID, @EventDefinitionID, @EventCount)")
					.AddParameter("@AwardDefinitionID", condition.AwardDefinitionID)
					.AddParameter("@EventDefinitionID", condition.EventDefinitionID)
					.AddParameter("@EventCount", condition.EventCount)
					.ExecuteNonQuery());
			}
		}
	}
}
