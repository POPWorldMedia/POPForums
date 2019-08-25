using System;
using System.Threading.Tasks;
using PopForums.Feeds;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.ScoringGame
{
	public interface IEventPublisher
	{
		Task ProcessEvent(string feedMessage, User user, string eventDefinitionID, bool overridePublishToActivityFeed);
		Task ProcessManualEvent(string feedMessage, User user, int pointValue);
	}

	public class EventPublisher : IEventPublisher
	{
		public EventPublisher(IEventDefinitionService eventDefinitionService, IPointLedgerRepository pointLedgerRepository, IFeedService feedService, IAwardCalculator awardCalculator, IProfileService profileService)
		{
			_eventDefinitionService = eventDefinitionService;
			_pointLedgerRepository = pointLedgerRepository;
			_feedService = feedService;
			_awardCalculator = awardCalculator;
			_profileService = profileService;
		}

		private readonly IEventDefinitionService _eventDefinitionService;
		private readonly IPointLedgerRepository _pointLedgerRepository;
		private readonly IFeedService _feedService;
		private readonly IAwardCalculator _awardCalculator;
		private readonly IProfileService _profileService;

		public async Task ProcessEvent(string feedMessage, User user, string eventDefinitionID, bool overridePublishToActivityFeed)
		{
			var timeStamp = DateTime.UtcNow;
			var eventDefinition = await _eventDefinitionService.GetEventDefinition(eventDefinitionID);
			var ledgerEntry = new PointLedgerEntry { UserID = user.UserID, EventDefinitionID = eventDefinitionID, Points = eventDefinition.PointValue, TimeStamp = timeStamp };
			await _pointLedgerRepository.RecordEntry(ledgerEntry);
			await _profileService.UpdatePointTotal(user);
			if (eventDefinition.IsPublishedToFeed && !overridePublishToActivityFeed)
			{
				await _feedService.PublishToFeed(user, feedMessage, eventDefinition.PointValue, timeStamp);
				_feedService.PublishToActivityFeed(feedMessage);
			}
			await _awardCalculator.QueueCalculation(user, eventDefinition);
		}

		public async Task ProcessManualEvent(string feedMessage, User user, int pointValue)
		{
			var timeStamp = DateTime.UtcNow;
			var eventDefinition = new EventDefinition { EventDefinitionID = "Manual", PointValue = pointValue };
			var ledgerEntry = new PointLedgerEntry { UserID = user.UserID, EventDefinitionID = eventDefinition.EventDefinitionID, Points = eventDefinition.PointValue, TimeStamp = timeStamp };
			await _pointLedgerRepository.RecordEntry(ledgerEntry);
			await _profileService.UpdatePointTotal(user);
			await _feedService.PublishToFeed(user, feedMessage, eventDefinition.PointValue, timeStamp);
		}
	}
}
