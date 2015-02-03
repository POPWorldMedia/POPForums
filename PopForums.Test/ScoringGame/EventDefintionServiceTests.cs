using Moq;
using NUnit.Framework;
using PopForums.Repositories;
using PopForums.ScoringGame;
using System.Collections.Generic;
using System.Linq;

namespace PopForums.Test.ScoringGame
{
	[TestFixture]
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

		[Test]
		public void GetReturnsFromRepo()
		{
			var service = GetService();
			var def = new EventDefinition {EventDefinitionID = "whatevs", PointValue = 2, Description = "stuff"};
			_eventDefRepo.Setup(x => x.Get(def.EventDefinitionID)).Returns(def);
			var result = service.GetEventDefinition(def.EventDefinitionID);
			Assert.AreSame(def, result);
		}

		[Test]
		public void GetStaticPostVoteReturnsStaticObject()
		{
			var service = GetService();
			var result = service.GetEventDefinition(EventDefinitionService.StaticEventIDs.PostVote);
			Assert.AreSame(EventDefinitionService.StaticEvents[EventDefinitionService.StaticEventIDs.PostVote], result);
		}

		[Test]
		public void GetAllMergesStaticWithRepo()
		{
			var service = GetService();
			var list = new List<EventDefinition> {new EventDefinition {EventDefinitionID = "aaa"}, new EventDefinition {EventDefinitionID = "zzz"}};
			_eventDefRepo.Setup(x => x.GetAll()).Returns(list);
			var result = service.GetAll();
			Assert.AreEqual(6, result.Count);
			Assert.IsTrue(result.Where(x => x.EventDefinitionID == "aaa").Count() == 1);
			Assert.IsTrue(result.Where(x => x.EventDefinitionID == "zzz").Count() == 1);
			Assert.IsTrue(result.Where(x => x.EventDefinitionID == EventDefinitionService.StaticEventIDs.NewPost).Count() == 1);
			Assert.IsTrue(result.Where(x => x.EventDefinitionID == EventDefinitionService.StaticEventIDs.NewTopic).Count() == 1);
			Assert.IsTrue(result.Where(x => x.EventDefinitionID == EventDefinitionService.StaticEventIDs.PostVote).Count() == 1);
		}

		[Test]
		public void GetAllMergesAndOrders()
		{
			var service = GetService();
			var list = new List<EventDefinition> { new EventDefinition { EventDefinitionID = "aaa" }, new EventDefinition { EventDefinitionID = "zzz" } };
			_eventDefRepo.Setup(x => x.GetAll()).Returns(list);
			var result = service.GetAll();
			Assert.AreEqual(6, result.Count);
			Assert.AreEqual("aaa", result[0].EventDefinitionID);
			Assert.AreEqual(EventDefinitionService.StaticEventIDs.NewPost, result[1].EventDefinitionID);
			Assert.AreEqual(EventDefinitionService.StaticEventIDs.NewTopic, result[2].EventDefinitionID);
			Assert.AreEqual(EventDefinitionService.StaticEventIDs.PostVote, result[3].EventDefinitionID);
			Assert.AreEqual("zzz", result[5].EventDefinitionID);
		}

		[Test]
		public void CreatePassesToRepo()
		{
			var service = GetService();
			var eventDef = new EventDefinition();
			service.Create(eventDef);
			_eventDefRepo.Verify(x => x.Create(eventDef), Times.Once());
		}

		[Test]
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
