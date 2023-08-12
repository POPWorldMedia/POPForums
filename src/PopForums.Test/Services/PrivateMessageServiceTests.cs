namespace PopForums.Test.Services;

public class PrivateMessageServiceTests
{
	private PrivateMessageService GetService()
	{
		_mockPMRepo = Substitute.For<IPrivateMessageRepository>();
		_mockSettings = Substitute.For<ISettingsManager>();
		_mockTextParse = Substitute.For<ITextParsingService>();
		_mockBroker = Substitute.For<IBroker>();
		var service = new PrivateMessageService(_mockPMRepo, _mockSettings, _mockTextParse, _mockBroker);
		return service;
	}

	private IPrivateMessageRepository _mockPMRepo;
	private ISettingsManager _mockSettings;
	private ITextParsingService _mockTextParse;
	private IBroker _mockBroker;

	[Fact]
	public async Task CreateNullTextThrows()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentNullException>(() => service.Create(null, new User(), new List<User> { new User() }));
	}

	[Fact]
	public async Task CreateEmptyTextThrows()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentNullException>(() => service.Create(String.Empty, new User(), new List<User> { new User() }));
	}

	[Fact]
	public async Task CreateNullUserThrows()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentNullException>(() => service.Create("oho h", null, new List<User> { new User() }));
	}

	[Fact]
	public async Task CreateNullToUsersThrows()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentException>(() => service.Create("oho h", new User(), null));
	}

	[Fact]
	public async Task CreateZeroToUsersThrows()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentException>(() => service.Create("oho h", new User(), new List<User>()));
	}

	[Fact]
	public async Task CreateSerializedUser()
	{
		var service = GetService();
		var pm = await service.Create("oihefio", new User { UserID = 12, Name = "jeff"}, new List<User> {new User { UserID = 45, Name = "diana"}});
		Assert.Equal(45, pm.Users[0].GetProperty("userID").GetInt32());
		Assert.Equal("diana", pm.Users[0].GetProperty("name").GetString());
	}

	[Fact]
	public async Task CreateSerializedUsers()
	{
		var service = GetService();
		var pm = await service.Create("oihefio", new User { UserID = 12, Name = "jeff" }, new List<User> { new User { UserID = 45, Name = "diana" }, new User { UserID = 78, Name = "simon" } });
		Assert.Equal(45, pm.Users[0].GetProperty("userID").GetInt32());
		Assert.Equal("diana", pm.Users[0].GetProperty("name").GetString());
		Assert.Equal(78, pm.Users[1].GetProperty("userID").GetInt32());
		Assert.Equal("simon", pm.Users[1].GetProperty("name").GetString());
	}

	[Fact]
	public async Task CreateCallsNotificationBroker()
	{
		var service = GetService();
		_mockPMRepo.GetUnreadCount(45).Returns(Task.FromResult(3));

		var pm = await service.Create("oihefio", new User { UserID = 12, Name = "jeff" }, new List<User> { new User { UserID = 45, Name = "diana" } });

		_mockBroker.Received().NotifyPMCount(45, 3);
	}

	[Fact]
	public async Task CreatePMPersistedIDReturned()
	{
		var service = GetService();
		var persist = new PrivateMessage();
		_mockPMRepo.CreatePrivateMessage(Arg.Do<PrivateMessage>(x => persist = x)).Returns(Task.FromResult(69));
		_mockTextParse.EscapeHtmlAndCensor("ohqefwwf").Returns("ohqefwwf");
		var pm = await service.Create("oihefio", new User { UserID = 12, Name = "jeff" }, new List<User> { new User { UserID = 45, Name = "diana" }, new User { UserID = 67, Name = "simon"} });
		Assert.Equal(69, pm.PMID);
	}

	[Fact]
	public async Task CreateAllUsersPresisted()
	{
		var user = new User { UserID = 12 };
		var to1 = new User { UserID = 45 };
		var to2 = new User { UserID = 67 };
		var service = GetService();
		var users = new List<int>();
		var originalUser = new List<int>();
		_mockPMRepo.CreatePrivateMessage(Arg.Any<PrivateMessage>()).Returns(Task.FromResult(69));
		await _mockPMRepo.AddUsers(Arg.Any<int>(), Arg.Do<List<int>>(x => originalUser = x), Arg.Any<DateTime>(), false);
		await _mockPMRepo.AddUsers(Arg.Any<int>(), Arg.Do<List<int>>(x => users = x), Arg.Any<DateTime>(), false);

		await service.Create("oihefio", user, new List<User> { to1, to2 });

		Assert.Equal(2, users.Count);
		Assert.Equal(to1.UserID, users[0]);
		Assert.Equal(to2.UserID, users[1]);
		// TODO: figure out multiple setups with same parameters
		//Assert.Equal(user.UserID, originalUser[0]);
	}

	[Fact]
	public async Task CreatePostPersist()
	{
		var user = new User { UserID = 12, Name = "jeff" };
		var to1 = new User { UserID = 45 };
		var to2 = new User { UserID = 67 };
		var service = GetService();
		_mockPMRepo.CreatePrivateMessage(Arg.Any<PrivateMessage>()).Returns(Task.FromResult(69));
		var post = new PrivateMessagePost();
		await _mockPMRepo.AddPost(Arg.Do<PrivateMessagePost>(x => post = x));
		_mockTextParse.ForumCodeToHtml("oihefio").Returns("oihefio");
		await service.Create("oihefio", user, new List<User> { to1, to2 });
		Assert.Equal("oihefio", post.FullText);
		Assert.Equal("jeff", post.Name);
		Assert.Equal(69, post.PMID);
		Assert.Equal(user.UserID, post.UserID);
	}

	[Fact]
	public async Task ReplyNullPMThrows()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentException>(() => service.Reply(null, "ohifwefhi", new User()));
	}

	[Fact]
	public async Task ReplyNoIdPMThrows()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentException>(() => service.Reply(new PrivateMessage(), "ohifwefhi", new User()));
	}

	[Fact]
	public async Task ReplyNullTextThrows()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentNullException>(() => service.Reply(new PrivateMessage{ PMID = 2 }, null, new User()));
	}

	[Fact]
	public async Task ReplyEmptyTextThrows()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentNullException>(() => service.Reply(new PrivateMessage { PMID = 2 }, String.Empty, new User()));
	}

	[Fact]
	public async Task ReplyNullUserThrows()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentNullException>(() => service.Reply(new PrivateMessage { PMID = 2 }, "wfwgrg", null));
	}

	[Fact]
	public async Task ReplyMapsAndPresistsPost()
	{
		var service = GetService();
		var post = new PrivateMessagePost();
		await _mockPMRepo.AddPost(Arg.Do<PrivateMessagePost>(x => post = x));
		var user = new User { UserID = 1, Name = "jeff"};
		var pm = new PrivateMessage {PMID = 2};
		var text = "mah message";
		_mockTextParse.ForumCodeToHtml(text).Returns(text);
		_mockPMRepo.GetUsers(pm.PMID).Returns(Task.FromResult(new List<PrivateMessageUser> {new PrivateMessageUser {UserID = user.UserID}}));
		_mockPMRepo.GetUnreadCount(user.UserID).Returns(Task.FromResult(42));

		await service.Reply(pm, text, user);

		Assert.Equal(text, post.FullText);
		Assert.Equal(user.Name, post.Name);
		Assert.Equal(user.UserID, post.UserID);
		Assert.Equal(pm.PMID, post.PMID);
		_mockBroker.Received().NotifyPMCount(user.UserID, 42);
	}

	[Fact]
	public async Task ReplyThrowsIfUserIsntOnPM()
	{
		var service = GetService();
		var user = new User { UserID = 1 };
		_mockPMRepo.GetUsers(Arg.Any<int>()).Returns(Task.FromResult(new List<PrivateMessageUser> { new PrivateMessageUser { UserID = 456 } }));
		await Assert.ThrowsAsync<Exception>(() => service.Reply(new PrivateMessage { PMID = 2 }, "wohfwo", user));
	}

	[Fact]
	public async Task IsUserInPMTrue()
	{
		var service = GetService();
		var user = new User { UserID = 1 };
		var pm = new PrivateMessage { PMID = 2 };
		_mockPMRepo.GetUsers(pm.PMID).Returns(Task.FromResult(new List<PrivateMessageUser> { new PrivateMessageUser { UserID = user.UserID } }));
		Assert.True(await service.IsUserInPM(user.UserID, pm.PMID));
	}

	[Fact]
	public async Task IsUserInPMFalse()
	{
		var service = GetService();
		var user = new User { UserID = 1 };
		var pm = new PrivateMessage { PMID = 2 };
		_mockPMRepo.GetUsers(pm.PMID).Returns(Task.FromResult(new List<PrivateMessageUser> { new PrivateMessageUser { UserID = 765 } }));
		Assert.False(await service.IsUserInPM(user.UserID, pm.PMID));
	}
}