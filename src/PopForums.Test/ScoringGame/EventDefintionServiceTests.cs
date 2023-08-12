namespace PopForums.Test.ScoringGame;

public class EventDefintionServiceTests
{
	private EventDefinitionService GetService()
	{
		_eventDefRepo = Substitute.For<IEventDefinitionRepository>();
		_awardConditionRepo = Substitute.For<IAwardConditionRepository>();
		return new EventDefinitionService(_eventDefRepo, _awardConditionRepo);
	}

	private IEventDefinitionRepository _eventDefRepo;
	private IAwardConditionRepository _awardConditionRepo;

	[Fact]
	public async Task GetReturnsFromRepo()
	{
		var service = GetService();
		var def = new EventDefinition {EventDefinitionID = "whatevs", PointValue = 2, Description = "stuff"};
		_eventDefRepo.Get(def.EventDefinitionID).Returns(Task.FromResult(def));
		var result = await service.GetEventDefinition(def.EventDefinitionID);
		Assert.Same(def, result);
	}

	[Fact]
	public async Task GetStaticPostVoteReturnsStaticObject()
	{
		var service = GetService();
		var result = await service.GetEventDefinition(EventDefinitionService.StaticEventIDs.PostVote);
		Assert.Same(EventDefinitionService.StaticEvents[EventDefinitionService.StaticEventIDs.PostVote], result);
	}

	[Fact]
	public async Task GetAllMergesStaticWithRepo()
	{
		var service = GetService();
		var list = new List<EventDefinition> {new() {EventDefinitionID = "AAA"}, new() {EventDefinitionID = "ZZZ"}};
		_eventDefRepo.GetAll().Returns(Task.FromResult(list));
		var result = await service.GetAll();
		Assert.Equal(7, result.Count);
		Assert.True(result.Count(x => x.EventDefinitionID == "AAA") == 1);
		Assert.True(result.Count(x => x.EventDefinitionID == "ZZZ") == 1);
		Assert.True(result.Count(x => x.EventDefinitionID == EventDefinitionService.StaticEventIDs.NewPost) == 1);
		Assert.True(result.Count(x => x.EventDefinitionID == EventDefinitionService.StaticEventIDs.NewTopic) == 1);
		Assert.True(result.Count(x => x.EventDefinitionID == EventDefinitionService.StaticEventIDs.PostVote) == 1);
	}
	
	[Fact]
	public async Task GetAllMergesAndOrders()
	{
		var service = GetService();
		var list = new List<EventDefinition> { new() { EventDefinitionID = "AAA" }, new() { EventDefinitionID = "ZZZ" } };
		_eventDefRepo.GetAll().Returns(Task.FromResult(list));
		var result = await service.GetAll();
		Assert.Equal(7, result.Count);
		Assert.Equal("AAA", result[0].EventDefinitionID);
		Assert.Equal(EventDefinitionService.StaticEventIDs.NewPost, result[1].EventDefinitionID);
		Assert.Equal(EventDefinitionService.StaticEventIDs.NewTopic, result[2].EventDefinitionID);
		Assert.Equal(EventDefinitionService.StaticEventIDs.PostVote, result[3].EventDefinitionID);
		Assert.Equal(EventDefinitionService.StaticEventIDs.PostVoteUndo, result[4].EventDefinitionID);
		Assert.Equal("ZZZ", result[6].EventDefinitionID);
	}

	[Fact]
	public async Task CreatePassesToRepo()
	{
		var service = GetService();
		var eventDef = new EventDefinition();
		await service.Create(eventDef);
		await _eventDefRepo.Received().Create(eventDef);
	}

	[Fact]
	public async Task DeleteCallsEventDefRepoAndAwardConditionRepo()
	{
		var service = GetService();
		const string eventDefID = "ohnoes!";
		await service.Delete(eventDefID);
		_eventDefRepo.Received().Delete(eventDefID);
		await _awardConditionRepo.Received().DeleteConditionsByEventDefinitionID(eventDefID);
	}
}