namespace PopForums.Test.Services;

public class UserServiceTests
{
	private IUserRepository _mockUserRepo;
	private IRoleRepository _mockRoleRepo;
	private IProfileRepository _mockProfileRepo;
	private ISettingsManager _mockSettingsManager;
	private IUserAvatarRepository _mockUserAvatarRepo;
	private IUserImageRepository _mockUserImageRepo;
	private ISecurityLogService _mockSecurityLogService;
	private ITextParsingService _mockTextParser;
	private IBanRepository _mockBanRepo;
	private IForgotPasswordMailer _mockForgotMailer;
	private IImageService _mockImageService;
	private IConfig _config;

	private UserService GetMockedUserService()
	{
		_mockUserRepo = Substitute.For<IUserRepository>();
		_mockRoleRepo = Substitute.For<IRoleRepository>();
		_mockProfileRepo = Substitute.For<IProfileRepository>();
		_mockSettingsManager = Substitute.For<ISettingsManager>();
		_mockUserAvatarRepo = Substitute.For<IUserAvatarRepository>();
		_mockUserImageRepo = Substitute.For<IUserImageRepository>();
		_mockSecurityLogService = Substitute.For<ISecurityLogService>();
		_mockTextParser = Substitute.For<ITextParsingService>();
		_mockBanRepo = Substitute.For<IBanRepository>();
		_mockForgotMailer = Substitute.For<IForgotPasswordMailer>();
		_mockImageService = Substitute.For<IImageService>();
		_config = Substitute.For<IConfig>();
		_mockRoleRepo.GetUserRoles(Arg.Any<int>()).Returns(Task.FromResult(new List<string>()));
		return new UserService(_mockUserRepo, _mockRoleRepo, _mockProfileRepo, _mockSettingsManager, _mockUserAvatarRepo, _mockUserImageRepo, _mockSecurityLogService, _mockTextParser, _mockBanRepo, _mockForgotMailer, _mockImageService, _config);
	}

	[Fact]
	public async Task SetPassword()
	{
		var userService = GetMockedUserService();
		var user = GetDummyUser("jeff", "a@b.com");
		var salt = Guid.NewGuid();
		await _mockUserRepo.SetHashedPassword(user, Arg.Any<string>(), Arg.Do<Guid>(x => salt = x));

		await userService.SetPassword(user, "fred", String.Empty, user);

		var hashedPassword = "fred".GetSHA256Hash(salt);
		await _mockUserRepo.Received().SetHashedPassword(user, hashedPassword, salt);
	}

	[Fact]
	public void CheckPassword()
	{
		var userService = GetMockedUserService();
		_mockUserRepo.GetHashedPasswordByEmail(string.Empty).Returns(Task.FromResult(Tuple.Create("0M/C5TGbgs3HGjOHPoJsk9fuETY/iskcT6Oiz80ihuU=", Arg.Any<Guid?>())));

		Assert.True(userService.CheckPassword(string.Empty, "fred").Result.Item1);
	}

	[Fact]
	public void CheckPasswordFail()
	{
		var userService = GetMockedUserService();
		_mockUserRepo.GetHashedPasswordByEmail(string.Empty).Returns(Task.FromResult(Tuple.Create("VwqQv7+MfqtdxdTiaDLVsQ==", Arg.Any<Guid?>())));

		var result = userService.CheckPassword(String.Empty, "fsdfsdfsdfsdf").Result.Item1;

		Assert.False(result);
	}

	[Fact]
	public void CheckPasswordHasSalt()
	{
		var userService = GetMockedUserService();
		Guid? salt = Guid.NewGuid();
		var hashedPassword = "fred".GetSHA256Hash(salt.Value);
		_mockUserRepo.GetHashedPasswordByEmail(string.Empty).Returns(Task.FromResult(Tuple.Create(hashedPassword, salt)));

		Assert.True(userService.CheckPassword(String.Empty, "fred").Result.Item1);
	}

	[Fact]
	public void CheckPasswordHasSaltFail()
	{
		var userService = GetMockedUserService();
		Guid? salt = Guid.NewGuid();
		var hashedPassword = "fred".GetSHA256Hash(salt.Value);
		_mockUserRepo.GetHashedPasswordByEmail(string.Empty).Returns(Task.FromResult(Tuple.Create(hashedPassword, salt)));

		Assert.False(userService.CheckPassword(String.Empty, "dsfsdfsdfsdf").Result.Item1);
	}

	[Fact]
	public void CheckPasswordPassesWithoutSaltOnMD5Fallback()
	{
		var userService = GetMockedUserService();
		Guid? salt = null;
		var hashedPassword = "fred".GetMD5Hash();
		_mockUserRepo.GetHashedPasswordByEmail(string.Empty).Returns(Task.FromResult(Tuple.Create(hashedPassword, salt)));

		Assert.True(userService.CheckPassword(String.Empty, "fred").Result.Item1);
	}

	[Fact]
	public void CheckPasswordPassesWithSaltOnMD5Fallback()
	{
		var userService = GetMockedUserService();
		Guid? salt = Guid.NewGuid();
		var hashedPassword = "fred".GetMD5Hash(salt.Value);
		_mockUserRepo.GetHashedPasswordByEmail(string.Empty).Returns(Task.FromResult(Tuple.Create(hashedPassword, salt)));

		Assert.True(userService.CheckPassword(String.Empty, "fred").Result.Item1);
	}

	[Fact]
	public void CheckPasswordFailsOnMD5FallbackNoMatch()
	{
		var userService = GetMockedUserService();
		Guid? salt = Guid.NewGuid();
		var hashedPassword = "fred".GetMD5Hash(salt.Value);
		_mockUserRepo.GetHashedPasswordByEmail(string.Empty).Returns(Task.FromResult(Tuple.Create(hashedPassword, salt)));

		Assert.False(userService.CheckPassword(String.Empty, "blah").Result.Item1);
	}

	[Fact]
	public void CheckPasswordFailsMD5FallbackDoesNotCallUpdate()
	{
		var userService = GetMockedUserService();
		Guid? salt = Guid.NewGuid();
		var email = "a@b.com";
		var user = new User { Email = email };
		var hashedPassword = "fred".GetMD5Hash(salt.Value);
		_mockUserRepo.GetHashedPasswordByEmail(email).Returns(Task.FromResult(Tuple.Create(hashedPassword, salt)));
		_mockUserRepo.GetUserByEmail(email).Returns(Task.FromResult(user));

		Assert.False(userService.CheckPassword(email, "abc").Result.Item1);
		_mockUserRepo.DidNotReceive().SetHashedPassword(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<Guid>());
	}

	[Fact]
	public void CheckPasswordPassesWithSaltOnMD5FallbackCallsUpdate()
	{
		var userService = GetMockedUserService();
		Guid? salt = Guid.NewGuid();
		var email = "a@b.com";
		var user = new User {Email = email};
		var hashedPassword = "fred".GetMD5Hash(salt.Value);
		_mockUserRepo.GetHashedPasswordByEmail(email).Returns(Task.FromResult(Tuple.Create(hashedPassword, salt)));
		_mockUserRepo.GetUserByEmail(email).Returns(Task.FromResult(user));

		Assert.True(userService.CheckPassword(email, "fred").Result.Item1);
		_mockUserRepo.Received().SetHashedPassword(user, Arg.Any<string>(), Arg.Any<Guid>());
	}

	[Fact]
	public async Task GetUser()
	{
		const int id = 1;
		const string name = "Jeff";
		const string email = "a@b.com";
		var roles = new List<string> {"blah", PermanentRoles.Admin};
		var userService = GetMockedUserService();
		var dummyUser = GetDummyUser(name, email);
		_mockUserRepo.GetUser(id).Returns(Task.FromResult(dummyUser));
		_mockRoleRepo.GetUserRoles(id).Returns(Task.FromResult(roles));
		var user = await userService.GetUser(id);
		Assert.Same(dummyUser, user);
	}

	[Fact]
	public async Task GetUserFail()
	{
		const int id = 1;
		var userService = GetMockedUserService();
		_mockUserRepo.GetUser(Arg.Is<int>(i => i != 1)).Returns(Task.FromResult(GetDummyUser("", "")));
		_mockUserRepo.GetUser(Arg.Is<int>(i => i == 1)).Returns((User)null);
		var user = await userService.GetUser(id);
		Assert.Null(user);
	}

	[Fact]
	public async Task GetUserByName()
	{
		const string name = "Jeff";
		const string email = "a@b.com";
		var roles = new List<string> { "blah", PermanentRoles.Admin };
		var dummyUser = GetDummyUser(name, email);
		var userService = GetMockedUserService();
		_mockUserRepo.GetUserByName(name).Returns(Task.FromResult(dummyUser));
		_mockRoleRepo.GetUserRoles(dummyUser.UserID).Returns(Task.FromResult(roles));
		var user = await userService.GetUserByName(name);
		Assert.Same(dummyUser, user);
	}

	[Fact]
	public async Task GetUserByNameFail()
	{
		const string name = "Jeff";
		var userService = GetMockedUserService();
		_mockUserRepo.GetUserByName(Arg.Is<string>(i => i != name)).Returns(Task.FromResult(GetDummyUser(name, "")));
		_mockUserRepo.GetUserByName(Arg.Is<string>(i => i == name)).Returns((User)null);
		var user = await userService.GetUserByName(name);
		Assert.Null(user);
	}

	[Fact]
	public async Task GetUserByNameReturnsNullWithNullOrEmptyName()
	{
		var userService = GetMockedUserService();
		Assert.Null(await userService.GetUserByName(""));
		Assert.Null(await userService.GetUserByName(null));
	}

	[Fact]
	public async Task GetUserByEmail()
	{
		const string name = "Jeff";
		const string email = "a@b.com";
		var roles = new List<string> { "blah", PermanentRoles.Admin };
		var dummyUser = GetDummyUser(name, email);
		var userService = GetMockedUserService();
		_mockUserRepo.GetUserByEmail(email).Returns(Task.FromResult(dummyUser));
		_mockRoleRepo.GetUserRoles(dummyUser.UserID).Returns(Task.FromResult(roles));
		var user = await userService.GetUserByEmail(email);
		Assert.Same(dummyUser, user);
	}

	[Fact]
	public async Task GetUserByEmailFail()
	{
		const string email = "a@b.com";
		var userManager = GetMockedUserService();
		_mockUserRepo.GetUserByEmail(Arg.Is<string>(i => i != email)).Returns(Task.FromResult(GetDummyUser("", email)));
		_mockUserRepo.GetUserByEmail(Arg.Is<string>(i => i == email)).Returns((User)null);
		var user = await userManager.GetUserByEmail(email);
		Assert.Null(user);
	}

	public static User GetDummyUser(string name, string email)
	{
		return new User { UserID = 1, Name = name, Email = email, IsApproved = true, AuthorizationKey = new Guid()};
	}

	[Fact]
	public async Task NameIsInUse()
	{
		const string name = "jeff";
		var userService = GetMockedUserService();
		_mockUserRepo.GetUserByName(Arg.Is(name)).Returns(Task.FromResult(GetDummyUser(name, "a@b.com")));
		
		Assert.True(await userService.IsNameInUse(name));
		await _mockUserRepo.Received(1).GetUserByName(name);
		Assert.False(await userService.IsNameInUse("notjeff"));
		Assert.True(await userService.IsNameInUse(name.ToUpper()));
	}

	[Fact]
	public async Task EmailIsInUse()
	{
		const string email = "a@b.com";
		var userManager = GetMockedUserService();
		_mockUserRepo.GetUserByEmail(Arg.Is(email)).Returns(Task.FromResult(GetDummyUser("jeff", email)));
		Assert.True(await userManager.IsEmailInUse(email));
		await _mockUserRepo.Received(1).GetUserByEmail(email);
		Assert.False(await userManager.IsEmailInUse("nota@b.com"));
		Assert.True(await userManager.IsEmailInUse(email.ToUpper()));
	}

	[Fact]
	public async Task EmailInUserByAnotherTrue()
	{
		var userService = GetMockedUserService();
		var user = GetDummyUser("jeff", "a@b.com");
		_mockUserRepo.GetUserByEmail("c@d.com").Returns(Task.FromResult(new User { UserID = 123 }));
		var result = await userService.IsEmailInUseByDifferentUser(user, "c@d.com");
		await _mockUserRepo.Received().GetUserByEmail("c@d.com");
		Assert.True(result);
	}

	[Fact]
	public async Task EmailInUserByAnotherFalseBecauseSameUser()
	{
		var userService = GetMockedUserService();
		var user = GetDummyUser("jeff", "a@b.com");
		_mockUserRepo.GetUserByEmail("a@b.com").Returns(Task.FromResult(user));
		var result = await userService.IsEmailInUseByDifferentUser(user, "a@b.com");
		await _mockUserRepo.Received().GetUserByEmail("a@b.com");
		Assert.False(result);
	}

	[Fact]
	public async Task EmailInUserByAnotherFalseBecauseNoUser()
	{
		var userService = GetMockedUserService();
		var user = GetDummyUser("jeff", "a@b.com");
		_mockUserRepo.GetUserByEmail("c@d.com").Returns((User)null);
		var result = await userService.IsEmailInUseByDifferentUser(user, "c@d.com");
		await _mockUserRepo.Received().GetUserByEmail("c@d.com");
		Assert.False(result);
	}

	[Fact]
	public async Task CreateUser()
	{
		const string name = "jeff";
		const string nameCensor = "jeffcensor";
		const string email = "a@b.com";
		const string password = "fred";
		const string ip = "127.0.0.1";
		var userService = GetMockedUserService();
		var dummyUser = GetDummyUser(nameCensor, email);
		_mockUserRepo.CreateUser(nameCensor, email, Arg.Any<DateTime>(), true, Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(dummyUser));
		_mockTextParser.Censor(name).Returns(nameCensor);
		var user = await userService.CreateUser(name, email, password, true, ip);
		Assert.Equal(dummyUser.Name, user.Name);
		Assert.Equal(dummyUser.Email, user.Email);
		_mockTextParser.Received().Censor(name);
		await _mockUserRepo.Received().CreateUser(nameCensor, email, Arg.Any<DateTime>(), true, Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>());
		await _mockSecurityLogService.Received().CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.UserCreated);
	}


	[Fact]
	public async Task CreateUserFromSignup()
	{
		const string name = "jeff";
		const string nameCensor = "jeffcensor";
		const string email = "a@b.com";
		const string password = "fred";
		const string ip = "127.0.0.1";
		var userManager = GetMockedUserService();
		var dummyUser = GetDummyUser(nameCensor, email);
		_mockUserRepo.CreateUser(nameCensor, email, Arg.Any<DateTime>(), true, Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(dummyUser));
		_mockTextParser.Censor(name).Returns(nameCensor);
		var settings = new Settings();
		_mockSettingsManager.Current.Returns(settings);
		var signupData = new SignupData {Email = email, Name = name, Password = password};
		var user = await userManager.CreateUserWithProfile(signupData, ip);
		await _mockUserRepo.Received().CreateUser(nameCensor, email, Arg.Any<DateTime>(), true, Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>());
		await _mockSecurityLogService.Received().CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.UserCreated);
	}

	[Fact]
	public async Task CreateInvalidEmail()
	{
		var userService = GetMockedUserService();
		_mockTextParser.Censor(Arg.Any<string>()).Returns("blah");
		await Assert.ThrowsAsync<Exception>(async () => await userService.CreateUser("", "a b@oihfwe", "", true, ""));
	}

	[Fact]
	public async Task CreateUsedName()
	{
		const string usedName = "jeff";
		const string email = "a@b.com";
		var userService = GetMockedUserService();
		_mockTextParser.Censor("jeff").Returns("jeff");
		_mockTextParser.Censor("anynamejeff").Returns("anynamejeff");
		_mockUserRepo.GetUserByName(Arg.Is(usedName)).Returns(Task.FromResult(GetDummyUser(usedName, email)));
		await Assert.ThrowsAsync<Exception>(async () => await userService.CreateUser(usedName, email, "", true, ""));
		await Assert.ThrowsAsync<Exception>(async () => await userService.CreateUser(usedName.ToUpper(), email, "", true, ""));
	}

	[Fact]
	public async Task CreateNameNull()
	{
		var userManager = GetMockedUserService();
		await Assert.ThrowsAsync<Exception>(async () => await userManager.CreateUser(null, "a@b.com", "", true, ""));
	}

	[Fact]
	public async Task CreateNameEmpty()
	{
		var userManager = GetMockedUserService();
		await Assert.ThrowsAsync<Exception>(async () => await userManager.CreateUser(String.Empty, "a@b.com", "", true, ""));
	}

	[Fact]
	public async Task CreateUsedEmail()
	{
		const string usedEmail = "a@b.com";
		var userService = GetMockedUserService();
		_mockTextParser.Censor(Arg.Any<string>()).Returns("blah");
		_mockUserRepo.GetUserByEmail(Arg.Is(usedEmail)).Returns(Task.FromResult(GetDummyUser("jeff", usedEmail)));
		await Assert.ThrowsAsync<Exception>(async () => await userService.CreateUser("", usedEmail, "", true, ""));
	}

	[Fact]
	public async Task CreateEmailBanned()
	{
		const string bannedEmail = "a@b.com";
		var userService = GetMockedUserService();
		_mockTextParser.Censor(Arg.Any<string>()).Returns("blah");
		_mockBanRepo.EmailIsBanned(bannedEmail).Returns(Task.FromResult(true));
		await Assert.ThrowsAsync<Exception>(async () => await userService.CreateUser("name", bannedEmail, "", true, ""));
	}

	[Fact]
	public async Task CreateIPBanned()
	{
		const string bannedIP = "1.2.3.4";
		var userManager = GetMockedUserService();
		_mockTextParser.Censor(Arg.Any<string>()).Returns("blah");
		_mockBanRepo.IPIsBanned(bannedIP).Returns(Task.FromResult(true));
		await Assert.ThrowsAsync<Exception>(async () => await userManager.CreateUser("", "a@b.com", "", true, bannedIP));
	}

	[Fact]
	public async Task UpdateLastActivityDate()
	{
		var userManager = GetMockedUserService();
		var user = UserTest.GetTestUser();
		await userManager.UpdateLastActivityDate(user);
		await _mockUserRepo.Received().UpdateLastActivityDate(user, Arg.Any<DateTime>());
	}

	[Fact]
	public async Task ChangeEmailSuccess()
	{
		const string oldName = "Jeff";
		const string oldEmail = "a@b.com";
		const string newEmail = "c@d.com";

		var userManager = GetMockedUserService();
		_mockUserRepo.GetUserByEmail(oldEmail).Returns(Task.FromResult(GetDummyUser(oldName, oldEmail)));
		_mockUserRepo.GetUserByEmail(newEmail).Returns((User)null);
		_mockSettingsManager.Current.IsNewUserApproved.Returns(false);
		var targetUser = GetDummyUser(oldName, oldEmail);
		var user = new User { UserID = 34243 };
		await userManager.ChangeEmail(targetUser, newEmail, user, "123");
		await _mockUserRepo.Received(1).ChangeEmail(targetUser, newEmail);
		await _mockSecurityLogService.Received().CreateLogEntry(user, targetUser, "123", "Old: a@b.com, New: c@d.com", SecurityLogType.EmailChange);
	}

	[Fact]
	public async Task ChangeEmailAlreadyInUse()
	{
		const string oldName = "Jeff";
		const string oldEmail = "a@b.com";
		const string newEmail = "c@d.com";

		var userService = GetMockedUserService();
		_mockUserRepo.GetUserByEmail(oldEmail).Returns(Task.FromResult(GetDummyUser(oldName, oldEmail)));
		_mockUserRepo.GetUserByEmail(newEmail).Returns(Task.FromResult(GetDummyUser("Diana", newEmail)));
		_mockSettingsManager.Current.IsNewUserApproved.Returns(true);
		var user = GetDummyUser(oldName, oldEmail);
		await Assert.ThrowsAsync<Exception>(() => userService.ChangeEmail(user, newEmail, new User(), string.Empty));
		await _mockUserRepo.DidNotReceive().ChangeEmail(Arg.Any<User>(), Arg.Any<string>());
	}

	[Fact]
	public async Task ChangeEmailBad()
	{
		const string badEmail = "a b @ c";
		var userManager = GetMockedUserService();
		_mockSettingsManager.Current.IsNewUserApproved.Returns(true);
		var user = GetDummyUser("", "");
		await Assert.ThrowsAsync<Exception>(() => userManager.ChangeEmail(user, badEmail, new User(), String.Empty));
		await _mockUserRepo.DidNotReceive().ChangeEmail(Arg.Any<User>(), Arg.Any<string>());
	}

	[Fact]
	public async Task ChangeEmailMapsIsApprovedFromSettingsToUserRepoCall()
	{
		const string oldName = "Jeff";
		const string oldEmail = "a@b.com";
		const string newEmail = "c@d.com";

		var userManager = GetMockedUserService();
		_mockUserRepo.GetUserByEmail(oldEmail).Returns(Task.FromResult(GetDummyUser(oldName, oldEmail)));
		_mockUserRepo.GetUserByEmail(newEmail).Returns((User)null);
		_mockSettingsManager.Current.IsNewUserApproved.Returns(true);
		var targetUser = GetDummyUser(oldName, oldEmail);
		var user = new User { UserID = 34243 };
		await userManager.ChangeEmail(targetUser, newEmail, user, "123");
		await _mockUserRepo.Received().UpdateIsApproved(targetUser, true);
	}

	[Fact]
	public async Task ChangeNameSuccess()
	{
		const string oldName = "Jeff";
		const string oldEmail = "a@b.com";
		const string newName = "Diana";

		var userManager = GetMockedUserService();
		_mockUserRepo.GetUserByName(oldName).Returns(Task.FromResult(GetDummyUser(oldName, oldEmail)));
		_mockUserRepo.GetUserByName(newName).Returns((User)null);
		var targetUser = GetDummyUser(oldName, oldEmail);
		var user = new User { UserID = 1234531 };
		await userManager.ChangeName(targetUser, newName, user, "123");
		await _mockUserRepo.Received().ChangeName(targetUser, newName);
		await _mockSecurityLogService.Received().CreateLogEntry(user, targetUser, "123", "Old: Jeff, New: Diana", SecurityLogType.NameChange);
	}

	[Fact]
	public async Task ChangeNameFailUsed()
	{
		const string oldName = "Jeff";
		const string oldEmail = "a@b.com";
		const string newName = "Diana";

		var userService = GetMockedUserService();
		_mockUserRepo.GetUserByName(oldName).Returns(Task.FromResult(GetDummyUser(oldName, oldEmail)));
		_mockUserRepo.GetUserByName(newName).Returns(Task.FromResult(GetDummyUser(newName, oldEmail)));
		var user = GetDummyUser(oldName, oldEmail);
		await Assert.ThrowsAsync<Exception>(() => userService.ChangeName(user, newName, new User(), string.Empty));
		await _mockUserRepo.DidNotReceive().ChangeName(Arg.Any<User>(), Arg.Any<string>());
	}

	[Fact]
	public async Task ChangeNameNull()
	{
		var userService = GetMockedUserService();
		var user = GetDummyUser("Jeff", "a@b.com");
		await Assert.ThrowsAsync<Exception>(() => userService.ChangeName(user, null, new User(), string.Empty));
		await _mockUserRepo.DidNotReceive().ChangeName(Arg.Any<User>(), Arg.Any<string>());
	}

	[Fact]
	public async Task ChangeNameEmpty()
	{
		var userService = GetMockedUserService();
		var user = GetDummyUser("Jeff", "a@b.com");
		
		await Assert.ThrowsAsync<Exception>(() => userService.ChangeName(user, String.Empty, new User(), string.Empty));
		await _mockUserRepo.DidNotReceive().ChangeName(Arg.Any<User>(), Arg.Any<string>());
	}

	[Fact]
	public async Task Logout()
	{
		var userService = GetMockedUserService();
		var user = UserTest.GetTestUser();
		await userService.Logout(user, "123");
		await _mockSecurityLogService.Received().CreateLogEntry(null, user, "123", String.Empty, SecurityLogType.Logout);
	}

	[Fact]
	public async Task LoginSuccess()
	{
		const string email = "a@b.com";
		const string password = "fred";
		const string ip = "1.1.1.1";
		var user = UserTest.GetTestUser();
		var userService = GetMockedUserService();
		Guid? salt = Guid.NewGuid();
		var saltedHash = password.GetSHA256Hash(salt.Value);
		_mockUserRepo.GetHashedPasswordByEmail(email).Returns(Task.FromResult(Tuple.Create(saltedHash, salt)));
		_mockUserRepo.GetUserByEmail(email).Returns(Task.FromResult(user));

		var (result, _) = await userService.Login(email, password, ip);

		Assert.True(result);
		await _mockUserRepo.Received().UpdateLastLoginDate(user, Arg.Any<DateTime>());
		await _mockSecurityLogService.Received().CreateLogEntry(null, user, ip, "", SecurityLogType.Login);
		await _mockUserRepo.DidNotReceive().SetHashedPassword(user, Arg.Any<string>(), Arg.Any<Guid>());
	}

	[Fact]
	public async Task LoginSuccessNoSalt()
	{
		const string email = "a@b.com";
		const string password = "fred";
		const string ip = "1.1.1.1";
		var user = UserTest.GetTestUser();
		var userService = GetMockedUserService();
		Guid? salt = Guid.NewGuid();
		Guid? nosalt = null;
		_mockUserRepo.GetHashedPasswordByEmail(email).Returns(Tuple.Create(password.GetSHA256Hash(), nosalt));
		_mockUserRepo.GetUserByEmail(email).Returns(Task.FromResult(user));
		await _mockUserRepo.SetHashedPassword(user, Arg.Any<string>(), Arg.Do<Guid>(x => salt = x));

		var (result, _) = await userService.Login(email, password, ip);

		Assert.True(result);
		await _mockUserRepo.Received().UpdateLastLoginDate(user, Arg.Any<DateTime>());
		await _mockSecurityLogService.Received().CreateLogEntry(null, user, ip, "", SecurityLogType.Login);
		var saltyPassword = password.GetSHA256Hash(salt.Value);
		await _mockUserRepo.Received().SetHashedPassword(user, saltyPassword, salt.Value);
	}

	[Fact]
	public async Task LoginFail()
	{
		const string email = "a@b.com";
		const string password = "fred";
		const string ip = "1.1.1.1";
		var userService = GetMockedUserService();
		Guid? salt = null;
		_mockUserRepo.GetHashedPasswordByEmail(Arg.Any<string>()).Returns(Task.FromResult(Tuple.Create("1234", salt)));

		var (result, userOut) = await userService.Login(email, password, ip);

		Assert.False(result);
		await _mockSecurityLogService.Received().CreateLogEntry((User)null, null, ip, "E-mail attempted: " + email, SecurityLogType.FailedLogin);
		await _mockUserRepo.DidNotReceive().SetHashedPassword(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<Guid>());
	}

	[Fact]
	public async Task LoginWithUser()
	{
		var user = UserTest.GetTestUser();
		var service = GetMockedUserService();
		const string ip = "1.1.1.1";

		await service.Login(user, ip);
			
		await _mockUserRepo.Received().UpdateLastLoginDate(user, Arg.Any<DateTime>());
		await _mockSecurityLogService.Received().CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.Login);
	}

	[Fact]
	public async Task LoginWithUserPersistCookie()
	{
		var user = UserTest.GetTestUser();
		var service = GetMockedUserService();
		const string ip = "1.1.1.1";

		await service.Login(user, ip);

		await _mockUserRepo.Received().UpdateLastLoginDate(user, Arg.Any<DateTime>());
		await _mockSecurityLogService.Received().CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.Login);
	}

	[Fact]
	public async Task GetAllRoles()
	{
		var userService = GetMockedUserService();
		var list = new List<string>();
		_mockRoleRepo.GetAllRoles().Returns(Task.FromResult(list));
		var result = await userService.GetAllRoles();
		await _mockRoleRepo.Received().GetAllRoles();
		Assert.Same(list, result);
	}

	[Fact]
	public async Task CreateRole()
	{
		var userService = GetMockedUserService();
		const string role = "blah";
		await _mockSecurityLogService.CreateLogEntry(Arg.Any<User>(), (User)null, string.Empty, Arg.Any<string>(), SecurityLogType.RoleCreated);
		
		await userService.CreateRole(role, new User(), string.Empty);
		
		await _mockRoleRepo.Received().CreateRole(role);
		await _mockSecurityLogService.Received().CreateLogEntry(Arg.Any<User>(), null, string.Empty, "Role: blah", SecurityLogType.RoleCreated);
	}

	[Fact]
	public async Task DeleteRole()
	{
		var userService = GetMockedUserService();
		const string role = "blah";
		await userService.DeleteRole(role, default, default);
		await _mockRoleRepo.Received().DeleteRole(role);
		await _mockSecurityLogService.Received().CreateLogEntry(Arg.Any<User>(), null, Arg.Any<string>(), "Role: blah", SecurityLogType.RoleDeleted);
	}

	[Fact]
	public async Task DeleteRoleThrowsOnAdminOrMod()
	{
		var userService = GetMockedUserService();
		await Assert.ThrowsAsync<InvalidOperationException>(() => userService.DeleteRole("Admin", default, default));
		await Assert.ThrowsAsync<InvalidOperationException>(() => userService.DeleteRole("Moderator", default, default));
		await Assert.ThrowsAsync<InvalidOperationException>(() => userService.DeleteRole("admin", default, default));
		await Assert.ThrowsAsync<InvalidOperationException>(() => userService.DeleteRole("moderator", default, default));
		await _mockRoleRepo.DidNotReceive().DeleteRole(Arg.Any<string>());
	}

	[Fact]
	public async Task UpdateIsApproved()
	{
		var userService = GetMockedUserService();
		var targetUser = GetDummyUser("Jeff", "a@b.com");
		var user = new User { UserID = 97 };
		await userService.UpdateIsApproved(targetUser, true, user, "123");
		await _mockUserRepo.Received().UpdateIsApproved(targetUser, true);
		await _mockSecurityLogService.Received().CreateLogEntry(user, targetUser, Arg.Any<string>(), String.Empty, SecurityLogType.IsApproved);
	}

	[Fact]
	public async Task UpdateIsApprovedFalse()
	{
		var userService = GetMockedUserService();
		var targetUser = GetDummyUser("Jeff", "a@b.com");
		var user = new User { UserID = 97 };
		await userService.UpdateIsApproved(targetUser, false, user, "123");
		await _mockUserRepo.Received().UpdateIsApproved(targetUser, false);
		await _mockSecurityLogService.Received().CreateLogEntry(user, targetUser, Arg.Any<string>(), String.Empty, SecurityLogType.IsNotApproved);
	}

	[Fact]
	public async Task UpdateAuthKey()
	{
		var userService = GetMockedUserService();
		var user = GetDummyUser("Jeff", "a@b.com");
		var key = Guid.NewGuid();
		await userService.UpdateAuthorizationKey(user, key);
		await _mockUserRepo.Received().UpdateAuthorizationKey(user, key);
	}

	[Fact]
	public async Task VerifyUserByAuthKey()
	{
		var key = Guid.NewGuid();
		const string name = "Jeff";
		const string email = "a@b.com";
		var dummyUser = GetDummyUser(name, email);
		dummyUser.AuthorizationKey = key;
		var userManager = GetMockedUserService();
		_mockUserRepo.GetUserByAuthorizationKey(key).Returns(Task.FromResult(dummyUser));
		var user = await userManager.VerifyAuthorizationCode(dummyUser.AuthorizationKey, "123");
		Assert.Same(dummyUser, user);
		await _mockUserRepo.Received().UpdateIsApproved(dummyUser, true);
	}

	[Fact]
	public async Task VerifyUserByAuthKeyFail()
	{
		var service = GetMockedUserService();
		_mockUserRepo.GetUserByAuthorizationKey(Arg.Any<Guid>()).Returns((User)null);
		var user = await service.VerifyAuthorizationCode(Guid.NewGuid(), "123");
		Assert.Null(user);
		await _mockUserRepo.DidNotReceive().UpdateIsApproved(Arg.Any<User>(), true);
	}

	[Fact]
	public async Task SearchByEmail()
	{
		var service = GetMockedUserService();
		var list = new List<User>();
		_mockUserRepo.SearchByEmail("blah").Returns(Task.FromResult(list));
		var result = await service.SearchByEmail("blah");
		Assert.Same(list, result);
		await _mockUserRepo.Received().SearchByEmail("blah");
	}

	[Fact]
	public async Task SearchByName()
	{
		var service = GetMockedUserService();
		var list = new List<User>();
		_mockUserRepo.SearchByName("blah").Returns(Task.FromResult(list));
		var result = await service.SearchByName("blah");
		Assert.Same(list, result);
		await _mockUserRepo.Received().SearchByName("blah");
	}

	[Fact]
	public async Task SearchByRole()
	{
		var service = GetMockedUserService();
		var list = new List<User>();
		_mockUserRepo.SearchByRole("blah").Returns(Task.FromResult(list));
		var result = await service.SearchByRole("blah");
		Assert.Same(list, result);
		await _mockUserRepo.Received().SearchByRole("blah");
	}

	[Fact]
	public async Task GetUserEdit()
	{
		var service = GetMockedUserService();
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(new Profile { UserID = 1, Web = "blah"}));
		var user = new User { UserID = 1 };
		user.Roles = new List<string>();
		var result = await service.GetUserEdit(user);
		Assert.Equal(1, result.UserID);
		Assert.Equal("blah", result.Web);
	}

	[Fact]
	public async Task EditUserProfileOnly()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1 };
		user.Roles = new List<string>();
		var userEdit = new UserEdit();
		var profile = new Profile();
		var returnedProfile = GetReturnedProfile(userEdit);
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(returnedProfile));
		await _mockProfileRepo.Update(Arg.Do<Profile>(x => profile = x));

		await service.EditUser(user, userEdit, false, false, null, null, "123", user);

		await _mockUserRepo.DidNotReceive().ChangeEmail(Arg.Any<User>(), Arg.Any<string>());
		await _mockUserRepo.DidNotReceive().ChangeName(Arg.Any<User>(), Arg.Any<string>());
		await _mockUserRepo.DidNotReceive().SetHashedPassword(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<Guid>());
		await _mockProfileRepo.Received().Update(Arg.Any<Profile>());
	}

	private Profile GetReturnedProfile(UserEdit userEdit)
	{
		return new Profile
		{
			UserID = 1,
			IsSubscribed = userEdit.IsSubscribed,
			ShowDetails = userEdit.ShowDetails,
			IsPlainText = userEdit.IsPlainText,
			HideVanity = userEdit.HideVanity,
			Signature = userEdit.Signature,
			Location = userEdit.Location,
			Dob = userEdit.Dob,
			Web = userEdit.Web,
			Instagram = userEdit.Instagram,
			Facebook = userEdit.Facebook,
			Twitter = userEdit.Twitter
		};
	}

	[Fact]
	public async Task EditUserApprovalChange()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1, IsApproved = false};
		user.Roles = new List<string>();
		var userEdit = new UserEdit {IsApproved = true};
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(GetReturnedProfile(userEdit)));
		await service.EditUser(user, userEdit, false, false, null, null, "123", user);
		await _mockUserRepo.Received().UpdateIsApproved(user, true);
	}

	[Fact]
	public async Task EditUserNewEmail()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1, Email = "c@d.com" };
		user.Roles = new List<string>();
		var userEdit = new UserEdit { NewEmail = "a@b.com" };
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(GetReturnedProfile(userEdit)));
		await service.EditUser(user, userEdit, false, false, null, null, "123", user);
		await _mockUserRepo.Received().ChangeEmail(user, "a@b.com");
	}

	[Fact]
	public async Task EditUserNewPassword()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1 };
		user.Roles = new List<string>();
		var userEdit = new UserEdit { NewPassword = "foo" };
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(GetReturnedProfile(userEdit)));
		await service.EditUser(user, userEdit, false, false, null, null, "123", user);
		await _mockUserRepo.Received().SetHashedPassword(user, Arg.Any<string>(), Arg.Any<Guid>());
	}

	[Fact]
	public async Task EditUserAddRole()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1 };
		user.Roles = new List<string>();
		var userEdit = new UserEdit { Roles = new [] {"Admin", "Moderator"} };
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(GetReturnedProfile(userEdit)));
		await service.EditUser(user, userEdit, false, false, null, null, "123", user);
		await _mockRoleRepo.Received().ReplaceUserRoles(1, userEdit.Roles);
		await _mockSecurityLogService.Received().CreateLogEntry(Arg.Any<User>(), user, "123", "Admin", SecurityLogType.UserAddedToRole);
		await _mockSecurityLogService.Received().CreateLogEntry(Arg.Any<User>(), user, "123", "Moderator", SecurityLogType.UserAddedToRole);
	}

	[Fact]
	public async Task EditUserRemoveRole()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1 };
		user.Roles = new List<string> { "Admin", "Moderator" };
		var userEdit = new UserEdit { Roles = new[] { "SomethingElse" } };
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(GetReturnedProfile(userEdit)));
		await service.EditUser(user, userEdit, false, false, null, null, "123", user);
		await _mockRoleRepo.Received().ReplaceUserRoles(1, userEdit.Roles);
		await _mockSecurityLogService.Received().CreateLogEntry(Arg.Any<User>(), user, "123", "Admin", SecurityLogType.UserRemovedFromRole);
		await _mockSecurityLogService.Received().CreateLogEntry(Arg.Any<User>(), user, "123", "Moderator", SecurityLogType.UserRemovedFromRole);
	}

	[Fact]
	public async Task EditUserDeleteAvatar()
	{
		var service = GetMockedUserService();
		var user = new User {UserID = 1, Roles = new List<string>()};
		var userEdit = new UserEdit();
		var returnedProfile = GetReturnedProfile(userEdit);
		returnedProfile.AvatarID = 3;
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(returnedProfile));
		var profile = new Profile();
		await _mockProfileRepo.Update(Arg.Do<Profile>(x => profile = x));
		await service.EditUser(user, userEdit, true, false, null, null, "123", user);
		await _mockProfileRepo.Received().Update(Arg.Any<Profile>());
		Assert.Null(profile.AvatarID);
	}

	[Fact]
	public async Task EditUserNoDeleteAvatar()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1 };
		user.Roles = new List<string>();
		var userEdit = new UserEdit();
		var returnedProfile = GetReturnedProfile(userEdit);
		returnedProfile.AvatarID = 3;
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(returnedProfile));
		var profile = new Profile();
		await _mockProfileRepo.Update(Arg.Do<Profile>(x => profile = x));
		await service.EditUser(user, userEdit, false, false, null, null, "123", user);
		await _mockProfileRepo.Received().Update(Arg.Any<Profile>());
		Assert.Equal(3, profile.AvatarID);
	}

	[Fact]
	public async Task EditUserDeletePhoto()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1 };
		user.Roles = new List<string>();
		var userEdit = new UserEdit();
		var returnedProfile = GetReturnedProfile(userEdit);
		returnedProfile.ImageID = 3;
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(returnedProfile));
		var profile = new Profile();
		await _mockProfileRepo.Update(Arg.Do<Profile>(x => profile = x));
		await service.EditUser(user, userEdit, false, true, null, null, "123", user);
		await _mockProfileRepo.Received().Update(Arg.Any<Profile>());
		Assert.Null(profile.ImageID);
	}

	[Fact]
	public async Task EditUserNoDeletePhoto()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1 };
		user.Roles = new List<string>();
		var userEdit = new UserEdit();
		var returnedProfile = GetReturnedProfile(userEdit);
		returnedProfile.ImageID = 3;
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(returnedProfile));
		var profile = new Profile();
		await _mockProfileRepo.Update(Arg.Do<Profile>(x => profile = x));
		await service.EditUser(user, userEdit, false, false, null, null, "123", user);
		await _mockProfileRepo.Received().Update(Arg.Any<Profile>());
		Assert.Equal(3, profile.ImageID);
	}

	[Fact]
	public async Task EditUserNewAvatar()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1 };
		user.Roles = new List<string>();
		var userEdit = new UserEdit();
		var returnedProfile = GetReturnedProfile(userEdit);
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(returnedProfile));
		_mockProfileRepo.Update(Arg.Any<Profile>()).Returns(Task.FromResult(true));
		_mockUserAvatarRepo.SaveNewAvatar(1, Arg.Any<byte[]>(), Arg.Any<DateTime>()).Returns(Task.FromResult(12));
		var image = new byte[1];

		await service.EditUser(user, userEdit, false, false, image, null, "123", user);

		await _mockUserAvatarRepo.Received().SaveNewAvatar(1, image, Arg.Any<DateTime>());
	}

	[Fact]
	public async Task EditUserNewPhoto()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1 };
		user.Roles = new List<string>();
		var userEdit = new UserEdit();
		var returnedProfile = GetReturnedProfile(userEdit);
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(returnedProfile));
		_mockProfileRepo.Update(Arg.Any<Profile>()).Returns(Task.FromResult(true));
		_mockUserImageRepo.SaveNewImage(1, 0, true, Arg.Any<byte[]>(), Arg.Any<DateTime>()).Returns(Task.FromResult(12));
		var image = new byte[1];

		await service.EditUser(user, userEdit, false, false, null, image, "123", user);

		await _mockUserImageRepo.Received().SaveNewImage(1, 0, true, image, Arg.Any<DateTime>());
	}

	[Fact]
	public async Task UserEditPhotosDeleteAvatar()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1 };
		var userEdit = new UserEdit();
		var returnedProfile = GetReturnedProfile(userEdit);
		returnedProfile.AvatarID = 3;
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(returnedProfile));
		var profile = new Profile();
		await _mockProfileRepo.Update(Arg.Do<Profile>(x => profile = x));
		await service.EditUserProfileImages(user, true, false, null, null);
		await _mockProfileRepo.Received().Update(Arg.Any<Profile>());
		await _mockUserAvatarRepo.Received().DeleteAvatarsByUserID(user.UserID);
		Assert.Null(profile.AvatarID);
	}

	[Fact]
	public async Task UserEditPhotosNoDeleteAvatar()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1 };
		var userEdit = new UserEdit();
		var returnedProfile = GetReturnedProfile(userEdit);
		returnedProfile.AvatarID = 3;
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(returnedProfile));
		var profile = new Profile();
		await _mockProfileRepo.Update(Arg.Do<Profile>(x => profile = x));
		await service.EditUserProfileImages(user, false, false, null, null);
		await _mockProfileRepo.Received().Update(Arg.Any<Profile>());
		await _mockUserAvatarRepo.DidNotReceive().DeleteAvatarsByUserID(user.UserID);
		Assert.Equal(3, profile.AvatarID);
	}

	[Fact]
	public async Task UserEditPhotosDeletePhoto()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1 };
		var userEdit = new UserEdit();
		var returnedProfile = GetReturnedProfile(userEdit);
		returnedProfile.ImageID = 3;
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(returnedProfile));
		var profile = new Profile();
		await _mockProfileRepo.Update(Arg.Do<Profile>(x => profile = x));
		await service.EditUserProfileImages(user, false, true, null, null);
		await _mockProfileRepo.Received().Update(Arg.Any<Profile>());
		await _mockUserImageRepo.Received().DeleteImagesByUserID(user.UserID);
		Assert.Null(profile.ImageID);
	}

	[Fact]
	public async Task UserEditPhotosNoDeletePhoto()
	{
		var service = GetMockedUserService();
		var user = new User { UserID = 1 };
		var userEdit = new UserEdit();
		var returnedProfile = GetReturnedProfile(userEdit);
		returnedProfile.ImageID = 3;
		_mockProfileRepo.GetProfile(1).Returns(Task.FromResult(returnedProfile));
		var profile = new Profile();
		await _mockProfileRepo.Update(Arg.Do<Profile>(x => profile = x));
		await service.EditUserProfileImages(user, false, false, null, null);
		await _mockProfileRepo.Received().Update(Arg.Any<Profile>());
		await _mockUserImageRepo.DidNotReceive().DeleteImagesByUserID(user.UserID);
		Assert.Equal(3, profile.ImageID);
	}

	[Fact]
	public async Task GetUsersOnlineCallsRepo()
	{
		var service = GetMockedUserService();
		var users = new List<User>();
		_mockUserRepo.GetUsersOnline().Returns(Task.FromResult(users));
		var result = await service.GetUsersOnline();
		await _mockUserRepo.Received().GetUsersOnline();
		Assert.Same(users, result);
	}

	[Fact]
	public async Task DeleteUserLogs()
	{
		var targetUser = new User { UserID = 1 };
		var user = new User { UserID = 2 };
		var service = GetMockedUserService();
		await service.DeleteUser(targetUser, user, "127.0.0.1", true);
		await _mockSecurityLogService.Received().CreateLogEntry(user, targetUser, "127.0.0.1", Arg.Any<string>(), SecurityLogType.UserDeleted);
	}

	[Fact]
	public async Task DeleteUserCallsRepo()
	{
		var targetUser = new User { UserID = 1 };
		var user = new User { UserID = 2 };
		var service = GetMockedUserService();
		await service.DeleteUser(targetUser, user, "127.0.0.1", true);
		await _mockUserRepo.Received().DeleteUser(targetUser);
	}

	[Fact]
	public async Task DeleteUserCallsBanRepoIfBanIsTrue()
	{
		var targetUser = new User { UserID = 1, Email = "a@b.com" };
		var user = new User { UserID = 2 };
		var service = GetMockedUserService();
		await service.DeleteUser(targetUser, user, "127.0.0.1", true);
		await _mockBanRepo.Received().BanEmail(targetUser.Email);
	}

	[Fact]
	public async Task DeleteUserDoesNotCallBanRepoIfBanIsFalse()
	{
		var targetUser = new User { UserID = 1, Email = "a@b.com" };
		var user = new User { UserID = 2 };
		var service = GetMockedUserService();
		await service.DeleteUser(targetUser, user, "127.0.0.1", false);
		await _mockBanRepo.DidNotReceive().BanEmail(targetUser.Email);
	}

	[Fact]
	public async Task ForgotPasswordCallsMailerForGoodUser()
	{
		var user = new User { UserID = 2, Email = "a@b.com" };
		var service = GetMockedUserService();
		_mockUserRepo.GetUserByEmail(user.Email).Returns(Task.FromResult(user));
		await service.GeneratePasswordResetEmail(user, "http");
		await _mockForgotMailer.Received(1).ComposeAndQueue(user, Arg.Any<string>());
	}

	[Fact]
	public async Task ForgotPasswordGeneratesNewAuthKey()
	{
		var user = new User { UserID = 2, Email = "a@b.com" };
		var service = GetMockedUserService();
		_mockUserRepo.GetUserByEmail(user.Email).Returns(Task.FromResult(user));
		await service.GeneratePasswordResetEmail(user, "http");
		await _mockUserRepo.Received(1).UpdateAuthorizationKey(user, Arg.Any<Guid>());
	}

	[Fact]
	public async Task ForgotPasswordThrowsForNoUser()
	{
		var service = GetMockedUserService();
		_mockUserRepo.GetUserByEmail(Arg.Any<string>()).Returns((User)null);
		await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.GeneratePasswordResetEmail(null, "http"));
		await _mockForgotMailer.DidNotReceive().ComposeAndQueue(Arg.Any<User>(), Arg.Any<string>());
	}
}