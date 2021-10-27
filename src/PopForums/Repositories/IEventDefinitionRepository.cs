namespace PopForums.Repositories;

public interface IEventDefinitionRepository
{
	Task<EventDefinition> Get(string eventDefinitionID);
	Task<List<EventDefinition>> GetAll();
	Task Create(EventDefinition eventDefinition);
	void Delete(string eventDefinitionID);
}