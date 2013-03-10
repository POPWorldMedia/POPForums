using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Test.ScoringGame
{
	[TestFixture]
	public class AwardDefinitionServiceTests
	{
		public AwardDefinitionService GetService()
		{
			_awardDefRepo = new Mock<IAwardDefinitionRepository>();
			_awardConditionRepo = new Mock<IAwardConditionRepository>();
			return new AwardDefinitionService(_awardDefRepo.Object, _awardConditionRepo.Object);
		}

		private Mock<IAwardDefinitionRepository> _awardDefRepo;
		private Mock<IAwardConditionRepository> _awardConditionRepo;

		[Test]
		public void CreateMapsObjectToRepo()
		{
			var awardDef = new AwardDefinition {AwardDefinitionID = "blah", Title = "title", Description = "desc", IsSingleTimeAward = true};
			var service = GetService();
			service.Create(awardDef);
			_awardDefRepo.Verify(x => x.Create(awardDef.AwardDefinitionID, awardDef.Title, awardDef.Description, awardDef.IsSingleTimeAward), Times.Once());
		}

		[Test]
		public void SaveConditionsDeletesOldOnes()
		{
			var awardDef = new AwardDefinition {AwardDefinitionID = "awarddef"};
			var service = GetService();
			service.SaveConditions(awardDef, new List<AwardCondition>());
			_awardConditionRepo.Verify(x => x.DeleteConditions(awardDef.AwardDefinitionID), Times.Once());
		}

		[Test]
		public void SaveConditionsSetsAllAwardDefIDs()
		{
			var awardDef = new AwardDefinition { AwardDefinitionID = "awarddef" };
			var list = new List<AwardCondition> { new AwardCondition { AwardDefinitionID = "bad" }, new AwardCondition { AwardDefinitionID = "toobad" } };
			var savingList = new List<AwardCondition>();
			var service = GetService();
			_awardConditionRepo.Setup(x => x.SaveConditions(It.IsAny<List<AwardCondition>>())).Callback<List<AwardCondition>>(x => savingList = x);
			service.SaveConditions(awardDef, list);
			Assert.AreEqual(savingList[0].AwardDefinitionID, awardDef.AwardDefinitionID);
			Assert.AreEqual(savingList[1].AwardDefinitionID, awardDef.AwardDefinitionID);
		}
	}
}
