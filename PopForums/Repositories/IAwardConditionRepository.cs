using System.Collections.Generic;
using PopForums.ScoringGame;

namespace PopForums.Repositories
{
	public interface IAwardConditionRepository
	{
		List<AwardCondition> GetConditions(string awardDefinitionID);
		void DeleteConditions(string awardDefinitionID);
		void SaveConditions(List<AwardCondition> conditions);
		void DeleteConditionsByEventDefinitionID(string eventDefinitionID);
		void DeleteCondition(string awardDefinitionID, string eventDefinitionID);
	}
}
