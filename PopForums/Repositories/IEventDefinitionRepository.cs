using System.Collections.Generic;
using PopForums.ScoringGame;

namespace PopForums.Repositories
{
	public interface IEventDefinitionRepository
	{
		EventDefinition Get(string eventDefinitionID);
		List<EventDefinition> GetAll();
		void Create(EventDefinition eventDefinition);
		void Delete(string eventDefinitionID);
	}
}
