using System.Collections.Generic;

namespace PopForums.ScoringGame
{
	public interface IAwardDefinitionService
	{
		AwardDefinition Get(string awardDefinitionID);
		List<AwardDefinition> GetByEventDefinitionID(string eventDefinitionID);
		void Create(AwardDefinition awardDefinition);
		void Delete(string awardDefinitionID);
		List<AwardCondition> GetConditions(string awardDefinitionID);
		void SaveConditions(AwardDefinition awardDefinition, List<AwardCondition> conditions);
		List<AwardDefinition> GetAll();
		void DeleteCondition(string awardDefinitionID, string eventDefinitionID);
		void AddCondition(AwardCondition awardDefintion);
	}
}