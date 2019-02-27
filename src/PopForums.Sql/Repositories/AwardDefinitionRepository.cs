using System.Collections.Generic;
using System.Linq;
using Dapper;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Sql.Repositories
{
	public class AwardDefinitionRepository : IAwardDefinitionRepository
	{
		public AwardDefinitionRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public AwardDefinition Get(string awardDefinitionID)
		{
			AwardDefinition awardDefinition = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				awardDefinition = connection.QuerySingleOrDefault<AwardDefinition>("SELECT AwardDefinitionID, Title, Description, IsSingleTimeAward FROM pf_AwardDefinition WHERE AwardDefinitionID = @AwardDefinitionID", new { AwardDefinitionID = awardDefinitionID }));
			return awardDefinition;
		}

		public List<AwardDefinition> GetAll()
		{
			var list = new List<AwardDefinition>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<AwardDefinition>("SELECT AwardDefinitionID, Title, Description, IsSingleTimeAward FROM pf_AwardDefinition ORDER BY AwardDefinitionID").ToList());
			return list;
		}

		public List<AwardDefinition> GetByEventDefinitionID(string eventDefinitionID)
		{
			var list = new List<AwardDefinition>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<AwardDefinition>("SELECT D.AwardDefinitionID, D.Title, D.Description, D.IsSingleTimeAward FROM pf_AwardDefinition D JOIN pf_AwardCondition C ON D.AwardDefinitionID = C.AwardDefinitionID WHERE C.EventDefinitionID = @EventDefinitionID", new { EventDefinitionID = eventDefinitionID }).ToList());
			return list;
		}

		public void Create(string awardDefinitionID, string title, string description, bool isSingleTimeAward)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_AwardDefinition (AwardDefinitionID, Title, Description, IsSingleTimeAward) VALUES (@AwardDefinitionID, @Title, @Description, @IsSingleTimeAward)", new { AwardDefinitionID = awardDefinitionID, Title = title, Description = description.NullToEmpty(), IsSingleTimeAward = isSingleTimeAward }));
		}

		public void Delete(string awardDefinitionID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_AwardDefinition WHERE AwardDefinitionID = @AwardDefinitionID", new { AwardDefinitionID = awardDefinitionID }));
		}
	}
}
