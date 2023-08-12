namespace PopForums.Test.ScoringGame;

public class EventPublisherTests
{
	private EventPublisher GetPublisher()
	{
		_eventDefService = Substitute.For<IEventDefinitionService>();
		_pointLedgerRepo = Substitute.For<IPointLedgerRepository>();
		_feedService = Substitute.For<IFeedService>();
		_awardCalc = Substitute.For<IAwardCalculator>();
		_profileService = Substitute.For<IProfileService>();
		return new EventPublisher(_eventDefService, _pointLedgerRepo, _feedService, _awardCalc, _profileService);
	}

	private IEventDefinitionService _eventDefService;
	private IPointLedgerRepository _pointLedgerRepo;
	private IFeedService _feedService;
	private IAwardCalculator _awardCalc;
	private IProfileService _profileService;

	[Fact]
	public async Task ProcessEventPublishesToLedger()
	{
		var user = new User { UserID = 123 };
		var eventDef = new EventDefinition {EventDefinitionID = "blah", PointValue = 42};
		const string message = "msg";
		var publisher = GetPublisher();
		_eventDefService.GetEventDefinition(eventDef.EventDefinitionID).Returns(Task.FromResult(eventDef));
		var entry = new PointLedgerEntry();
		await _pointLedgerRepo.RecordEntry(Arg.Do<PointLedgerEntry>(x => entry = x));
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
		_eventDefService.GetEventDefinition(eventDef.EventDefinitionID).Returns(Task.FromResult(eventDef));
		await publisher.ProcessEvent(message, user, eventDef.EventDefinitionID, false);
		await _feedService.Received().PublishToFeed(user, message, eventDef.PointValue, Arg.Any<DateTime>());
	}

	[Fact]
	public async Task ProcessEventDoesNotPublishToFeedServiceWhenEventDefSaysNo()
	{
		var user = new User { UserID = 123 };
		var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42, IsPublishedToFeed = false };
		const string message = "msg";
		var publisher = GetPublisher();
		_eventDefService.GetEventDefinition(eventDef.EventDefinitionID).Returns(Task.FromResult(eventDef));
		await publisher.ProcessEvent(message, user, eventDef.EventDefinitionID, false);
		await _feedService.DidNotReceive().PublishToFeed(user, message, eventDef.PointValue, Arg.Any<DateTime>());
	}

	[Fact]
	public async Task ProcessEventCallsCalculator()
	{
		var user = new User { UserID = 123 };
		var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42 };
		var publisher = GetPublisher();
		_eventDefService.GetEventDefinition(eventDef.EventDefinitionID).Returns(Task.FromResult(eventDef));
		await publisher.ProcessEvent("msg", user, eventDef.EventDefinitionID, false);
		await _awardCalc.Received().QueueCalculation(user, eventDef);
	}

	[Fact]
	public async Task ProcessEventUpdatesProfilePointTotal()
	{
		var user = new User { UserID = 123 };
		var eventDef = new EventDefinition { EventDefinitionID = "blah", PointValue = 42 };
		var publisher = GetPublisher();
		_eventDefService.GetEventDefinition(eventDef.EventDefinitionID).Returns(Task.FromResult(eventDef));
		await publisher.ProcessEvent("msg", user, eventDef.EventDefinitionID, false);
		await _profileService.Received().UpdatePointTotal(user);
	}

	[Fact]
	public async Task ProcessManualEventPublishesToLedger()
	{
		var user = new User { UserID = 123 };
		const string message = "msg";
		const int points = 252;
		var publisher = GetPublisher();
		var entry = new PointLedgerEntry();
		await _pointLedgerRepo.RecordEntry(Arg.Do<PointLedgerEntry>(x => entry = x));
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
		await _feedService.Received().PublishToFeed(user, message, points, Arg.Any<DateTime>());
	}

	[Fact]
	public async Task ProcessManualEventUpdatesProfilePointTotal()
	{
		var user = new User { UserID = 123 };
		var publisher = GetPublisher();
		await publisher.ProcessManualEvent("msg", user, 252);
		await _profileService.Received().UpdatePointTotal(user);
	}
}