namespace PopForums.Test.ScoringGame;

public class UserAwardServiceTests
{
	public UserAwardService GetService()
	{
		_userAwardRepo = Substitute.For<IUserAwardRepository>();
		_notificationTunnel = Substitute.For<INotificationTunnel>();
		_tenantService = Substitute.For<ITenantService>();
		return new UserAwardService(_userAwardRepo, _notificationTunnel, _tenantService);
	}

	private IUserAwardRepository _userAwardRepo;
	private INotificationTunnel _notificationTunnel;
	private ITenantService _tenantService;

	[Fact]
	public async Task IssueMapsFieldsToRepoCall()
	{
		var user = new User { UserID = 123 };
		var awardDef = new AwardDefinition {AwardDefinitionID = "blah", Description = "desc", Title = "title", IsSingleTimeAward = true};
		var service = GetService();
		await service.IssueAward(user, awardDef);
		await _userAwardRepo.Received().IssueAward(user.UserID, awardDef.AwardDefinitionID, awardDef.Title, awardDef.Description, Arg.Any<DateTime>());
	}

	[Fact]
	public async Task IsAwardedMapsAndReturnsRightValue()
	{
		var user = new User { UserID = 123 };
		var awardDef = new AwardDefinition { AwardDefinitionID = "blah" };
		var service = GetService();
		_userAwardRepo.IsAwarded(user.UserID, awardDef.AwardDefinitionID).Returns(Task.FromResult(true));
		var result = await service.IsAwarded(user, awardDef);
		Assert.True(result);
	}

	[Fact]
	public async Task GetAwardsMapsUserIDAndReturnsList()
	{
		var user = new User { UserID = 123 };
		var list = new List<UserAward>();
		var service = GetService();
		_userAwardRepo.GetAwards(user.UserID).Returns(Task.FromResult(list));
		var result = await service.GetAwards(user);
		Assert.Same(list, result);
	}
}