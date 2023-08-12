namespace PopForums.Test.ScoringGame;

public class AwardDefinitionServiceTests
{
	public AwardDefinitionService GetService()
	{
		_awardDefRepo = Substitute.For<IAwardDefinitionRepository>();
		_awardConditionRepo = Substitute.For<IAwardConditionRepository>();
		return new AwardDefinitionService(_awardDefRepo, _awardConditionRepo);
	}

	private IAwardDefinitionRepository _awardDefRepo;
	private IAwardConditionRepository _awardConditionRepo;

	[Fact]
	public void CreateMapsObjectToRepo()
	{
		var awardDef = new AwardDefinition {AwardDefinitionID = "blah", Title = "title", Description = "desc", IsSingleTimeAward = true};
		var service = GetService();
		service.Create(awardDef);
		_awardDefRepo.Received().Create(awardDef.AwardDefinitionID, awardDef.Title, awardDef.Description, awardDef.IsSingleTimeAward);
	}

	[Fact]
	public async Task SaveConditionsDeletesOldOnes()
	{
		var awardDef = new AwardDefinition {AwardDefinitionID = "awarddef"};
		var service = GetService();
		await service.SaveConditions(awardDef, new List<AwardCondition>());
		await _awardConditionRepo.Received().DeleteConditions(awardDef.AwardDefinitionID);
	}

	[Fact]
	public async Task SaveConditionsSetsAllAwardDefIDs()
	{
		var awardDef = new AwardDefinition { AwardDefinitionID = "awarddef" };
		var list = new List<AwardCondition> { new AwardCondition { AwardDefinitionID = "bad" }, new AwardCondition { AwardDefinitionID = "toobad" } };
		var savingList = new List<AwardCondition>();
		var service = GetService();
		await _awardConditionRepo.SaveConditions(Arg.Do<List<AwardCondition>>(x => savingList = x));
		await service.SaveConditions(awardDef, list);
		Assert.Equal(savingList[0].AwardDefinitionID, awardDef.AwardDefinitionID);
		Assert.Equal(savingList[1].AwardDefinitionID, awardDef.AwardDefinitionID);
	}
}