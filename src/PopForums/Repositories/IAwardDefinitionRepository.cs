using System.Collections.Generic;
using PopForums.ScoringGame;

namespace PopForums.Repositories
{
	public interface IAwardDefinitionRepository
	{
		AwardDefinition Get(string awardDefinitionID);
		List<AwardDefinition> GetAll();
		List<AwardDefinition> GetByEventDefinitionID(string eventDefinitionID);
		void Create(string awardDefinitionID, string title, string description, bool isSingleTimeAward);
		void Delete(string awardDefinitionID);
	}
}
