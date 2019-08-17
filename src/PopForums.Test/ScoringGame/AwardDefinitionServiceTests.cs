using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using PopForums.Repositories;
using PopForums.ScoringGame;
using Xunit;

namespace PopForums.Test.ScoringGame
{
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

		[Fact]
		public void CreateMapsObjectToRepo()
		{
			var awardDef = new AwardDefinition {AwardDefinitionID = "blah", Title = "title", Description = "desc", IsSingleTimeAward = true};
			var service = GetService();
			service.Create(awardDef);
			_awardDefRepo.Verify(x => x.Create(awardDef.AwardDefinitionID, awardDef.Title, awardDef.Description, awardDef.IsSingleTimeAward), Times.Once());
		}

		[Fact]
		public async Task SaveConditionsDeletesOldOnes()
		{
			var awardDef = new AwardDefinition {AwardDefinitionID = "awarddef"};
			var service = GetService();
			await service.SaveConditions(awardDef, new List<AwardCondition>());
			_awardConditionRepo.Verify(x => x.DeleteConditions(awardDef.AwardDefinitionID), Times.Once());
		}

		[Fact]
		public async Task SaveConditionsSetsAllAwardDefIDs()
		{
			var awardDef = new AwardDefinition { AwardDefinitionID = "awarddef" };
			var list = new List<AwardCondition> { new AwardCondition { AwardDefinitionID = "bad" }, new AwardCondition { AwardDefinitionID = "toobad" } };
			var savingList = new List<AwardCondition>();
			var service = GetService();
			_awardConditionRepo.Setup(x => x.SaveConditions(It.IsAny<List<AwardCondition>>())).Callback<List<AwardCondition>>(x => savingList = x);
			await service.SaveConditions(awardDef, list);
			Assert.Equal(savingList[0].AwardDefinitionID, awardDef.AwardDefinitionID);
			Assert.Equal(savingList[1].AwardDefinitionID, awardDef.AwardDefinitionID);
		}
	}
}
