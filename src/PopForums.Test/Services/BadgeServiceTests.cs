namespace PopForums.Test.Services;

public class BadgeServiceTests
{
	private IBadgeRepository _mockBadgeRepo;
	private IUserService _mockUserService;

	private BadgeService GetService()
	{
		_mockBadgeRepo = Substitute.For<IBadgeRepository>();
		_mockUserService = Substitute.For<IUserService>();
		return new BadgeService(_mockBadgeRepo, _mockUserService);
	}

	[Fact]
	public async Task SaveNewBadgeCreatesAndSetsID()
	{
		var service = GetService();
		var badge = new Badge { Name = "Boss", Role = "Admin", ForegroundColor = "FFFFFF", BackgroundColor = "00aa00" };
		_mockBadgeRepo.Create(badge).Returns(Task.FromResult(42));

		var result = await service.Save(badge);

		Assert.True(result.IsSuccessful);
		Assert.Equal(42, result.Data.BadgeID);
		Assert.Equal("ffffff", badge.ForegroundColor);
		await _mockBadgeRepo.DidNotReceive().Update(Arg.Any<Badge>());
	}

	[Fact]
	public async Task SaveExistingBadgeUpdates()
	{
		var service = GetService();
		var badge = new Badge { BadgeID = 3, Name = "Boss", Role = "Admin", ForegroundColor = "ffffff", BackgroundColor = "000000" };

		var result = await service.Save(badge);

		Assert.True(result.IsSuccessful);
		await _mockBadgeRepo.Received().Update(badge);
		await _mockBadgeRepo.DidNotReceive().Create(Arg.Any<Badge>());
	}

	[Theory]
	[InlineData("", "Admin", "ffffff", "000000")]
	[InlineData("Boss", "", "ffffff", "000000")]
	[InlineData("Boss", "Admin", "#ffffff", "000000")]
	[InlineData("Boss", "Admin", "fff", "000000")]
	[InlineData("Boss", "Admin", "ffffff", "gggggg")]
	[InlineData("Boss", "Admin", "ffffff", null)]
	public async Task SaveRejectsInvalidBadges(string name, string role, string foreground, string background)
	{
		var service = GetService();
		var badge = new Badge { Name = name, Role = role, ForegroundColor = foreground, BackgroundColor = background };

		var result = await service.Save(badge);

		Assert.False(result.IsSuccessful);
		await _mockBadgeRepo.DidNotReceive().Create(Arg.Any<Badge>());
		await _mockBadgeRepo.DidNotReceive().Update(Arg.Any<Badge>());
	}

	[Fact]
	public async Task GetBadgesMatchesEffectiveRoles()
	{
		var service = GetService();
		var boss = new Badge { BadgeID = 1, Role = "Admin" };
		var supporter = new Badge { BadgeID = 2, Role = "Subscriber" };
		var annual = new Badge { BadgeID = 3, Role = "annual" };
		_mockBadgeRepo.GetAll().Returns(Task.FromResult(new List<Badge> { boss, supporter, annual }));
		var userIDs = new List<int> { 1, 2, 3 };
		_mockUserService.GetEffectiveRoles(userIDs).Returns(Task.FromResult(new Dictionary<int, List<string>>
		{
			{ 1, new List<string> { "Admin", "Subscriber", "annual" } },
			{ 2, new List<string> { "Moderator" } },
			{ 3, new List<string> { "Subscriber" } }
		}));

		var result = await service.GetBadges(userIDs);

		Assert.Equal(new[] { boss, supporter, annual }, result[1]);
		Assert.False(result.ContainsKey(2));
		Assert.Equal(new[] { supporter }, result[3]);
	}

	[Fact]
	public async Task GetBadgesFromPostsUsesDistinctUserIDs()
	{
		var service = GetService();
		_mockBadgeRepo.GetAll().Returns(Task.FromResult(new List<Badge>()));
		_mockUserService.GetEffectiveRoles(Arg.Any<List<int>>()).Returns(Task.FromResult(new Dictionary<int, List<string>>()));
		var posts = new List<Post> { new Post { UserID = 1 }, new Post { UserID = 2 }, new Post { UserID = 1 } };

		await service.GetBadges(posts);

		await _mockUserService.Received().GetEffectiveRoles(Arg.Is<List<int>>(x => x.SequenceEqual(new[] { 1, 2 })));
	}

	[Fact]
	public async Task GetBadgesForUserWithNoBadgesIsEmpty()
	{
		var service = GetService();
		_mockBadgeRepo.GetAll().Returns(Task.FromResult(new List<Badge> { new Badge { Role = "Admin" } }));
		_mockUserService.GetEffectiveRoles(Arg.Any<List<int>>()).Returns(Task.FromResult(new Dictionary<int, List<string>> { { 5, new List<string>() } }));

		var result = await service.GetBadges(5);

		Assert.Empty(result);
	}
}
