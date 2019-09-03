using System;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.ScoringGame
{
	public interface IAwardCalculator
	{
		Task QueueCalculation(User user, EventDefinition eventDefinition);
		Task ProcessCalculation(string eventDefinitionID, int userID);
	}

	public class AwardCalculator : IAwardCalculator
	{
		public AwardCalculator(IAwardCalculationQueueRepository awardCalcRepository, IEventDefinitionService eventDefinitionService, IUserRepository userRepository, IErrorLog errorLog, IAwardDefinitionService awardDefinitionService, IUserAwardService userAwardService, IPointLedgerRepository pointLedgerRepository, ITenantService tenantService)
		{
			_awardCalcRepository = awardCalcRepository;
			_eventDefinitionService = eventDefinitionService;
			_userRepository = userRepository;
			_errorLog = errorLog;
			_awardDefinitionService = awardDefinitionService;
			_userAwardService = userAwardService;
			_pointLedgerRepository = pointLedgerRepository;
			_tenantService = tenantService;
		}

		private readonly IAwardCalculationQueueRepository _awardCalcRepository;
		private readonly IEventDefinitionService _eventDefinitionService;
		private readonly IUserRepository _userRepository;
		private readonly IErrorLog _errorLog;
		private readonly IAwardDefinitionService _awardDefinitionService;
		private readonly IUserAwardService _userAwardService;
		private readonly IPointLedgerRepository _pointLedgerRepository;
		private readonly ITenantService _tenantService;

		public async Task QueueCalculation(User user, EventDefinition eventDefinition)
		{
			var tenantID = _tenantService.GetTenant();
			var payload = new AwardCalculationPayload {EventDefinitionID = eventDefinition.EventDefinitionID, UserID = user.UserID, TenantID = tenantID};
			await _awardCalcRepository.Enqueue(payload);
		}

		public async Task ProcessCalculation(string eventDefinitionID, int userID)
		{
			var eventDefinition = await _eventDefinitionService.GetEventDefinition(eventDefinitionID);
			var user = await _userRepository.GetUser(userID);
			if (eventDefinition == null)
			{
				_errorLog.Log(new Exception($"Event calculation attempt on nonexistent event \"{eventDefinitionID}\""), ErrorSeverity.Warning);
				return;
			}
			if (user == null)
			{
				_errorLog.Log(new Exception($"Event calculation attempt on nonexistent user {userID}"), ErrorSeverity.Warning);
				return;
			}
			var associatedAwards = await _awardDefinitionService.GetByEventDefinitionID(eventDefinition.EventDefinitionID);
			foreach (var award in associatedAwards)
			{
				if (award.IsSingleTimeAward)
				{
					var isAwarded = await _userAwardService.IsAwarded(user, award);
					if (isAwarded)
						continue;
				}
				var conditions = await _awardDefinitionService.GetConditions(award.AwardDefinitionID);
				var conditionsMet = 0;
				foreach (var condition in conditions)
				{
					var eventCount = await _pointLedgerRepository.GetEntryCount(user.UserID, condition.EventDefinitionID);
					if (eventCount >= condition.EventCount)
						conditionsMet++;
				}
				if (conditions.Count != 0 && conditionsMet == conditions.Count)
					await _userAwardService.IssueAward(user, award);
			}
		}
	}
}