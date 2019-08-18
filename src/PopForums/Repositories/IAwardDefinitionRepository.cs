using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.ScoringGame;

namespace PopForums.Repositories
{
	public interface IAwardDefinitionRepository
	{
		Task<AwardDefinition> Get(string awardDefinitionID);
		Task<List<AwardDefinition>> GetAll();
		Task<List<AwardDefinition>> GetByEventDefinitionID(string eventDefinitionID);
		Task Create(string awardDefinitionID, string title, string description, bool isSingleTimeAward);
		Task Delete(string awardDefinitionID);
	}
}
