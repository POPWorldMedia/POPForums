namespace PopForums.Test.Services;

public class ProfileServiceTests
{
	private Mock<IProfileRepository> _profileRepo;
	private Mock<ITextParsingService> _textParsingService;
	private Mock<IPointLedgerRepository> _pointLedger;

	private ProfileService GetService()
	{
		_profileRepo = new Mock<IProfileRepository>();
		_textParsingService = new Mock<ITextParsingService>();
		_pointLedger = new Mock<IPointLedgerRepository>();
		return new ProfileService(_profileRepo.Object, _textParsingService.Object, _pointLedger.Object);
	}

	[Fact]
	public async Task GetProfile()
	{
		var service = GetService();
		var profile = new Profile { UserID = 123, Location = "Cleveland" };
		var user = UserServiceTests.GetDummyUser("Jeff", "a@b.com");
		_profileRepo.Setup(p => p.GetProfile(user.UserID)).ReturnsAsync(profile);
		var result = await service.GetProfile(user);
		Assert.Equal(profile, result);
		_profileRepo.Verify(p => p.GetProfile(user.UserID), Times.Once());
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
		_profileRepo.Setup(p => p.GetProfile(user.UserID)).ReturnsAsync(profile);
		_textParsingService.Setup(t => t.HtmlToClientHtml("blah")).Returns("parsed");

		var result = await service.GetProfileForEdit(user);

		Assert.Equal("parsed", result.Signature);
	}

	[Fact]
	public async Task GetProfileForEditParsesSigPlainText()
	{
		var service = GetService();
		var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = "blah", IsPlainText = true };
		var user = UserServiceTests.GetDummyUser("Jeff", "a@b.com");
		_profileRepo.Setup(p => p.GetProfile(user.UserID)).ReturnsAsync(profile);
		_textParsingService.Setup(t => t.HtmlToForumCode("blah")).Returns("parsed");

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
		_profileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(returnedProfile);
		_profileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
		_textParsingService.Setup(t => t.ForumCodeToHtml(It.IsAny<string>())).Returns("parsed");
		var userEdit = new UserEditProfile
		{
			Dob = new DateTime(2000, 1, 1),
			HideVanity = true,
			Instagram = "i",
			IsPlainText = true,
			IsSubscribed = true,
			Location = "l",
			Facebook = "fb",
			Twitter = "tw",
			ShowDetails = true,
			Signature = "s",
			Web = "w",
			IsAutoFollowOnReply = true
		};

		await service.EditUserProfile(user, userEdit);

		_profileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
		Assert.Equal(new DateTime(2000, 1, 1), profile.Dob);
		Assert.True(profile.HideVanity);
		Assert.Equal("i", profile.Instagram);
		Assert.True(profile.IsPlainText);
		Assert.True(profile.IsSubscribed);
		Assert.True(profile.IsAutoFollowOnReply);
		Assert.Equal("l", profile.Location);
		Assert.Equal("fb", profile.Facebook);
		Assert.Equal("tw", profile.Twitter);
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
		_profileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(returnedProfile);
		_profileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
		_textParsingService.Setup(t => t.ClientHtmlToHtml(It.IsAny<string>())).Returns("parsed");
		var userEdit = new UserEditProfile
		{
			Dob = new DateTime(2000, 1, 1),
			HideVanity = true,
			Instagram = "i",
			IsPlainText = true,
			IsSubscribed = true,
			Location = "l",
			Facebook = "fb",
			Twitter = "tw",
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
		_profileRepo.Setup(p => p.GetProfile(user.UserID)).ReturnsAsync(profile);

		var result = await service.GetProfileForEdit(user);

		_textParsingService.Verify(x => x.ClientHtmlToForumCode(It.IsAny<string>()), Times.Never);
		Assert.Equal(string.Empty, result.Signature);
		_profileRepo.Verify(p => p.GetProfile(user.UserID), Times.Once());
	}

	[Fact]
	public async Task CreateFromProfileObject()
	{
		var service = GetService();
		var profile = new Profile { UserID = 123, Location = "Cleveland" };
		_profileRepo.Setup(p => p.Create(profile));
		await service.Create(profile);
		_profileRepo.Verify(p => p.Create(profile), Times.Once());
	}

	[Fact]
	public async Task CreateFromProfileThrowsWithoutUserID()
	{
		var service = GetService();
		var profile = new Profile();
		await Assert.ThrowsAsync<Exception>(() => service.Create(profile));
		_profileRepo.Verify(p => p.Create(profile), Times.Never());
	}

	[Fact]
	public async Task Update()
	{
		var service = GetService();
		var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = ""};
		_profileRepo.Setup(p => p.Update(profile)).ReturnsAsync(true);
		await service.Update(profile);
		_profileRepo.Verify(p => p.Update(profile), Times.Once());
	}

	[Fact]
	public async Task UpdateTrimsSig()
	{
		var service = GetService();
		var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = " " };
		var trimProfile = new Profile { Signature = "no"};
		_profileRepo.Setup(p => p.Update(It.IsAny<Profile>())).ReturnsAsync(true).Callback<Profile>(p => trimProfile = p);
		await service.Update(profile);
		Assert.Equal("", trimProfile.Signature);
	}

	[Fact]
	public async Task UpdateThrowsWithNoProfile()
	{
		var service = GetService();
		var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = "" };
		_profileRepo.Setup(p => p.Update(profile)).ReturnsAsync(false);
		await Assert.ThrowsAsync<Exception>(() => service.Update(profile));
		_profileRepo.Verify(p => p.Update(profile), Times.Once());
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
		_profileRepo.Setup(p => p.GetSignatures(It.IsAny<List<int>>())).Callback<List<int>>(l => ids = l);
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
		_profileRepo.Setup(p => p.GetSignatures(It.IsAny<List<int>>())).Callback<List<int>>(l => ids = l);
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
		_profileRepo.Setup(p => p.GetAvatars(It.IsAny<List<int>>())).Callback<List<int>>(l => ids = l);
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
		_pointLedger.Setup(x => x.GetPointTotal(user.UserID)).ReturnsAsync(total);
		await service.UpdatePointTotal(user);
		_profileRepo.Verify(x => x.UpdatePoints(user.UserID, total), Times.Once());
	}
}