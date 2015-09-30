using System;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.ScoringGame
{
	public class AwardCalculator : IAwardCalculator
	{
		public AwardCalculator(IAwardCalculationQueueRepository awardCalcRepository, IEventDefinitionService eventDefinitionService, IUserRepository userRepository, IErrorLog errorLog, IAwardDefinitionService awardDefinitionService, IUserAwardService userAwardService, IPointLedgerRepository pointLedgerRepository)
		{
			_awardCalcRepository = awardCalcRepository;
			_eventDefinitionService = eventDefinitionService;
			_userRepository = userRepository;
			_errorLog = errorLog;
			_awardDefinitionService = awardDefinitionService;
			_userAwardService = userAwardService;
			_pointLedgerRepository = pointLedgerRepository;
		}

		private readonly IAwardCalculationQueueRepository _awardCalcRepository;
		private readonly IEventDefinitionService _eventDefinitionService;
		private readonly IUserRepository _userRepository;
		private readonly IErrorLog _errorLog;
		private readonly IAwardDefinitionService _awardDefinitionService;
		private readonly IUserAwardService _userAwardService;
		private readonly IPointLedgerRepository _pointLedgerRepository;

		public void QueueCalculation(User user, EventDefinition eventDefinition)
		{
			_awardCalcRepository.Enqueue(eventDefinition.EventDefinitionID, user.UserID);
		}

		public void ProcessOneCalculation()
		{
			var nextItem = _awardCalcRepository.Dequeue();
			if (String.IsNullOrEmpty(nextItem.Key))
				return;
			var eventDefinition = _eventDefinitionService.GetEventDefinition(nextItem.Key);
			var user = _userRepository.GetUser(nextItem.Value);
			if (eventDefinition == null)
			{
				_errorLog.Log(new Exception(String.Format("Event calculation attempt on nonexistent event \"{0}\"", nextItem.Key)), ErrorSeverity.Warning);
				return;
			}
			if (user == null)
			{
				_errorLog.Log(new Exception(String.Format("Event calculation attempt on nonexistent user {0}", nextItem.Value)), ErrorSeverity.Warning);
				return;
			}
			var associatedAwards = _awardDefinitionService.GetByEventDefinitionID(eventDefinition.EventDefinitionID);
			foreach (var award in associatedAwards)
			{
				if (award.IsSingleTimeAward)
				{
					var isAwarded = _userAwardService.IsAwarded(user, award);
					if (isAwarded)
						continue;
				}
				var conditions = _awardDefinitionService.GetConditions(award.AwardDefinitionID);
				var conditionsMet = 0;
				foreach (var condition in conditions)
				{
					var eventCount = _pointLedgerRepository.GetEntryCount(user.UserID, condition.EventDefinitionID);
					if (eventCount >= condition.EventCount)
						conditionsMet++;
				}
				if (conditions.Count != 0 && conditionsMet == conditions.Count)
					_userAwardService.IssueAward(user, award);
			}
		}
	}
}