using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		public async Task<AwardDefinition> Get(string awardDefinitionID)
		{
			Task<AwardDefinition> awardDefinition = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				awardDefinition = connection.QuerySingleOrDefaultAsync<AwardDefinition>("SELECT AwardDefinitionID, Title, Description, IsSingleTimeAward FROM pf_AwardDefinition WHERE AwardDefinitionID = @AwardDefinitionID", new { AwardDefinitionID = awardDefinitionID }));
			return await awardDefinition;
		}

		public async Task<List<AwardDefinition>> GetAll()
		{
			Task<IEnumerable<AwardDefinition>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QueryAsync<AwardDefinition>("SELECT AwardDefinitionID, Title, Description, IsSingleTimeAward FROM pf_AwardDefinition ORDER BY AwardDefinitionID"));
			var list = result.Result.ToList();
			return list;
		}

		public async Task<List<AwardDefinition>> GetByEventDefinitionID(string eventDefinitionID)
		{
			Task<IEnumerable<AwardDefinition>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QueryAsync<AwardDefinition>("SELECT D.AwardDefinitionID, D.Title, D.Description, D.IsSingleTimeAward FROM pf_AwardDefinition D JOIN pf_AwardCondition C ON D.AwardDefinitionID = C.AwardDefinitionID WHERE C.EventDefinitionID = @EventDefinitionID", new { EventDefinitionID = eventDefinitionID }));
			var list = result.Result.ToList();
			return list;
		}

		public async Task Create(string awardDefinitionID, string title, string description, bool isSingleTimeAward)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_AwardDefinition (AwardDefinitionID, Title, Description, IsSingleTimeAward) VALUES (@AwardDefinitionID, @Title, @Description, @IsSingleTimeAward)", new { AwardDefinitionID = awardDefinitionID, Title = title, Description = description.NullToEmpty(), IsSingleTimeAward = isSingleTimeAward }));
		}

		public async Task Delete(string awardDefinitionID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_AwardDefinition WHERE AwardDefinitionID = @AwardDefinitionID", new { AwardDefinitionID = awardDefinitionID }));
		}
	}
}
