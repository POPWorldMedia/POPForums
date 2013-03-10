using System;
using Moq;
using NUnit.Framework;
using PopForums.Feeds;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;
using PopForums.Services;

namespace PopForums.Test.ScoringGame
{
	[TestFixture]
	public class EventPublisherTests
	{
		private EventPublisher GetPublisher()
		{
			_eventDefService = new Mock<IEventDefinitionService>();
			_pointLedgerRepo = new Mock<IPointLedgerRepository>();
			_feedService = new Mock<IFeedService>();
			_awardCalc = new Mock<IAwardCalculator>();
			_profileService = new Mock<IProfileService>();
			return new EventPublisher(_eventDefService.Object, _pointLedgerRepo.Object, _feedService.Object, _awardCalc.Object, _profileService.Object);
		}

		private Mock<IEventDefinitionService> _eventDefService;
		private Mock<IPointLedgerRepository> _pointLedgerRepo;
		private Mock<IFeedService> _feedService;
		private Mock<IAwardCalculator> _awardCalc;
		private Mock<IProfileService> _profileService;

		[Test]
		public void ProcessEventPublishesToLedger()
		{
			var user = new User(123, DateTime.MinValue);
			var eventDef = new EventDefinition {EventDefinitionID = "blah", PointValue = 42};
			const string message = "msg";
			var publisher = GetPublisher();
			_eventDefService.Setup(x => x.GetEventDefinition(eventDef.EventDefinitionID)).Returns(eventDef);
			var entry = new PointLedgerEntry();
			_pointLedgerRepo.Setup(x => x.RecordEntry(It.IsAny<PointLedgerEntry>())).Callback<PointLedgerEntry>(x => entry = x);
			publisher.ProcessEvent(message, user, eventDef.EventDefinitionID, false);
			Assert.AreEqual(user.UserID, entry.UserID);
			Assert.AreEqual(eventDef.EventDefinitionID, entry.EventDefinitionID);
			Assert.AreEqual(eventDef.PointValue, entry.Points);
		}

		[Test]
		public void ProcessEventPublishesToFeedService()
		{
			var user = new User(123, DateTime.MinValue);
			var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42, IsPublishedToFeed = true };
			const string message = "msg";
			var publisher = GetPublisher();
			_eventDefService.Setup(x => x.GetEventDefinition(eventDef.EventDefinitionID)).Returns(eventDef);
			publisher.ProcessEvent(message, user, eventDef.EventDefinitionID, false);
			_feedService.Verify(x => x.PublishToFeed(user, message, eventDef.PointValue, It.IsAny<DateTime>()), Times.Once());
		}

		[Test]
		public void ProcessEventPublishesToFeedServiceForActivity()
		{
			var user = new User(123, DateTime.MinValue);
			var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42, IsPublishedToFeed = true };
			const string message = "msg";
			var publisher = GetPublisher();
			_eventDefService.Setup(x => x.GetEventDefinition(eventDef.EventDefinitionID)).Returns(eventDef);
			publisher.ProcessEvent(message, user, eventDef.EventDefinitionID, false);
			_feedService.Verify(x => x.PublishToActivityFeed(message), Times.Once());
		}

		[Test]
		public void ProcessEventDoesNotPublishToFeedServiceForActivityWhenEventDefSaysNo()
		{
			var user = new User(123, DateTime.MinValue);
			var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42, IsPublishedToFeed = false };
			const string message = "msg";
			var publisher = GetPublisher();
			_eventDefService.Setup(x => x.GetEventDefinition(eventDef.EventDefinitionID)).Returns(eventDef);
			publisher.ProcessEvent(message, user, eventDef.EventDefinitionID, false);
			_feedService.Verify(x => x.PublishToActivityFeed(message), Times.Never());
		}

		[Test]
		public void ProcessEventDoesNotPublishToFeedServiceWhenEventDefSaysNo()
		{
			var user = new User(123, DateTime.MinValue);
			var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42, IsPublishedToFeed = false };
			const string message = "msg";
			var publisher = GetPublisher();
			_eventDefService.Setup(x => x.GetEventDefinition(eventDef.EventDefinitionID)).Returns(eventDef);
			publisher.ProcessEvent(message, user, eventDef.EventDefinitionID, false);
			_feedService.Verify(x => x.PublishToFeed(user, message, eventDef.PointValue, It.IsAny<DateTime>()), Times.Never());
		}

		[Test]
		public void ProcessEventCallsCalculator()
		{
			var user = new User(123, DateTime.MinValue);
			var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42 };
			var publisher = GetPublisher();
			_eventDefService.Setup(x => x.GetEventDefinition(eventDef.EventDefinitionID)).Returns(eventDef);
			publisher.ProcessEvent("msg", user, eventDef.EventDefinitionID, false);
			_awardCalc.Verify(x => x.QueueCalculation(user, eventDef), Times.Once());
		}

		[Test]
		public void ProcessEventUpdatesProfilePointTotal()
		{
			var user = new User(123, DateTime.MinValue);
			var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42 };
			var publisher = GetPublisher();
			_eventDefService.Setup(x => x.GetEventDefinition(eventDef.EventDefinitionID)).Returns(eventDef);
			publisher.ProcessEvent("msg", user, eventDef.EventDefinitionID, false);
			_profileService.Verify(x => x.UpdatePointTotal(user), Times.Once());
		}

		[Test]
		public void ProcessManualEventPublishesToLedger()
		{
			var user = new User(123, DateTime.MinValue);
			const string message = "msg";
			const int points = 252;
			var publisher = GetPublisher();
			var entry = new PointLedgerEntry();
			_pointLedgerRepo.Setup(x => x.RecordEntry(It.IsAny<PointLedgerEntry>())).Callback<PointLedgerEntry>(x => entry = x);
			publisher.ProcessManualEvent(message, user, points);
			Assert.AreEqual(user.UserID, entry.UserID);
			Assert.AreEqual("Manual", entry.EventDefinitionID);
			Assert.AreEqual(points, entry.Points);
		}

		[Test]
		public void ProcessManualEventPublishesToFeedService()
		{
			var user = new User(123, DateTime.MinValue);
			const string message = "msg";
			const int points = 252;
			var publisher = GetPublisher();
			publisher.ProcessManualEvent(message, user, points);
			_feedService.Verify(x => x.PublishToFeed(user, message, points, It.IsAny<DateTime>()), Times.Once());
		}

		[Test]
		public void ProcessManualEventUpdatesProfilePointTotal()
		{
			var user = new User(123, DateTime.MinValue);
			var publisher = GetPublisher();
			publisher.ProcessManualEvent("msg", user, 252);
			_profileService.Verify(x => x.UpdatePointTotal(user), Times.Once());
		}
	}
}
