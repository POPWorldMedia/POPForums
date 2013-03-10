using System.Collections.Generic;
using PopForums.Repositories;

namespace PopForums.ScoringGame
{
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

		public List<AwardCondition> GetConditions(string awardDefinitionID)
		{
			return _awardConditionRepository.GetConditions(awardDefinitionID);
		}

		public void SaveConditions(AwardDefinition awardDefinition, List<AwardCondition> conditions)
		{
			_awardConditionRepository.DeleteConditions(awardDefinition.AwardDefinitionID);
			foreach (var condition in conditions)
				condition.AwardDefinitionID = awardDefinition.AwardDefinitionID;
			_awardConditionRepository.SaveConditions(conditions);
		}

		public void DeleteCondition(string awardDefinitionID, string eventDefinitionID)
		{
			_awardConditionRepository.DeleteCondition(awardDefinitionID, eventDefinitionID);
		}

		public void AddCondition(AwardCondition awardDefintion)
		{
			_awardConditionRepository.SaveConditions(new List<AwardCondition> {awardDefintion});
		}
	}
}
