using System.Collections.Generic;
using PopForums.Data.Sql;
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
			    connection.Command(_sqlObjectFactory, "SELECT AwardDefinitionID, Title, Description, IsSingleTimeAward FROM pf_AwardDefinition WHERE AwardDefinitionID = @AwardDefinitionID")
			    .AddParameter(_sqlObjectFactory, "@AwardDefinitionID", awardDefinitionID)
			    .ExecuteReader()
			    .ReadOne(r => awardDefinition = new AwardDefinition
			        {
						AwardDefinitionID = r.GetString(0),
						Title = r.GetString(1),
						Description = r.GetString(2),
						IsSingleTimeAward = r.GetBoolean(3)
			        }));
			return awardDefinition;
		}

		public List<AwardDefinition> GetAll()
		{
			var list = new List<AwardDefinition>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "SELECT AwardDefinitionID, Title, Description, IsSingleTimeAward FROM pf_AwardDefinition ORDER BY AwardDefinitionID")
				.ExecuteReader()
				.ReadAll(r => list.Add(new AwardDefinition
				{
					AwardDefinitionID = r.GetString(0),
					Title = r.GetString(1),
					Description = r.GetString(2),
					IsSingleTimeAward = r.GetBoolean(3)
				})));
			return list;
		}

		public List<AwardDefinition> GetByEventDefinitionID(string eventDefinitionID)
		{
			var list = new List<AwardDefinition>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "SELECT D.AwardDefinitionID, D.Title, D.Description, D.IsSingleTimeAward FROM pf_AwardDefinition D JOIN pf_AwardCondition C ON D.AwardDefinitionID = C.AwardDefinitionID WHERE C.EventDefinitionID = @EventDefinitionID")
				.AddParameter(_sqlObjectFactory, "@EventDefinitionID", eventDefinitionID)
				.ExecuteReader()
				.ReadAll(r => list.Add(new AwardDefinition
				{
					AwardDefinitionID = r.GetString(0),
					Title = r.GetString(1),
					Description = r.GetString(2),
					IsSingleTimeAward = r.GetBoolean(3)
				})));
			return list;
		}

		public void Create(string awardDefinitionID, string title, string description, bool isSingleTimeAward)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "INSERT INTO pf_AwardDefinition (AwardDefinitionID, Title, Description, IsSingleTimeAward) VALUES (@AwardDefinitionID, @Title, @Description, @IsSingleTimeAward)")
				.AddParameter(_sqlObjectFactory, "@AwardDefinitionID", awardDefinitionID)
				.AddParameter(_sqlObjectFactory, "@Title", title)
				.AddParameter(_sqlObjectFactory, "@Description", description.NullToEmpty())
				.AddParameter(_sqlObjectFactory, "@IsSingleTimeAward", isSingleTimeAward)
				.ExecuteNonQuery());
		}

		public void Delete(string awardDefinitionID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_AwardDefinition WHERE AwardDefinitionID = @AwardDefinitionID")
				.AddParameter(_sqlObjectFactory, "@AwardDefinitionID", awardDefinitionID)
				.ExecuteNonQuery());
		}
	}
}
