using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.ScoringGame;

namespace PopForums.Repositories
{
	public interface IAwardConditionRepository
	{
		Task<List<AwardCondition>> GetConditions(string awardDefinitionID);
		Task DeleteConditions(string awardDefinitionID);
		Task SaveConditions(List<AwardCondition> conditions);
		Task DeleteConditionsByEventDefinitionID(string eventDefinitionID);
		Task DeleteCondition(string awardDefinitionID, string eventDefinitionID);
	}
}
