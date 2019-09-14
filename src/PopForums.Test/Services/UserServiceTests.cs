using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Moq;
using Xunit;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using PopForums.Test.Models;

namespace PopForums.Test.Services
{
	public class UserServiceTests
	{
		private Mock<IUserRepository> _mockUserRepo;
		private Mock<IRoleRepository> _mockRoleRepo;
		private Mock<IProfileRepository> _mockProfileRepo;
		private Mock<ISettingsManager> _mockSettingsManager;
		private Mock<IUserAvatarRepository> _mockUserAvatarRepo;
		private Mock<IUserImageRepository> _mockUserImageRepo;
		private Mock<ISecurityLogService> _mockSecurityLogService;
		private Mock<ITextParsingService> _mockTextParser;
		private Mock<IBanRepository> _mockBanRepo;
		private Mock<IForgotPasswordMailer> _mockForgotMailer;
		private Mock<IImageService> _mockImageService;

		private UserService GetMockedUserService()
		{
			_mockUserRepo = new Mock<IUserRepository>();
			_mockRoleRepo = new Mock<IRoleRepository>();
			_mockProfileRepo = new Mock<IProfileRepository>();
			_mockSettingsManager = new Mock<ISettingsManager>();
			_mockUserAvatarRepo = new Mock<IUserAvatarRepository>();
			_mockUserImageRepo = new Mock<IUserImageRepository>();
			_mockSecurityLogService = new Mock<ISecurityLogService>();
			_mockTextParser = new Mock<ITextParsingService>();
			_mockBanRepo = new Mock<IBanRepository>();
			_mockForgotMailer = new Mock<IForgotPasswordMailer>();
			_mockImageService = new Mock<IImageService>();
			_mockRoleRepo.Setup(r => r.GetUserRoles(It.IsAny<int>())).ReturnsAsync(new List<string>());
			return new UserService(_mockUserRepo.Object, _mockRoleRepo.Object, _mockProfileRepo.Object, _mockSettingsManager.Object, _mockUserAvatarRepo.Object, _mockUserImageRepo.Object, _mockSecurityLogService.Object, _mockTextParser.Object, _mockBanRepo.Object, _mockForgotMailer.Object, _mockImageService.Object);
		}

		[Fact]
		public async Task SetPassword()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("jeff", "a@b.com");
			var salt = Guid.NewGuid();
			_mockUserRepo.Setup(x => x.SetHashedPassword(user, It.IsAny<string>(), It.IsAny<Guid>())).Callback<User, string, Guid>((u, p, s) => salt = s);

			await userService.SetPassword(user, "fred", String.Empty, user);

			var hashedPassword = "fred".GetSHA256Hash(salt);
			_mockUserRepo.Verify(r => r.SetHashedPassword(user, hashedPassword, salt));
		}

		[Fact]
		public void CheckPassword()
		{
			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(string.Empty)).ReturnsAsync(Tuple.Create("0M/C5TGbgs3HGjOHPoJsk9fuETY/iskcT6Oiz80ihuU=", It.IsAny<Guid?>()));

			Assert.True(userService.CheckPassword(string.Empty, "fred").Result.Item1);
		}

		[Fact]
		public void CheckPasswordFail()
		{
			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(string.Empty)).ReturnsAsync(Tuple.Create("VwqQv7+MfqtdxdTiaDLVsQ==", It.IsAny<Guid?>()));

			Assert.False(userService.CheckPassword(String.Empty, "fsdfsdfsdfsdf").Result.Item1);
		}

		[Fact]
		public void CheckPasswordHasSalt()
		{
			var userService = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			var hashedPassword = "fred".GetSHA256Hash(salt.Value);
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(string.Empty)).ReturnsAsync(Tuple.Create(hashedPassword, salt));

			Assert.True(userService.CheckPassword(String.Empty, "fred").Result.Item1);
		}

		[Fact]
		public void CheckPasswordHasSaltFail()
		{
			var userService = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			var hashedPassword = "fred".GetSHA256Hash(salt.Value);
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(string.Empty)).ReturnsAsync(Tuple.Create(hashedPassword, salt));

			Assert.False(userService.CheckPassword(String.Empty, "dsfsdfsdfsdf").Result.Item1);
		}

		[Fact]
		public void CheckPasswordPassesWithoutSaltOnMD5Fallback()
		{
			var userService = GetMockedUserService();
			Guid? salt = null;
			var hashedPassword = "fred".GetMD5Hash();
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(string.Empty)).ReturnsAsync(Tuple.Create(hashedPassword, salt));

			Assert.True(userService.CheckPassword(String.Empty, "fred").Result.Item1);
		}

		[Fact]
		public void CheckPasswordPassesWithSaltOnMD5Fallback()
		{
			var userService = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			var hashedPassword = "fred".GetMD5Hash(salt.Value);
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(string.Empty)).ReturnsAsync(Tuple.Create(hashedPassword, salt));

			Assert.True(userService.CheckPassword(String.Empty, "fred").Result.Item1);
		}

		[Fact]
		public void CheckPasswordFailsOnMD5FallbackNoMatch()
		{
			var userService = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			var hashedPassword = "fred".GetMD5Hash(salt.Value);
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(string.Empty)).ReturnsAsync(Tuple.Create(hashedPassword, salt));

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
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(email)).ReturnsAsync(Tuple.Create(hashedPassword, salt));
			_mockUserRepo.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(user);

			Assert.False(userService.CheckPassword(email, "abc").Result.Item1);
			_mockUserRepo.Verify(x => x.SetHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
		}

		[Fact]
		public void CheckPasswordPassesWithSaltOnMD5FallbackCallsUpdate()
		{
			var userService = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			var email = "a@b.com";
			var user = new User {Email = email};
			var hashedPassword = "fred".GetMD5Hash(salt.Value);
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(email)).ReturnsAsync(Tuple.Create(hashedPassword, salt));
			_mockUserRepo.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(user);

			Assert.True(userService.CheckPassword(email, "fred").Result.Item1);
			_mockUserRepo.Verify(x => x.SetHashedPassword(user, It.IsAny<string>(), It.IsAny<Guid>()), Times.Once);
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
			_mockUserRepo.Setup(r => r.GetUser(id)).ReturnsAsync(dummyUser);
			_mockRoleRepo.Setup(r => r.GetUserRoles(id)).ReturnsAsync(roles);
			var user = await userService.GetUser(id);
			Assert.Same(dummyUser, user);
		}

		[Fact]
		public async Task GetUserFail()
		{
			const int id = 1;
			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUser(It.Is<int>(i => i != 1))).ReturnsAsync(GetDummyUser("", ""));
			_mockUserRepo.Setup(r => r.GetUser(It.Is<int>(i => i == 1))).ReturnsAsync((User)null);
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
			_mockUserRepo.Setup(r => r.GetUserByName(name)).ReturnsAsync(dummyUser);
			_mockRoleRepo.Setup(r => r.GetUserRoles(dummyUser.UserID)).ReturnsAsync(roles);
			var user = await userService.GetUserByName(name);
			Assert.Same(dummyUser, user);
		}

		[Fact]
		public async Task GetUserByNameFail()
		{
			const string name = "Jeff";
			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByName(It.Is<string>(i => i != name))).ReturnsAsync(GetDummyUser(name, ""));
			_mockUserRepo.Setup(r => r.GetUserByName(It.Is<string>(i => i == name))).ReturnsAsync((User)null);
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
			_mockUserRepo.Setup(r => r.GetUserByEmail(email)).ReturnsAsync(dummyUser);
			_mockRoleRepo.Setup(r => r.GetUserRoles(dummyUser.UserID)).ReturnsAsync(roles);
			var user = await userService.GetUserByEmail(email);
			Assert.Same(dummyUser, user);
		}

		[Fact]
		public async Task GetUserByEmailFail()
		{
			const string email = "a@b.com";
			var userManager = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByEmail(It.Is<string>(i => i != email))).ReturnsAsync(GetDummyUser("", email));
			_mockUserRepo.Setup(r => r.GetUserByEmail(It.Is<string>(i => i == email))).ReturnsAsync((User)null);
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
			_mockUserRepo.Setup(r => r.GetUserByName(It.IsRegex("^" + name + "$", RegexOptions.IgnoreCase))).ReturnsAsync(GetDummyUser(name, "a@b.com"));
			Assert.True(await userService.IsNameInUse(name));
			_mockUserRepo.Verify(r => r.GetUserByName(name), Times.Exactly(1));
			Assert.False(await userService.IsNameInUse("notjeff"));
			Assert.True(await userService.IsNameInUse(name.ToUpper()));
		}

		[Fact]
		public async Task EmailIsInUse()
		{
			const string email = "a@b.com";
			var userManager = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByEmail(It.IsRegex("^" + email + "$", RegexOptions.IgnoreCase))).ReturnsAsync(GetDummyUser("jeff", email));
			Assert.True(await userManager.IsEmailInUse(email));
			_mockUserRepo.Verify(r => r.GetUserByEmail(email), Times.Exactly(1));
			Assert.False(await userManager.IsEmailInUse("nota@b.com"));
			Assert.True(await userManager.IsEmailInUse(email.ToUpper()));
		}

		[Fact]
		public async Task EmailInUserByAnotherTrue()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("jeff", "a@b.com");
			_mockUserRepo.Setup(u => u.GetUserByEmail("c@d.com")).ReturnsAsync(new User { UserID = 123 });
			var result = await userService.IsEmailInUseByDifferentUser(user, "c@d.com");
			_mockUserRepo.Verify(u => u.GetUserByEmail("c@d.com"), Times.Once());
			Assert.True(result);
		}

		[Fact]
		public async Task EmailInUserByAnotherFalseBecauseSameUser()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("jeff", "a@b.com");
			_mockUserRepo.Setup(u => u.GetUserByEmail("a@b.com")).ReturnsAsync(user);
			var result = await userService.IsEmailInUseByDifferentUser(user, "a@b.com");
			_mockUserRepo.Verify(u => u.GetUserByEmail("a@b.com"), Times.Once());
			Assert.False(result);
		}

		[Fact]
		public async Task EmailInUserByAnotherFalseBecauseNoUser()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("jeff", "a@b.com");
			_mockUserRepo.Setup(u => u.GetUserByEmail("c@d.com")).ReturnsAsync((User)null);
			var result = await userService.IsEmailInUseByDifferentUser(user, "c@d.com");
			_mockUserRepo.Verify(u => u.GetUserByEmail("c@d.com"), Times.Once());
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
			_mockUserRepo.Setup(r => r.CreateUser(nameCensor, email, It.IsAny<DateTime>(), true, It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(dummyUser);
			_mockTextParser.Setup(t => t.Censor(name)).Returns(nameCensor);
			var user = await userService.CreateUser(name, email, password, true, ip);
			Assert.Equal(dummyUser.Name, user.Name);
			Assert.Equal(dummyUser.Email, user.Email);
			_mockTextParser.Verify(t => t.Censor(name), Times.Once());
			_mockUserRepo.Verify(r => r.CreateUser(nameCensor, email, It.IsAny<DateTime>(), true, It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.UserCreated));
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
			_mockUserRepo.Setup(r => r.CreateUser(nameCensor, email, It.IsAny<DateTime>(), true, It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(dummyUser);
			_mockTextParser.Setup(t => t.Censor(name)).Returns(nameCensor);
			var settings = new Settings();
			_mockSettingsManager.Setup(s => s.Current).Returns(settings);
			var signUpdata = new SignupData {Email = email, Name = name, Password = password};
			var user = await userManager.CreateUser(signUpdata, ip);
			_mockUserRepo.Verify(r => r.CreateUser(nameCensor, email, It.IsAny<DateTime>(), true, It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.UserCreated));
		}

		[Fact]
		public async Task CreateInvalidEmail()
		{
			var userService = GetMockedUserService();
			_mockTextParser.Setup(t => t.Censor(It.IsAny<string>())).Returns("blah");
			await Assert.ThrowsAsync<Exception>(async () => await userService.CreateUser("", "a b@oihfwe", "", true, ""));
		}

		[Fact]
		public async Task CreateUsedName()
		{
			const string usedName = "jeff";
			const string email = "a@b.com";
			var userService = GetMockedUserService();
			_mockTextParser.Setup(t => t.Censor("jeff")).Returns("jeff");
			_mockTextParser.Setup(t => t.Censor("anynamejeff")).Returns("anynamejeff");
			_mockUserRepo.Setup(r => r.GetUserByName(It.IsRegex("^" + usedName + "$", RegexOptions.IgnoreCase))).ReturnsAsync(GetDummyUser(usedName, email));
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
			_mockTextParser.Setup(t => t.Censor(It.IsAny<string>())).Returns("blah");
			_mockUserRepo.Setup(r => r.GetUserByEmail(It.IsRegex("^" + usedEmail + "$", RegexOptions.IgnoreCase))).ReturnsAsync(GetDummyUser("jeff", usedEmail));
			await Assert.ThrowsAsync<Exception>(async () => await userService.CreateUser("", usedEmail, "", true, ""));
		}

		[Fact]
		public async Task CreateEmailBanned()
		{
			const string bannedEmail = "a@b.com";
			var userService = GetMockedUserService();
			_mockTextParser.Setup(t => t.Censor(It.IsAny<string>())).Returns("blah");
			_mockBanRepo.Setup(b => b.EmailIsBanned(bannedEmail)).ReturnsAsync(true);
			await Assert.ThrowsAsync<Exception>(async () => await userService.CreateUser("name", bannedEmail, "", true, ""));
		}

		[Fact]
		public async Task CreateIPBanned()
		{
			const string bannedIP = "1.2.3.4";
			var userManager = GetMockedUserService();
			_mockTextParser.Setup(t => t.Censor(It.IsAny<string>())).Returns("blah");
			_mockBanRepo.Setup(b => b.IPIsBanned(bannedIP)).ReturnsAsync(true);
			await Assert.ThrowsAsync<Exception>(async () => await userManager.CreateUser("", "a@b.com", "", true, bannedIP));
		}

		[Fact]
		public async Task UpdateLastActivityDate()
		{
			var userManager = GetMockedUserService();
			var user = UserTest.GetTestUser();
			await userManager.UpdateLastActivityDate(user);
			_mockUserRepo.Verify(r => r.UpdateLastActivityDate(user, It.IsAny<DateTime>()), Times.Once());
		}

		[Fact]
		public async Task ChangeEmailSuccess()
		{
			const string oldName = "Jeff";
			const string oldEmail = "a@b.com";
			const string newEmail = "c@d.com";

			var userManager = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByEmail(oldEmail)).ReturnsAsync(GetDummyUser(oldName, oldEmail));
			_mockUserRepo.Setup(r => r.GetUserByEmail(newEmail)).ReturnsAsync((User)null);
			_mockSettingsManager.Setup(x => x.Current.IsNewUserApproved).Returns(false);
			var targetUser = GetDummyUser(oldName, oldEmail);
			var user = new User { UserID = 34243 };
			await userManager.ChangeEmail(targetUser, newEmail, user, "123");
			_mockUserRepo.Verify(r => r.ChangeEmail(targetUser, newEmail), Times.Exactly(1));
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, "123", "Old: a@b.com, New: c@d.com", SecurityLogType.EmailChange));
		}

		[Fact]
		public async Task ChangeEmailAlreadyInUse()
		{
			const string oldName = "Jeff";
			const string oldEmail = "a@b.com";
			const string newEmail = "c@d.com";

			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByEmail(oldEmail)).ReturnsAsync(GetDummyUser(oldName, oldEmail));
			_mockUserRepo.Setup(r => r.GetUserByEmail(newEmail)).ReturnsAsync(GetDummyUser("Diana", newEmail));
			_mockSettingsManager.Setup(x => x.Current.IsNewUserApproved).Returns(true);
			var user = GetDummyUser(oldName, oldEmail);
			await Assert.ThrowsAsync<Exception>(() => userService.ChangeEmail(user, newEmail, It.IsAny<User>(), It.IsAny<string>()));
			_mockUserRepo.Verify(r => r.ChangeEmail(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Fact]
		public async Task ChangeEmailBad()
		{
			const string badEmail = "a b @ c";
			var userManager = GetMockedUserService();
			_mockSettingsManager.Setup(x => x.Current.IsNewUserApproved).Returns(true);
			var user = GetDummyUser("", "");
			await Assert.ThrowsAsync<Exception>(() => userManager.ChangeEmail(user, badEmail, It.IsAny<User>(), It.IsAny<string>()));
			_mockUserRepo.Verify(r => r.ChangeEmail(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Fact]
		public async Task ChangeEmailMapsIsApprovedFromSettingsToUserRepoCall()
		{
			const string oldName = "Jeff";
			const string oldEmail = "a@b.com";
			const string newEmail = "c@d.com";

			var userManager = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByEmail(oldEmail)).ReturnsAsync(GetDummyUser(oldName, oldEmail));
			_mockUserRepo.Setup(r => r.GetUserByEmail(newEmail)).ReturnsAsync((User)null);
			_mockSettingsManager.Setup(x => x.Current.IsNewUserApproved).Returns(true);
			var targetUser = GetDummyUser(oldName, oldEmail);
			var user = new User { UserID = 34243 };
			await userManager.ChangeEmail(targetUser, newEmail, user, "123");
			_mockUserRepo.Verify(x => x.UpdateIsApproved(targetUser, true), Times.Once());
		}

		[Fact]
		public async Task ChangeNameSuccess()
		{
			const string oldName = "Jeff";
			const string oldEmail = "a@b.com";
			const string newName = "Diana";

			var userManager = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByName(oldName)).ReturnsAsync(GetDummyUser(oldName, oldEmail));
			_mockUserRepo.Setup(r => r.GetUserByName(newName)).ReturnsAsync((User)null);
			var targetUser = GetDummyUser(oldName, oldEmail);
			var user = new User { UserID = 1234531 };
			await userManager.ChangeName(targetUser, newName, user, "123");
			_mockUserRepo.Verify(r => r.ChangeName(targetUser, newName));
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, "123", "Old: Jeff, New: Diana", SecurityLogType.NameChange));
		}

		[Fact]
		public async Task ChangeNameFailUsed()
		{
			const string oldName = "Jeff";
			const string oldEmail = "a@b.com";
			const string newName = "Diana";

			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByName(oldName)).ReturnsAsync(GetDummyUser(oldName, oldEmail));
			_mockUserRepo.Setup(r => r.GetUserByName(newName)).ReturnsAsync(GetDummyUser(newName, oldEmail));
			var user = GetDummyUser(oldName, oldEmail);
			await Assert.ThrowsAsync<Exception>(() => userService.ChangeName(user, newName, It.IsAny<User>(), It.IsAny<string>()));
			_mockUserRepo.Verify(r => r.ChangeName(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Fact]
		public async Task ChangeNameNull()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("Jeff", "a@b.com");
			await Assert.ThrowsAsync<Exception>(() => userService.ChangeName(user, null, It.IsAny<User>(), It.IsAny<string>()));
			_mockUserRepo.Verify(r => r.ChangeName(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Fact]
		public async Task ChangeNameEmpty()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("Jeff", "a@b.com");
			await Assert.ThrowsAsync<Exception>(() => userService.ChangeName(user, String.Empty, It.IsAny<User>(), It.IsAny<string>()));
			_mockUserRepo.Verify(r => r.ChangeName(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Fact]
		public async Task Logout()
		{
			var userService = GetMockedUserService();
			var user = UserTest.GetTestUser();
			await userService.Logout(user, "123");
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, "123", String.Empty, SecurityLogType.Logout));
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
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(email)).ReturnsAsync(Tuple.Create(saltedHash, salt));
			_mockUserRepo.Setup(r => r.GetUserByEmail(email)).ReturnsAsync(user);
			_mockUserRepo.Setup(r => r.UpdateLastLoginDate(user, It.IsAny<DateTime>()));

			var (result, userOut) = await userService.Login(email, password, ip);

			Assert.True(result);
			_mockUserRepo.Verify(r => r.UpdateLastLoginDate(user, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, ip, "", SecurityLogType.Login), Times.Once());
			_mockUserRepo.Verify(x => x.SetHashedPassword(user, It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
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
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(email)).ReturnsAsync(Tuple.Create(password.GetSHA256Hash(), nosalt));
			_mockUserRepo.Setup(r => r.GetUserByEmail(email)).ReturnsAsync(user);
			_mockUserRepo.Setup(r => r.UpdateLastLoginDate(user, It.IsAny<DateTime>()));
			_mockUserRepo.Setup(x => x.SetHashedPassword(user, It.IsAny<string>(), It.IsAny<Guid>())).Callback<User, string, Guid>((u, p, s) => salt = s);

			var (result, userOut) = await userService.Login(email, password, ip);

			Assert.True(result);
			_mockUserRepo.Verify(r => r.UpdateLastLoginDate(user, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, ip, "", SecurityLogType.Login), Times.Once());
			var saltyPassword = password.GetSHA256Hash(salt.Value);
			_mockUserRepo.Verify(x => x.SetHashedPassword(user, saltyPassword, salt.Value), Times.Once());
		}

		[Fact]
		public async Task LoginFail()
		{
			const string email = "a@b.com";
			const string password = "fred";
			const string ip = "1.1.1.1";
			var userService = GetMockedUserService();
			Guid? salt = null;
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(It.IsAny<string>())).ReturnsAsync(Tuple.Create("1234", salt));

			var (result, userOut) = await userService.Login(email, password, ip);

			Assert.False(result);
			_mockSecurityLogService.Verify(s => s.CreateLogEntry((User)null, null, ip, "E-mail attempted: " + email, SecurityLogType.FailedLogin), Times.Once());
			_mockUserRepo.Verify(x => x.SetHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
		}

		[Fact]
		public async Task LoginWithUser()
		{
			var user = UserTest.GetTestUser();
			var service = GetMockedUserService();
			const string ip = "1.1.1.1";

			await service.Login(user, ip);
			
			_mockUserRepo.Verify(u => u.UpdateLastLoginDate(user, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.Login), Times.Once());
		}

		[Fact]
		public async Task LoginWithUserPersistCookie()
		{
			var user = UserTest.GetTestUser();
			var service = GetMockedUserService();
			const string ip = "1.1.1.1";

			await service.Login(user, ip);

			_mockUserRepo.Verify(u => u.UpdateLastLoginDate(user, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.Login), Times.Once());
		}

		[Fact]
		public async Task GetAllRoles()
		{
			var userService = GetMockedUserService();
			var list = new List<string>();
			_mockRoleRepo.Setup(r => r.GetAllRoles()).ReturnsAsync(list);
			var result = await userService.GetAllRoles();
			_mockRoleRepo.Verify(r => r.GetAllRoles(), Times.Once());
			Assert.Same(list, result);
		}

		[Fact]
		public async Task CreateRole()
		{
			var userService = GetMockedUserService();
			const string role = "blah";
			await userService.CreateRole(role, It.IsAny<User>(), It.IsAny<string>());
			_mockRoleRepo.Verify(r => r.CreateRole(role), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), null, It.IsAny<string>(), "Role: blah", SecurityLogType.RoleCreated), Times.Once());
		}

		[Fact]
		public async Task DeleteRole()
		{
			var userService = GetMockedUserService();
			const string role = "blah";
			await userService.DeleteRole(role, It.IsAny<User>(), It.IsAny<string>());
			_mockRoleRepo.Verify(r => r.DeleteRole(role), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), null, It.IsAny<string>(), "Role: blah", SecurityLogType.RoleDeleted), Times.Once());
		}

		[Fact]
		public async Task DeleteRoleThrowsOnAdminOrMod()
		{
			var userService = GetMockedUserService();
			await Assert.ThrowsAsync<InvalidOperationException>(() => userService.DeleteRole("Admin", It.IsAny<User>(), It.IsAny<string>()));
			await Assert.ThrowsAsync<InvalidOperationException>(() => userService.DeleteRole("Moderator", It.IsAny<User>(), It.IsAny<string>()));
			await Assert.ThrowsAsync<InvalidOperationException>(() => userService.DeleteRole("admin", It.IsAny<User>(), It.IsAny<string>()));
			await Assert.ThrowsAsync<InvalidOperationException>(() => userService.DeleteRole("moderator", It.IsAny<User>(), It.IsAny<string>()));
			_mockRoleRepo.Verify(r => r.DeleteRole(It.IsAny<string>()), Times.Never());
		}

		[Fact]
		public async Task UpdateIsApproved()
		{
			var userService = GetMockedUserService();
			var targetUser = GetDummyUser("Jeff", "a@b.com");
			var user = new User { UserID = 97 };
			await userService.UpdateIsApproved(targetUser, true, user, "123");
			_mockUserRepo.Verify(u => u.UpdateIsApproved(targetUser, true), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, It.IsAny<string>(), String.Empty, SecurityLogType.IsApproved));
		}

		[Fact]
		public async Task UpdateIsApprovedFalse()
		{
			var userService = GetMockedUserService();
			var targetUser = GetDummyUser("Jeff", "a@b.com");
			var user = new User { UserID = 97 };
			await userService.UpdateIsApproved(targetUser, false, user, "123");
			_mockUserRepo.Verify(u => u.UpdateIsApproved(targetUser, false), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, It.IsAny<string>(), String.Empty, SecurityLogType.IsNotApproved));
		}

		[Fact]
		public async Task UpdateAuthKey()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("Jeff", "a@b.com");
			var key = Guid.NewGuid();
			await userService.UpdateAuthorizationKey(user, key);
			_mockUserRepo.Verify(u => u.UpdateAuthorizationKey(user, key), Times.Once());
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
			_mockUserRepo.Setup(r => r.GetUserByAuthorizationKey(key)).ReturnsAsync(dummyUser);
			var user = await userManager.VerifyAuthorizationCode(dummyUser.AuthorizationKey, "123");
			Assert.Same(dummyUser, user);
			_mockUserRepo.Verify(u => u.UpdateIsApproved(dummyUser, true), Times.Once());
		}

		[Fact]
		public async Task VerifyUserByAuthKeyFail()
		{
			var service = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByAuthorizationKey(It.IsAny<Guid>())).ReturnsAsync((User)null);
			var user = await service.VerifyAuthorizationCode(Guid.NewGuid(), "123");
			Assert.Null(user);
			_mockUserRepo.Verify(u => u.UpdateIsApproved(It.IsAny<User>(), true), Times.Never());
		}

		[Fact]
		public async Task SearchByEmail()
		{
			var service = GetMockedUserService();
			var list = new List<User>();
			_mockUserRepo.Setup(u => u.SearchByEmail("blah")).ReturnsAsync(list);
			var result = await service.SearchByEmail("blah");
			Assert.Same(list, result);
			_mockUserRepo.Verify(u => u.SearchByEmail("blah"), Times.Once());
		}

		[Fact]
		public async Task SearchByName()
		{
			var service = GetMockedUserService();
			var list = new List<User>();
			_mockUserRepo.Setup(u => u.SearchByName("blah")).ReturnsAsync(list);
			var result = await service.SearchByName("blah");
			Assert.Same(list, result);
			_mockUserRepo.Verify(u => u.SearchByName("blah"), Times.Once());
		}

		[Fact]
		public async Task SearchByRole()
		{
			var service = GetMockedUserService();
			var list = new List<User>();
			_mockUserRepo.Setup(u => u.SearchByRole("blah")).ReturnsAsync(list);
			var result = await service.SearchByRole("blah");
			Assert.Same(list, result);
			_mockUserRepo.Verify(u => u.SearchByRole("blah"), Times.Once());
		}

		[Fact]
		public async Task GetUserEdit()
		{
			var service = GetMockedUserService();
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(new Profile { UserID = 1, Web = "blah"});
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
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(returnedProfile);
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);

			await service.EditUser(user, userEdit, false, false, null, null, "123", user);

			_mockUserRepo.Verify(u => u.ChangeEmail(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
			_mockUserRepo.Verify(u => u.ChangeName(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
			_mockUserRepo.Verify(u => u.SetHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
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
				TimeZone = userEdit.TimeZone,
				IsDaylightSaving = userEdit.IsDaylightSaving,
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
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(GetReturnedProfile(userEdit));
			await service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockUserRepo.Verify(u => u.UpdateIsApproved(user, true), Times.Once());
		}

		[Fact]
		public async Task EditUserNewEmail()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1, Email = "c@d.com" };
			user.Roles = new List<string>();
			var userEdit = new UserEdit { NewEmail = "a@b.com" };
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(GetReturnedProfile(userEdit));
			await service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockUserRepo.Verify(u => u.ChangeEmail(user, "a@b.com"), Times.Once());
		}

		[Fact]
		public async Task EditUserNewPassword()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string>();
			var userEdit = new UserEdit { NewPassword = "foo" };
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(GetReturnedProfile(userEdit));
			await service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockUserRepo.Verify(u => u.SetHashedPassword(user, It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
		}

		[Fact]
		public async Task EditUserAddRole()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string>();
			var userEdit = new UserEdit { Roles = new [] {"Admin", "Moderator"} };
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(GetReturnedProfile(userEdit));
			await service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockRoleRepo.Verify(r => r.ReplaceUserRoles(1, userEdit.Roles), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), user, "123", "Admin", SecurityLogType.UserAddedToRole), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), user, "123", "Moderator", SecurityLogType.UserAddedToRole), Times.Once());
		}

		[Fact]
		public async Task EditUserRemoveRole()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string> { "Admin", "Moderator" };
			var userEdit = new UserEdit { Roles = new[] { "SomethingElse" } };
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(GetReturnedProfile(userEdit));
			await service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockRoleRepo.Verify(r => r.ReplaceUserRoles(1, userEdit.Roles), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), user, "123", "Admin", SecurityLogType.UserRemovedFromRole), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), user, "123", "Moderator", SecurityLogType.UserRemovedFromRole), Times.Once());
		}

		[Fact]
		public async Task EditUserDeleteAvatar()
		{
			var service = GetMockedUserService();
			var user = new User {UserID = 1, Roles = new List<string>()};
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.AvatarID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			await service.EditUser(user, userEdit, true, false, null, null, "123", user);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
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
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			await service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
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
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			await service.EditUser(user, userEdit, false, true, null, null, "123", user);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
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
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			await service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
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
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(returnedProfile);
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).ReturnsAsync(true);
			_mockUserAvatarRepo.Setup(a => a.SaveNewAvatar(1, It.IsAny<byte[]>(), It.IsAny<DateTime>())).ReturnsAsync(12);
			var image = new byte[1];

			await service.EditUser(user, userEdit, false, false, image, null, "123", user);

			_mockUserAvatarRepo.Verify(a => a.SaveNewAvatar(1, image, It.IsAny<DateTime>()), Times.Once());
		}

		[Fact]
		public async Task EditUserNewPhoto()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string>();
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(returnedProfile);
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).ReturnsAsync(true);
			_mockUserImageRepo.Setup(a => a.SaveNewImage(1, 0, true, It.IsAny<byte[]>(), It.IsAny<DateTime>())).ReturnsAsync(12);
			var image = new byte[1];

			await service.EditUser(user, userEdit, false, false, null, image, "123", user);

			_mockUserImageRepo.Verify(a => a.SaveNewImage(1, 0, true, image, It.IsAny<DateTime>()), Times.Once());
		}

		[Fact]
		public async Task EditUserProfile()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string>();
			var returnedProfile = new Profile { UserID = 1 };
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(returnedProfile);
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			_mockTextParser.Setup(t => t.ForumCodeToHtml(It.IsAny<string>())).Returns("parsed");
			var userEdit = new UserEditProfile
			               	{
			               		Dob = new DateTime(2000,1,1), HideVanity = true, Instagram = "i", IsDaylightSaving = true, IsPlainText = true, IsSubscribed = true, Location = "l", Facebook = "fb", Twitter = "tw", ShowDetails = true, Signature = "s", TimeZone = -7, Web = "w"
			               	};
			await service.EditUserProfile(user, userEdit);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			Assert.Equal(new DateTime(2000, 1, 1), profile.Dob);
			Assert.True(profile.HideVanity);
			Assert.Equal("i", profile.Instagram);
			Assert.True(profile.IsDaylightSaving);
			Assert.True(profile.IsPlainText);
			Assert.True(profile.IsSubscribed);
			Assert.Equal("l", profile.Location);
			Assert.Equal("fb", profile.Facebook);
			Assert.Equal("tw", profile.Twitter);
			Assert.True(profile.ShowDetails);
			Assert.Equal("parsed", profile.Signature);
			Assert.Equal(-7, profile.TimeZone);
			Assert.Equal("w", profile.Web);
		}

		[Fact]
		public async Task UserEditPhotosDeleteAvatar()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.AvatarID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			await service.EditUserProfileImages(user, true, false, null, null);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			_mockUserAvatarRepo.Verify(u => u.DeleteAvatarsByUserID(user.UserID));
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
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			await service.EditUserProfileImages(user, false, false, null, null);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			_mockUserAvatarRepo.Verify(u => u.DeleteAvatarsByUserID(user.UserID), Times.Never());
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
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			await service.EditUserProfileImages(user, false, true, null, null);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			_mockUserImageRepo.Verify(u => u.DeleteImagesByUserID(user.UserID));
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
			_mockProfileRepo.Setup(p => p.GetProfile(1)).ReturnsAsync(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			await service.EditUserProfileImages(user, false, false, null, null);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			_mockUserImageRepo.Verify(u => u.DeleteImagesByUserID(user.UserID), Times.Never());
			Assert.Equal(3, profile.ImageID);
		}

		[Fact]
		public async Task GetUsersOnlineCallsRepo()
		{
			var service = GetMockedUserService();
			var users = new List<User>();
			_mockUserRepo.Setup(u => u.GetUsersOnline()).ReturnsAsync(users);
			var result = await service.GetUsersOnline();
			_mockUserRepo.Verify(u => u.GetUsersOnline(), Times.Once());
			Assert.Same(users, result);
		}

		[Fact]
		public async Task DeleteUserLogs()
		{
			var targetUser = new User { UserID = 1 };
			var user = new User { UserID = 2 };
			var service = GetMockedUserService();
			await service.DeleteUser(targetUser, user, "127.0.0.1", true);
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, "127.0.0.1", It.IsAny<string>(), SecurityLogType.UserDeleted));
		}

		[Fact]
		public async Task DeleteUserCallsRepo()
		{
			var targetUser = new User { UserID = 1 };
			var user = new User { UserID = 2 };
			var service = GetMockedUserService();
			await service.DeleteUser(targetUser, user, "127.0.0.1", true);
			_mockUserRepo.Verify(u => u.DeleteUser(targetUser), Times.Once());
		}

		[Fact]
		public async Task DeleteUserCallsBanRepoIfBanIsTrue()
		{
			var targetUser = new User { UserID = 1, Email = "a@b.com" };
			var user = new User { UserID = 2 };
			var service = GetMockedUserService();
			await service.DeleteUser(targetUser, user, "127.0.0.1", true);
			_mockBanRepo.Verify(b => b.BanEmail(targetUser.Email), Times.Once());
		}

		[Fact]
		public async Task DeleteUserDoesNotCallBanRepoIfBanIsFalse()
		{
			var targetUser = new User { UserID = 1, Email = "a@b.com" };
			var user = new User { UserID = 2 };
			var service = GetMockedUserService();
			await service.DeleteUser(targetUser, user, "127.0.0.1", false);
			_mockBanRepo.Verify(b => b.BanEmail(targetUser.Email), Times.Never());
		}

		[Fact]
		public async Task ForgotPasswordCallsMailerForGoodUser()
		{
			var user = new User { UserID = 2, Email = "a@b.com" };
			var service = GetMockedUserService();
			_mockUserRepo.Setup(u => u.GetUserByEmail(user.Email)).ReturnsAsync(user);
			await service.GeneratePasswordResetEmail(user, "http");
			_mockForgotMailer.Verify(f => f.ComposeAndQueue(user, It.IsAny<string>()), Times.Exactly(1));
		}

		[Fact]
		public async Task ForgotPasswordGeneratesNewAuthKey()
		{
			var user = new User { UserID = 2, Email = "a@b.com" };
			var service = GetMockedUserService();
			_mockUserRepo.Setup(u => u.GetUserByEmail(user.Email)).ReturnsAsync(user);
			await service.GeneratePasswordResetEmail(user, "http");
			_mockUserRepo.Verify(u => u.UpdateAuthorizationKey(user, It.IsAny<Guid>()), Times.Exactly(1));
		}

		[Fact]
		public async Task ForgotPasswordThrowsForNoUser()
		{
			var service = GetMockedUserService();
			_mockUserRepo.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((User)null);
			await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.GeneratePasswordResetEmail(null, "http"));
			_mockForgotMailer.Verify(f => f.ComposeAndQueue(It.IsAny<User>(), It.IsAny<string>()), Times.Exactly(0));
		}
	}
}
