using System;
using System.Threading.Tasks;
using Moq;
using PopForums.Feeds;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;
using PopForums.Services;
using Xunit;

namespace PopForums.Test.ScoringGame
{
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

		[Fact]
		public async Task ProcessEventPublishesToLedger()
		{
			var user = new User { UserID = 123 };
			var eventDef = new EventDefinition {EventDefinitionID = "blah", PointValue = 42};
			const string message = "msg";
			var publisher = GetPublisher();
			_eventDefService.Setup(x => x.GetEventDefinition(eventDef.EventDefinitionID)).ReturnsAsync(eventDef);
			var entry = new PointLedgerEntry();
			_pointLedgerRepo.Setup(x => x.RecordEntry(It.IsAny<PointLedgerEntry>())).Callback<PointLedgerEntry>(x => entry = x);
			await publisher.ProcessEvent(message, user, eventDef.EventDefinitionID, false);
			Assert.Equal(user.UserID, entry.UserID);
			Assert.Equal(eventDef.EventDefinitionID, entry.EventDefinitionID);
			Assert.Equal(eventDef.PointValue, entry.Points);
		}

		[Fact]
		public async Task ProcessEventPublishesToFeedService()
		{
			var user = new User { UserID = 123 };
			var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42, IsPublishedToFeed = true };
			const string message = "msg";
			var publisher = GetPublisher();
			_eventDefService.Setup(x => x.GetEventDefinition(eventDef.EventDefinitionID)).ReturnsAsync(eventDef);
			await publisher.ProcessEvent(message, user, eventDef.EventDefinitionID, false);
			_feedService.Verify(x => x.PublishToFeed(user, message, eventDef.PointValue, It.IsAny<DateTime>()), Times.Once());
		}

		[Fact]
		public async Task ProcessEventPublishesToFeedServiceForActivity()
		{
			var user = new User { UserID = 123 };
			var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42, IsPublishedToFeed = true };
			const string message = "msg";
			var publisher = GetPublisher();
			_eventDefService.Setup(x => x.GetEventDefinition(eventDef.EventDefinitionID)).ReturnsAsync(eventDef);
			await publisher.ProcessEvent(message, user, eventDef.EventDefinitionID, false);
			_feedService.Verify(x => x.PublishToActivityFeed(message), Times.Once());
		}

		[Fact]
		public async Task ProcessEventDoesNotPublishToFeedServiceForActivityWhenEventDefSaysNo()
		{
			var user = new User { UserID = 123 };
			var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42, IsPublishedToFeed = false };
			const string message = "msg";
			var publisher = GetPublisher();
			_eventDefService.Setup(x => x.GetEventDefinition(eventDef.EventDefinitionID)).ReturnsAsync(eventDef);
			await publisher.ProcessEvent(message, user, eventDef.EventDefinitionID, false);
			_feedService.Verify(x => x.PublishToActivityFeed(message), Times.Never());
		}

		[Fact]
		public async Task ProcessEventDoesNotPublishToFeedServiceWhenEventDefSaysNo()
		{
			var user = new User { UserID = 123 };
			var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42, IsPublishedToFeed = false };
			const string message = "msg";
			var publisher = GetPublisher();
			_eventDefService.Setup(x => x.GetEventDefinition(eventDef.EventDefinitionID)).ReturnsAsync(eventDef);
			await publisher.ProcessEvent(message, user, eventDef.EventDefinitionID, false);
			_feedService.Verify(x => x.PublishToFeed(user, message, eventDef.PointValue, It.IsAny<DateTime>()), Times.Never());
		}

		[Fact]
		public async Task ProcessEventCallsCalculator()
		{
			var user = new User { UserID = 123 };
			var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42 };
			var publisher = GetPublisher();
			_eventDefService.Setup(x => x.GetEventDefinition(eventDef.EventDefinitionID)).ReturnsAsync(eventDef);
			await publisher.ProcessEvent("msg", user, eventDef.EventDefinitionID, false);
			_awardCalc.Verify(x => x.QueueCalculation(user, eventDef), Times.Once());
		}

		[Fact]
		public async Task ProcessEventUpdatesProfilePointTotal()
		{
			var user = new User { UserID = 123 };
			var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42 };
			var publisher = GetPublisher();
			_eventDefService.Setup(x => x.GetEventDefinition(eventDef.EventDefinitionID)).ReturnsAsync(eventDef);
			await publisher.ProcessEvent("msg", user, eventDef.EventDefinitionID, false);
			_profileService.Verify(x => x.UpdatePointTotal(user), Times.Once());
		}

		[Fact]
		public async Task ProcessManualEventPublishesToLedger()
		{
			var user = new User { UserID = 123 };
			const string message = "msg";
			const int points = 252;
			var publisher = GetPublisher();
			var entry = new PointLedgerEntry();
			_pointLedgerRepo.Setup(x => x.RecordEntry(It.IsAny<PointLedgerEntry>())).Callback<PointLedgerEntry>(x => entry = x);
			await publisher.ProcessManualEvent(message, user, points);
			Assert.Equal(user.UserID, entry.UserID);
			Assert.Equal("Manual", entry.EventDefinitionID);
			Assert.Equal(points, entry.Points);
		}

		[Fact]
		public async Task ProcessManualEventPublishesToFeedService()
		{
			var user = new User { UserID = 123 };
			const string message = "msg";
			const int points = 252;
			var publisher = GetPublisher();
			await publisher.ProcessManualEvent(message, user, points);
			_feedService.Verify(x => x.PublishToFeed(user, message, points, It.IsAny<DateTime>()), Times.Once());
		}

		[Fact]
		public async Task ProcessManualEventUpdatesProfilePointTotal()
		{
			var user = new User { UserID = 123 };
			var publisher = GetPublisher();
			await publisher.ProcessManualEvent("msg", user, 252);
			_profileService.Verify(x => x.UpdatePointTotal(user), Times.Once());
		}
	}
}
