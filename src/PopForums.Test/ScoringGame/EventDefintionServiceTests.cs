using Moq;
using PopForums.Repositories;
using PopForums.ScoringGame;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PopForums.Test.ScoringGame
{
	public class EventDefintionServiceTests
	{
		private EventDefinitionService GetService()
		{
			_eventDefRepo = new Mock<IEventDefinitionRepository>();
			_awardConditionRepo = new Mock<IAwardConditionRepository>();
			return new EventDefinitionService(_eventDefRepo.Object, _awardConditionRepo.Object);
		}

		private Mock<IEventDefinitionRepository> _eventDefRepo;
		private Mock<IAwardConditionRepository> _awardConditionRepo;

		[Fact]
		public void GetReturnsFromRepo()
		{
			var service = GetService();
			var def = new EventDefinition {EventDefinitionID = "whatevs", PointValue = 2, Description = "stuff"};
			_eventDefRepo.Setup(x => x.Get(def.EventDefinitionID)).Returns(def);
			var result = service.GetEventDefinition(def.EventDefinitionID);
			Assert.Same(def, result);
		}

		[Fact]
		public void GetStaticPostVoteReturnsStaticObject()
		{
			var service = GetService();
			var result = service.GetEventDefinition(EventDefinitionService.StaticEventIDs.PostVote);
			Assert.Same(EventDefinitionService.StaticEvents[EventDefinitionService.StaticEventIDs.PostVote], result);
		}

		[Fact]
		public void GetAllMergesStaticWithRepo()
		{
			var service = GetService();
			var list = new List<EventDefinition> {new EventDefinition {EventDefinitionID = "aaa"}, new EventDefinition {EventDefinitionID = "zzz"}};
			_eventDefRepo.Setup(x => x.GetAll()).Returns(list);
			var result = service.GetAll();
			Assert.Equal(6, result.Count);
			Assert.True(result.Count(x => x.EventDefinitionID == "aaa") == 1);
			Assert.True(result.Where(x => x.EventDefinitionID == "zzz").Count() == 1);
			Assert.True(result.Where(x => x.EventDefinitionID == EventDefinitionService.StaticEventIDs.NewPost).Count() == 1);
			Assert.True(result.Where(x => x.EventDefinitionID == EventDefinitionService.StaticEventIDs.NewTopic).Count() == 1);
			Assert.True(result.Where(x => x.EventDefinitionID == EventDefinitionService.StaticEventIDs.PostVote).Count() == 1);
		}

		[Fact]
		public void GetAllMergesAndOrders()
		{
			var service = GetService();
			var list = new List<EventDefinition> { new EventDefinition { EventDefinitionID = "aaa" }, new EventDefinition { EventDefinitionID = "zzz" } };
			_eventDefRepo.Setup(x => x.GetAll()).Returns(list);
			var result = service.GetAll();
			Assert.Equal(6, result.Count);
			Assert.Equal("aaa", result[0].EventDefinitionID);
			Assert.Equal(EventDefinitionService.StaticEventIDs.NewPost, result[1].EventDefinitionID);
			Assert.Equal(EventDefinitionService.StaticEventIDs.NewTopic, result[2].EventDefinitionID);
			Assert.Equal(EventDefinitionService.StaticEventIDs.PostVote, result[3].EventDefinitionID);
			Assert.Equal("zzz", result[5].EventDefinitionID);
		}

		[Fact]
		public void CreatePassesToRepo()
		{
			var service = GetService();
			var eventDef = new EventDefinition();
			service.Create(eventDef);
			_eventDefRepo.Verify(x => x.Create(eventDef), Times.Once());
		}

		[Fact]
		public void DeleteCallsEventDefRepoAndAwardConditionRepo()
		{
			var service = GetService();
			const string eventDefID = "ohnoes!";
			service.Delete(eventDefID);
			_eventDefRepo.Verify(x => x.Delete(eventDefID), Times.Once());
			_awardConditionRepo.Verify(x => x.DeleteConditionsByEventDefinitionID(eventDefID), Times.Once());
		}
	}
}
