using System.Collections.Generic;

namespace PopForums.ScoringGame
{
	public interface IEventDefinitionService
	{
		EventDefinition GetEventDefinition(string eventDefinitionID);
		List<EventDefinition> GetAll();
		void Create(EventDefinition eventDefinition);
		void Delete(string eventDefinitionID);
	}
}