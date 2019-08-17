using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Repositories;

namespace PopForums.ScoringGame
{
	public interface IAwardDefinitionService
	{
		AwardDefinition Get(string awardDefinitionID);
		List<AwardDefinition> GetByEventDefinitionID(string eventDefinitionID);
		void Create(AwardDefinition awardDefinition);
		void Delete(string awardDefinitionID);
		Task<List<AwardCondition>> GetConditions(string awardDefinitionID);
		Task SaveConditions(AwardDefinition awardDefinition, List<AwardCondition> conditions);
		List<AwardDefinition> GetAll();
		Task DeleteCondition(string awardDefinitionID, string eventDefinitionID);
		Task AddCondition(AwardCondition awardDefintion);
	}

	public class AwardDefinitionService : IAwardDefinitionService
	{
		public AwardDefinitionService(IAwardDefinitionRepository awardDefintionRepository, IAwardConditionRepository awardConditionRepository)
		{
			_awardDefinitionRepository = awardDefintionRepository;
			_awardConditionRepository = awardConditionRepository;
		}

		private readonly IAwardDefinitionRepository _awardDefinitionRepository;
		private readonly IAwardConditionRepository _awardConditionRepository;

		public AwardDefinition Get(string awardDefinitionID)
		{
			return _awardDefinitionRepository.Get(awardDefinitionID);
		}

		public List<AwardDefinition> GetAll()
		{
			return _awardDefinitionRepository.GetAll();
		}

		public List<AwardDefinition> GetByEventDefinitionID(string eventDefinitionID)
		{
			return _awardDefinitionRepository.GetByEventDefinitionID(eventDefinitionID);
		}

		public void Create(AwardDefinition awardDefinition)
		{
			_awardDefinitionRepository.Create(awardDefinition.AwardDefinitionID, awardDefinition.Title, awardDefinition.Description, awardDefinition.IsSingleTimeAward);
		}

		public void Delete(string awardDefinitionID)
		{
			_awardDefinitionRepository.Delete(awardDefinitionID);
		}

		public async Task<List<AwardCondition>> GetConditions(string awardDefinitionID)
		{
			return await _awardConditionRepository.GetConditions(awardDefinitionID);
		}

		public async Task SaveConditions(AwardDefinition awardDefinition, List<AwardCondition> conditions)
		{
			await _awardConditionRepository.DeleteConditions(awardDefinition.AwardDefinitionID);
			foreach (var condition in conditions)
				condition.AwardDefinitionID = awardDefinition.AwardDefinitionID;
			await _awardConditionRepository.SaveConditions(conditions);
		}

		public async Task DeleteCondition(string awardDefinitionID, string eventDefinitionID)
		{
			await _awardConditionRepository.DeleteCondition(awardDefinitionID, eventDefinitionID);
		}

		public async Task AddCondition(AwardCondition awardDefintion)
		{
			await _awardConditionRepository.SaveConditions(new List<AwardCondition> {awardDefintion});
		}
	}
}
