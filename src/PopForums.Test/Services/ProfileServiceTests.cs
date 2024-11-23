namespace PopForums.Test.Services;

public class ProfileServiceTests
{
	private IProfileRepository _profileRepo;
	private ITextParsingService _textParsingService;
	private IPointLedgerRepository _pointLedger;

	private ProfileService GetService()
	{
		_profileRepo = Substitute.For<IProfileRepository>();
		_textParsingService = Substitute.For<ITextParsingService>();
		_pointLedger = Substitute.For<IPointLedgerRepository>();
		return new ProfileService(_profileRepo, _textParsingService, _pointLedger);
	}

	[Fact]
	public async Task GetProfile()
	{
		var service = GetService();
		var profile = new Profile { UserID = 123, Location = "Cleveland" };
		var user = UserServiceTests.GetDummyUser("Jeff", "a@b.com");
		_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(profile));
		var result = await service.GetProfile(user);
		Assert.Equal(profile, result);
		await _profileRepo.Received().GetProfile(user.UserID);
	}

	[Fact]
	public async Task GetProfileReturnsNullForNullUser()
	{
		var service = GetService();
		var result = await service.GetProfile(null);
		Assert.Null(result);
	}

	[Fact]
	public async Task GetProfileForEditParsesSigRichText()
	{
		var service = GetService();
		var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = "blah", IsPlainText = false };
		var user = UserServiceTests.GetDummyUser("Jeff", "a@b.com");
		_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(profile));
		_textParsingService.HtmlToClientHtml("blah").Returns("parsed");

		var result = await service.GetProfileForEdit(user);

		Assert.Equal("parsed", result.Signature);
	}

	[Fact]
	public async Task GetProfileForEditParsesSigPlainText()
	{
		var service = GetService();
		var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = "blah", IsPlainText = true };
		var user = UserServiceTests.GetDummyUser("Jeff", "a@b.com");
		_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(profile));
		_textParsingService.HtmlToForumCode("blah").Returns("parsed");

		var result = await service.GetProfileForEdit(user);

		Assert.Equal("parsed", result.Signature);
	}

	[Fact]
	public async Task EditUserProfilePlainText()
	{
		var service = GetService();
		var user = new User { UserID = 1 };
		user.Roles = new List<string>();
		var returnedProfile = new Profile { UserID = 1, IsPlainText = true };
		var profile = new Profile();
		_profileRepo.GetProfile(1).Returns(Task.FromResult(returnedProfile));
		await _profileRepo.Update(Arg.Do<Profile>(x => profile = x));
		_textParsingService.ForumCodeToHtml(Arg.Any<string>()).Returns("parsed");
		var userEdit = new UserEditProfile
		{
			Dob = new DateTime(2000, 1, 1),
			HideVanity = true,
			Instagram = "i",
			IsPlainText = true,
			IsSubscribed = true,
			Location = "l",
			Facebook = "fb",
			ShowDetails = true,
			Signature = "s",
			Web = "w",
			IsAutoFollowOnReply = true
		};

		await service.EditUserProfile(user, userEdit);

		await _profileRepo.Received().Update(Arg.Any<Profile>());
		Assert.Equal(new DateTime(2000, 1, 1), profile.Dob);
		Assert.True(profile.HideVanity);
		Assert.Equal("i", profile.Instagram);
		Assert.True(profile.IsPlainText);
		Assert.True(profile.IsSubscribed);
		Assert.True(profile.IsAutoFollowOnReply);
		Assert.Equal("l", profile.Location);
		Assert.Equal("fb", profile.Facebook);
		Assert.True(profile.ShowDetails);
		Assert.Equal("parsed", profile.Signature);
		Assert.Equal("w", profile.Web);
	}

	[Fact]
	public async Task EditUserProfileRichText()
	{
		var service = GetService();
		var user = new User { UserID = 1 };
		user.Roles = new List<string>();
		var returnedProfile = new Profile { UserID = 1, IsPlainText = false };
		var profile = new Profile();
		_profileRepo.GetProfile(1).Returns(Task.FromResult(returnedProfile));
		await _profileRepo.Update(Arg.Do<Profile>(x => profile = x));
		_textParsingService.ClientHtmlToHtml(Arg.Any<string>()).Returns("parsed");
		var userEdit = new UserEditProfile
		{
			Dob = new DateTime(2000, 1, 1),
			HideVanity = true,
			Instagram = "i",
			IsPlainText = true,
			IsSubscribed = true,
			Location = "l",
			Facebook = "fb",
			ShowDetails = true,
			Signature = "s",
			Web = "w"
		};

		await service.EditUserProfile(user, userEdit);

		Assert.Equal("parsed", profile.Signature);
	}

	[Fact]
	public async Task GetProfileForEditParsesSigGuardForNull()
	{
		var service = GetService();
		var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = null };
		var user = UserServiceTests.GetDummyUser("Jeff", "a@b.com");
		_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(profile));

		var result = await service.GetProfileForEdit(user);

		_textParsingService.DidNotReceive().ClientHtmlToForumCode(Arg.Any<string>());
		Assert.Equal(string.Empty, result.Signature);
		await _profileRepo.Received().GetProfile(user.UserID);
	}

	[Fact]
	public async Task CreateFromProfileObject()
	{
		var service = GetService();
		var profile = new Profile { UserID = 123, Location = "Cleveland" };
		await service.Create(profile);
		await _profileRepo.Received().Create(profile);
	}

	[Fact]
	public async Task CreateFromProfileThrowsWithoutUserID()
	{
		var service = GetService();
		var profile = new Profile();
		await Assert.ThrowsAsync<Exception>(() => service.Create(profile));
		await _profileRepo.DidNotReceive().Create(profile);
	}

	[Fact]
	public async Task Update()
	{
		var service = GetService();
		var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = ""};
		_profileRepo.Update(profile).Returns(Task.FromResult(true));
		await service.Update(profile);
		await _profileRepo.Received().Update(profile);
	}

	[Fact]
	public async Task UpdateTrimsSig()
	{
		var service = GetService();
		var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = " " };
		var trimProfile = new Profile { Signature = "no"};
		_profileRepo.Update(Arg.Do<Profile>(x => trimProfile = x)).Returns(Task.FromResult(true));
		await service.Update(profile);
		Assert.Equal("", trimProfile.Signature);
	}

	[Fact]
	public async Task UpdateThrowsWithNoProfile()
	{
		var service = GetService();
		var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = "" };
		_profileRepo.Update(profile).Returns(Task.FromResult(false));
		await Assert.ThrowsAsync<Exception>(() => service.Update(profile));
		await _profileRepo.Received().Update(profile);
	}

	[Fact]
	public async Task GetSigsOnlyTakesPostsWithShowSig()
	{
		var posts = new List<Post>
		{
			new Post { UserID = 1, ShowSig = false },
			new Post { UserID = 2, ShowSig = true },
			new Post { UserID = 3, ShowSig = false },
			new Post { UserID = 4, ShowSig = true },
			new Post { UserID = 5, ShowSig = true },
			new Post { UserID = 6, ShowSig = false },
		};
		var service = GetService();
		var ids = new List<int>();
		await _profileRepo.GetSignatures(Arg.Do<List<int>>(x => ids = x));
		await service.GetSignatures(posts);
		Assert.Equal(3, ids.Count);
		Assert.Equal(2, ids[0]);
		Assert.Equal(4, ids[1]);
		Assert.Equal(5, ids[2]);
	}

	[Fact]
	public async Task GetSigsDoesntSendDupeUserIDs()
	{
		var posts = new List<Post>
		{
			new Post { UserID = 1, ShowSig = false },
			new Post { UserID = 2, ShowSig = true },
			new Post { UserID = 2, ShowSig = false },
			new Post { UserID = 2, ShowSig = true },
			new Post { UserID = 3, ShowSig = true },
			new Post { UserID = 3, ShowSig = true },
		};
		var service = GetService();
		var ids = new List<int>();
		await _profileRepo.GetSignatures(Arg.Do<List<int>>(x => ids = x));
		await service.GetSignatures(posts);
		Assert.Equal(2, ids.Count);
		Assert.Equal(2, ids[0]);
		Assert.Equal(3, ids[1]);
	}

	[Fact]
	public async Task GetAvatarsDoesntSendDupeUserIDs()
	{
		var posts = new List<Post>
		{
			new Post { UserID = 1 },
			new Post { UserID = 2 },
			new Post { UserID = 2 },
			new Post { UserID = 2 },
			new Post { UserID = 3 },
			new Post { UserID = 3 },
		};
		var service = GetService();
		var ids = new List<int>();
		await _profileRepo.GetAvatars(Arg.Do<List<int>>(x => ids = x));
		await service.GetAvatars(posts);
		Assert.Equal(3, ids.Count);
		Assert.Equal(1, ids[0]);
		Assert.Equal(2, ids[1]);
		Assert.Equal(3, ids[2]);
	}

	[Fact]
	public async Task UpdatePointsUpdatesPoints()
	{
		var service = GetService();
		var user = new User { UserID = 123 };
		const int total = 87;
		_pointLedger.GetPointTotal(user.UserID).Returns(Task.FromResult(total));
		await service.UpdatePointTotal(user);
		await _profileRepo.Received().UpdatePoints(user.UserID, total);
	}
}