using System;
using PopForums.Feeds;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.ScoringGame
{
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

		public void ProcessEvent(string feedMessage, User user, string eventDefinitionID, bool overridePublishToActivityFeed)
		{
			var timeStamp = DateTime.UtcNow;
			var eventDefinition = _eventDefinitionService.GetEventDefinition(eventDefinitionID);
			var ledgerEntry = new PointLedgerEntry { UserID = user.UserID, EventDefinitionID = eventDefinitionID, Points = eventDefinition.PointValue, TimeStamp = timeStamp };
			_pointLedgerRepository.RecordEntry(ledgerEntry);
			_profileService.UpdatePointTotal(user);
			if (eventDefinition.IsPublishedToFeed && !overridePublishToActivityFeed)
			{
				_feedService.PublishToFeed(user, feedMessage, eventDefinition.PointValue, timeStamp);
				_feedService.PublishToActivityFeed(feedMessage);
			}
			_awardCalculator.QueueCalculation(user, eventDefinition);
		}

		public void ProcessManualEvent(string feedMessage, User user, int pointValue)
		{
			var timeStamp = DateTime.UtcNow;
			var eventDefinition = new EventDefinition { EventDefinitionID = "Manual", PointValue = pointValue };
			var ledgerEntry = new PointLedgerEntry { UserID = user.UserID, EventDefinitionID = eventDefinition.EventDefinitionID, Points = eventDefinition.PointValue, TimeStamp = timeStamp };
			_pointLedgerRepository.RecordEntry(ledgerEntry);
			_profileService.UpdatePointTotal(user);
			_feedService.PublishToFeed(user, feedMessage, eventDefinition.PointValue, timeStamp);
		}
	}
}
