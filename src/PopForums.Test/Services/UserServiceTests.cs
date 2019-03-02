using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
			_mockRoleRepo.Setup(r => r.GetUserRoles(It.IsAny<int>())).Returns(new List<string>());
			return new UserService(_mockUserRepo.Object, _mockRoleRepo.Object, _mockProfileRepo.Object, _mockSettingsManager.Object, _mockUserAvatarRepo.Object, _mockUserImageRepo.Object, _mockSecurityLogService.Object, _mockTextParser.Object, _mockBanRepo.Object, _mockForgotMailer.Object, _mockImageService.Object);
		}

		[Fact]
		public void SetPassword()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("jeff", "a@b.com");
			var salt = Guid.NewGuid();
			_mockUserRepo.Setup(x => x.SetHashedPassword(user, It.IsAny<string>(), It.IsAny<Guid>())).Callback<User, string, Guid>((u, p, s) => salt = s);

			userService.SetPassword(user, "fred", String.Empty, user);

			var hashedPassword = "fred".GetMD5Hash(salt);
			_mockUserRepo.Verify(r => r.SetHashedPassword(user, hashedPassword, salt));
		}

		[Fact]
		public void CheckPassword()
		{
			var userService = GetMockedUserService();
			Guid? salt;
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(String.Empty, out salt)).Returns("VwqQv7+MfqtdxdTiaDLVsQ==");

			Assert.True(userService.CheckPassword(String.Empty, "fred", out salt));
		}

		[Fact]
		public void CheckPasswordFail()
		{
			var userService = GetMockedUserService();
			Guid? salt;
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(String.Empty, out salt)).Returns("VwqQv7+MfqtdxdTiaDLVsQ==");

			Assert.False(userService.CheckPassword(String.Empty, "fsdfsdfsdfsdf", out salt));
		}

		[Fact]
		public void CheckPasswordHasSalt()
		{
			var userService = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			var hashedPassword = "fred".GetMD5Hash(salt.Value);
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(String.Empty, out salt)).Returns(hashedPassword);

			Assert.True(userService.CheckPassword(String.Empty, "fred", out salt));
		}

		[Fact]
		public void CheckPasswordHasSaltFail()
		{
			var userService = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			var hashedPassword = "fred".GetMD5Hash(salt.Value);
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(String.Empty, out salt)).Returns(hashedPassword);

			Assert.False(userService.CheckPassword(String.Empty, "dsfsdfsdfsdf", out salt));
		}

		[Fact]
		public void GetUser()
		{
			const int id = 1;
			const string name = "Jeff";
			const string email = "a@b.com";
			var roles = new List<string> {"blah", PermanentRoles.Admin};
			var userService = GetMockedUserService();
			var dummyUser = GetDummyUser(name, email);
			_mockUserRepo.Setup(r => r.GetUser(id)).Returns(dummyUser);
			_mockRoleRepo.Setup(r => r.GetUserRoles(id)).Returns(roles);
			var user = userService.GetUser(id);
			Assert.Same(dummyUser, user);
		}

		[Fact]
		public void GetUserFail()
		{
			const int id = 1;
			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUser(It.Is<int>(i => i != 1))).Returns(GetDummyUser("", ""));
			_mockUserRepo.Setup(r => r.GetUser(It.Is<int>(i => i == 1))).Returns((User)null);
			var user = userService.GetUser(id);
			Assert.Null(user);
		}

		[Fact]
		public void GetUserByName()
		{
			const string name = "Jeff";
			const string email = "a@b.com";
			var roles = new List<string> { "blah", PermanentRoles.Admin };
			var dummyUser = GetDummyUser(name, email);
			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByName(name)).Returns(dummyUser);
			_mockRoleRepo.Setup(r => r.GetUserRoles(dummyUser.UserID)).Returns(roles);
			var user = userService.GetUserByName(name);
			Assert.Same(dummyUser, user);
		}

		[Fact]
		public void GetUserByNameFail()
		{
			const string name = "Jeff";
			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByName(It.Is<string>(i => i != name))).Returns(GetDummyUser(name, ""));
			_mockUserRepo.Setup(r => r.GetUserByName(It.Is<string>(i => i == name))).Returns((User)null);
			var user = userService.GetUserByName(name);
			Assert.Null(user);
		}

		[Fact]
		public void GetUserByNameReturnsNullWithNullOrEmptyName()
		{
			var userService = GetMockedUserService();
			Assert.Null(userService.GetUserByName(""));
			Assert.Null(userService.GetUserByName(null));
		}

		[Fact]
		public void GetUserByEmail()
		{
			const string name = "Jeff";
			const string email = "a@b.com";
			var roles = new List<string> { "blah", PermanentRoles.Admin };
			var dummyUser = GetDummyUser(name, email);
			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByEmail(email)).Returns(dummyUser);
			_mockRoleRepo.Setup(r => r.GetUserRoles(dummyUser.UserID)).Returns(roles);
			var user = userService.GetUserByEmail(email);
			Assert.Same(dummyUser, user);
		}

		[Fact]
		public void GetUserByEmailFail()
		{
			const string email = "a@b.com";
			var userManager = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByEmail(It.Is<string>(i => i != email))).Returns(GetDummyUser("", email));
			_mockUserRepo.Setup(r => r.GetUserByEmail(It.Is<string>(i => i == email))).Returns((User)null);
			var user = userManager.GetUserByEmail(email);
			Assert.Null(user);
		}

		public static User GetDummyUser(string name, string email)
		{
			var almostNow = DateTime.UtcNow.AddDays(-1);
			return new User { UserID = 1, Name = name, Email = email, IsApproved = true, LastActivityDate = almostNow, LastLoginDate = almostNow, AuthorizationKey = new Guid()};
		}

		[Fact]
		public void NameIsInUse()
		{
			const string name = "jeff";
			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByName(It.IsRegex("^" + name + "$", RegexOptions.IgnoreCase))).Returns(GetDummyUser(name, "a@b.com"));
			Assert.True(userService.IsNameInUse(name));
			_mockUserRepo.Verify(r => r.GetUserByName(name), Times.Exactly(1));
			Assert.False(userService.IsNameInUse("notjeff"));
			Assert.True(userService.IsNameInUse(name.ToUpper()));
		}

		[Fact]
		public void EmailIsInUse()
		{
			const string email = "a@b.com";
			var userManager = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByEmail(It.IsRegex("^" + email + "$", RegexOptions.IgnoreCase))).Returns(GetDummyUser("jeff", email));
			Assert.True(userManager.IsEmailInUse(email));
			_mockUserRepo.Verify(r => r.GetUserByEmail(email), Times.Exactly(1));
			Assert.False(userManager.IsEmailInUse("nota@b.com"));
			Assert.True(userManager.IsEmailInUse(email.ToUpper()));
		}

		[Fact]
		public void EmailInUserByAnotherTrue()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("jeff", "a@b.com");
			_mockUserRepo.Setup(u => u.GetUserByEmail("c@d.com")).Returns(new User { UserID = 123 });
			var result = userService.IsEmailInUseByDifferentUser(user, "c@d.com");
			_mockUserRepo.Verify(u => u.GetUserByEmail("c@d.com"), Times.Once());
			Assert.True(result);
		}

		[Fact]
		public void EmailInUserByAnotherFalseBecauseSameUser()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("jeff", "a@b.com");
			_mockUserRepo.Setup(u => u.GetUserByEmail("a@b.com")).Returns(user);
			var result = userService.IsEmailInUseByDifferentUser(user, "a@b.com");
			_mockUserRepo.Verify(u => u.GetUserByEmail("a@b.com"), Times.Once());
			Assert.False(result);
		}

		[Fact]
		public void EmailInUserByAnotherFalseBecauseNoUser()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("jeff", "a@b.com");
			_mockUserRepo.Setup(u => u.GetUserByEmail("c@d.com")).Returns((User)null);
			var result = userService.IsEmailInUseByDifferentUser(user, "c@d.com");
			_mockUserRepo.Verify(u => u.GetUserByEmail("c@d.com"), Times.Once());
			Assert.False(result);
		}

		[Fact]
		public void CreateUser()
		{
			const string name = "jeff";
			const string nameCensor = "jeffcensor";
			const string email = "a@b.com";
			const string password = "fred";
			const string ip = "127.0.0.1";
			var userService = GetMockedUserService();
			var dummyUser = GetDummyUser(nameCensor, email);
			_mockUserRepo.Setup(r => r.CreateUser(nameCensor, email, It.IsAny<DateTime>(), true, It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(dummyUser);
			_mockTextParser.Setup(t => t.Censor(name)).Returns(nameCensor);
			var user = userService.CreateUser(name, email, password, true, ip);
			Assert.Equal(dummyUser.Name, user.Name);
			Assert.Equal(dummyUser.Email, user.Email);
			_mockTextParser.Verify(t => t.Censor(name), Times.Once());
			_mockUserRepo.Verify(r => r.CreateUser(nameCensor, email, It.IsAny<DateTime>(), true, It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.UserCreated));
		}


		[Fact]
		public void CreateUserFromSignup()
		{
			const string name = "jeff";
			const string nameCensor = "jeffcensor";
			const string email = "a@b.com";
			const string password = "fred";
			const string ip = "127.0.0.1";
			var userManager = GetMockedUserService();
			var dummyUser = GetDummyUser(nameCensor, email);
			_mockUserRepo.Setup(r => r.CreateUser(nameCensor, email, It.IsAny<DateTime>(), true, It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(dummyUser);
			_mockTextParser.Setup(t => t.Censor(name)).Returns(nameCensor);
			var settings = new Settings();
			_mockSettingsManager.Setup(s => s.Current).Returns(settings);
			var signUpdata = new SignupData {Email = email, Name = name, Password = password};
			var user = userManager.CreateUser(signUpdata, ip);
			_mockUserRepo.Verify(r => r.CreateUser(nameCensor, email, It.IsAny<DateTime>(), true, It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.UserCreated));
		}

		[Fact]
		public void CreateInvalidEmail()
		{
			var userService = GetMockedUserService();
			_mockTextParser.Setup(t => t.Censor(It.IsAny<string>())).Returns("blah");
			Assert.Throws<Exception>(() => userService.CreateUser("", "a b@oihfwe", "", true, ""));
		}

		[Fact]
		public void CreateUsedName()
		{
			const string usedName = "jeff";
			const string email = "a@b.com";
			var userService = GetMockedUserService();
			_mockTextParser.Setup(t => t.Censor("jeff")).Returns("jeff");
			_mockTextParser.Setup(t => t.Censor("anynamejeff")).Returns("anynamejeff");
			_mockUserRepo.Setup(r => r.GetUserByName(It.IsRegex("^" + usedName + "$", RegexOptions.IgnoreCase))).Returns(GetDummyUser(usedName, email));
			Assert.Throws<Exception>(() => userService.CreateUser(usedName, email, "", true, ""));
			Assert.Throws<Exception>(() => userService.CreateUser(usedName.ToUpper(), email, "", true, ""));
		}

		[Fact]
		public void CreateNameNull()
		{
			var userManager = GetMockedUserService();
			Assert.Throws<Exception>(() => userManager.CreateUser(null, "a@b.com", "", true, ""));
		}

		[Fact]
		public void CreateNameEmpty()
		{
			var userManager = GetMockedUserService();
			Assert.Throws<Exception>(() => userManager.CreateUser(String.Empty, "a@b.com", "", true, ""));
		}

		[Fact]
		public void CreateUsedEmail()
		{
			const string usedEmail = "a@b.com";
			var userService = GetMockedUserService();
			_mockTextParser.Setup(t => t.Censor(It.IsAny<string>())).Returns("blah");
			_mockUserRepo.Setup(r => r.GetUserByEmail(It.IsRegex("^" + usedEmail + "$", RegexOptions.IgnoreCase))).Returns(GetDummyUser("jeff", usedEmail));
			Assert.Throws<Exception>(() => userService.CreateUser("", usedEmail, "", true, ""));
		}

		[Fact]
		public void CreateEmailBanned()
		{
			const string bannedEmail = "a@b.com";
			var userService = GetMockedUserService();
			_mockTextParser.Setup(t => t.Censor(It.IsAny<string>())).Returns("blah");
			_mockBanRepo.Setup(b => b.EmailIsBanned(bannedEmail)).Returns(true);
			Assert.Throws<Exception>(() => userService.CreateUser("name", bannedEmail, "", true, ""));
		}

		[Fact]
		public void CreateIPBanned()
		{
			const string bannedIP = "1.2.3.4";
			var userManager = GetMockedUserService();
			_mockTextParser.Setup(t => t.Censor(It.IsAny<string>())).Returns("blah");
			_mockBanRepo.Setup(b => b.IPIsBanned(bannedIP)).Returns(true);
			Assert.Throws<Exception>(() => userManager.CreateUser("", "a@b.com", "", true, bannedIP));
		}

		[Fact]
		public void UpdateLastActivityDate()
		{
			var userManager = GetMockedUserService();
			var user = UserTest.GetTestUser();
			var oldActivityDate = user.LastActivityDate;
			userManager.UpdateLastActicityDate(user);
			_mockUserRepo.Verify(r => r.UpdateLastActivityDate(user, It.IsAny<DateTime>()), Times.Once());
			Assert.NotEqual(oldActivityDate, user.LastActivityDate);
		}

		[Fact]
		public void ChangeEmailSuccess()
		{
			const string oldName = "Jeff";
			const string oldEmail = "a@b.com";
			const string newEmail = "c@d.com";

			var userManager = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByEmail(oldEmail)).Returns(GetDummyUser(oldName, oldEmail));
			_mockUserRepo.Setup(r => r.GetUserByEmail(newEmail)).Returns((User)null);
			_mockSettingsManager.Setup(x => x.Current.IsNewUserApproved).Returns(false);
			var targetUser = GetDummyUser(oldName, oldEmail);
			var user = new User { UserID = 34243 };
			userManager.ChangeEmail(targetUser, newEmail, user, "123");
			_mockUserRepo.Verify(r => r.ChangeEmail(targetUser, newEmail), Times.Exactly(1));
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, "123", "Old: a@b.com, New: c@d.com", SecurityLogType.EmailChange));
		}

		[Fact]
		public void ChangeEmailAlreadyInUse()
		{
			const string oldName = "Jeff";
			const string oldEmail = "a@b.com";
			const string newEmail = "c@d.com";

			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByEmail(oldEmail)).Returns(GetDummyUser(oldName, oldEmail));
			_mockUserRepo.Setup(r => r.GetUserByEmail(newEmail)).Returns(GetDummyUser("Diana", newEmail));
			_mockSettingsManager.Setup(x => x.Current.IsNewUserApproved).Returns(true);
			var user = GetDummyUser(oldName, oldEmail);
			Assert.Throws<Exception>(() => userService.ChangeEmail(user, newEmail, It.IsAny<User>(), It.IsAny<string>()));
			_mockUserRepo.Verify(r => r.ChangeEmail(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Fact]
		public void ChangeEmailBad()
		{
			const string badEmail = "a b @ c";
			var userManager = GetMockedUserService();
			_mockSettingsManager.Setup(x => x.Current.IsNewUserApproved).Returns(true);
			var user = GetDummyUser("", "");
			Assert.Throws<Exception>(() => userManager.ChangeEmail(user, badEmail, It.IsAny<User>(), It.IsAny<string>()));
			_mockUserRepo.Verify(r => r.ChangeEmail(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Fact]
		public void ChangeEmailMapsIsApprovedFromSettingsToUserRepoCall()
		{
			const string oldName = "Jeff";
			const string oldEmail = "a@b.com";
			const string newEmail = "c@d.com";

			var userManager = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByEmail(oldEmail)).Returns(GetDummyUser(oldName, oldEmail));
			_mockUserRepo.Setup(r => r.GetUserByEmail(newEmail)).Returns((User)null);
			_mockSettingsManager.Setup(x => x.Current.IsNewUserApproved).Returns(true);
			var targetUser = GetDummyUser(oldName, oldEmail);
			var user = new User { UserID = 34243 };
			userManager.ChangeEmail(targetUser, newEmail, user, "123");
			_mockUserRepo.Verify(x => x.UpdateIsApproved(targetUser, true), Times.Once());
		}

		[Fact]
		public void ChangeNameSuccess()
		{
			const string oldName = "Jeff";
			const string oldEmail = "a@b.com";
			const string newName = "Diana";

			var userManager = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByName(oldName)).Returns(GetDummyUser(oldName, oldEmail));
			_mockUserRepo.Setup(r => r.GetUserByName(newName)).Returns((User)null);
			var targetUser = GetDummyUser(oldName, oldEmail);
			var user = new User { UserID = 1234531 };
			userManager.ChangeName(targetUser, newName, user, "123");
			_mockUserRepo.Verify(r => r.ChangeName(targetUser, newName));
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, "123", "Old: Jeff, New: Diana", SecurityLogType.NameChange));
		}

		[Fact]
		public void ChangeNameFailUsed()
		{
			const string oldName = "Jeff";
			const string oldEmail = "a@b.com";
			const string newName = "Diana";

			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByName(oldName)).Returns(GetDummyUser(oldName, oldEmail));
			_mockUserRepo.Setup(r => r.GetUserByName(newName)).Returns(GetDummyUser(newName, oldEmail));
			var user = GetDummyUser(oldName, oldEmail);
			Assert.Throws<Exception>(() => userService.ChangeName(user, newName, It.IsAny<User>(), It.IsAny<string>()));
			_mockUserRepo.Verify(r => r.ChangeName(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Fact]
		public void ChangeNameNull()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("Jeff", "a@b.com");
			Assert.Throws<Exception>(() => userService.ChangeName(user, null, It.IsAny<User>(), It.IsAny<string>()));
			_mockUserRepo.Verify(r => r.ChangeName(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Fact]
		public void ChangeNameEmpty()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("Jeff", "a@b.com");
			Assert.Throws<Exception>(() => userService.ChangeName(user, String.Empty, It.IsAny<User>(), It.IsAny<string>()));
			_mockUserRepo.Verify(r => r.ChangeName(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Fact]
		public void Logout()
		{
			var userService = GetMockedUserService();
			var user = UserTest.GetTestUser();
			userService.Logout(user, "123");
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, "123", String.Empty, SecurityLogType.Logout));
		}

		[Fact]
		public void LoginSuccess()
		{
			const string email = "a@b.com";
			const string password = "fred";
			const string ip = "1.1.1.1";
			var user = UserTest.GetTestUser();
			var userService = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			var saltedHash = password.GetMD5Hash(salt.Value);
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(email, out salt)).Returns(saltedHash);
			_mockUserRepo.Setup(r => r.GetUserByEmail(email)).Returns(user);
			_mockUserRepo.Setup(r => r.UpdateLastLoginDate(user, It.IsAny<DateTime>()));
			User userOut;

			var result = userService.Login(email, password, ip, out userOut);

			Assert.True(result);
			_mockUserRepo.Verify(r => r.UpdateLastLoginDate(user, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, ip, "", SecurityLogType.Login), Times.Once());
			_mockUserRepo.Verify(x => x.SetHashedPassword(user, It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
		}

		[Fact]
		public void LoginSuccessNoSalt()
		{
			const string email = "a@b.com";
			const string password = "fred";
			const string ip = "1.1.1.1";
			var user = UserTest.GetTestUser();
			var userService = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			Guid? nosalt;
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(email, out nosalt)).Returns(password.GetMD5Hash());
			_mockUserRepo.Setup(r => r.GetUserByEmail(email)).Returns(user);
			_mockUserRepo.Setup(r => r.UpdateLastLoginDate(user, It.IsAny<DateTime>()));
			_mockUserRepo.Setup(x => x.SetHashedPassword(user, It.IsAny<string>(), It.IsAny<Guid>())).Callback<User, string, Guid>((u, p, s) => salt = s);
			User userOut;

			var result = userService.Login(email, password, ip, out userOut);

			Assert.True(result);
			_mockUserRepo.Verify(r => r.UpdateLastLoginDate(user, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, ip, "", SecurityLogType.Login), Times.Once());
			var saltyPassword = password.GetMD5Hash(salt.Value);
			_mockUserRepo.Verify(x => x.SetHashedPassword(user, saltyPassword, salt.Value), Times.Once());
		}

		[Fact]
		public void LoginFail()
		{
			const string email = "a@b.com";
			const string password = "fred";
			const string ip = "1.1.1.1";
			var userService = GetMockedUserService();
			Guid? salt;
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(It.IsAny<string>(), out salt)).Returns("1234");
			User userOut;

			var result = userService.Login(email, password, ip, out userOut);

			Assert.False(result);
			_mockSecurityLogService.Verify(s => s.CreateLogEntry((User)null, null, ip, "E-mail attempted: " + email, SecurityLogType.FailedLogin), Times.Once());
			_mockUserRepo.Verify(x => x.SetHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
		}

		[Fact]
		public void LoginWithUser()
		{
			var user = UserTest.GetTestUser();
			var service = GetMockedUserService();
			const string ip = "1.1.1.1";

			service.Login(user, ip);
			
			_mockUserRepo.Verify(u => u.UpdateLastLoginDate(user, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.Login), Times.Once());
		}

		[Fact]
		public void LoginWithUserPersistCookie()
		{
			var user = UserTest.GetTestUser();
			var service = GetMockedUserService();
			const string ip = "1.1.1.1";

			service.Login(user, ip);

			_mockUserRepo.Verify(u => u.UpdateLastLoginDate(user, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.Login), Times.Once());
		}

		[Fact]
		public void GetAllRoles()
		{
			var userService = GetMockedUserService();
			var list = new List<string>();
			_mockRoleRepo.Setup(r => r.GetAllRoles()).Returns(list);
			var result = userService.GetAllRoles();
			_mockRoleRepo.Verify(r => r.GetAllRoles(), Times.Once());
			Assert.Same(list, result);
		}

		[Fact]
		public void CreateRole()
		{
			var userService = GetMockedUserService();
			const string role = "blah";
			userService.CreateRole(role, It.IsAny<User>(), It.IsAny<string>());
			_mockRoleRepo.Verify(r => r.CreateRole(role), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), null, It.IsAny<string>(), "Role: blah", SecurityLogType.RoleCreated), Times.Once());
		}

		[Fact]
		public void DeleteRole()
		{
			var userService = GetMockedUserService();
			const string role = "blah";
			userService.DeleteRole(role, It.IsAny<User>(), It.IsAny<string>());
			_mockRoleRepo.Verify(r => r.DeleteRole(role), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), null, It.IsAny<string>(), "Role: blah", SecurityLogType.RoleDeleted), Times.Once());
		}

		[Fact]
		public void DeleteRoleThrowsOnAdminOrMod()
		{
			var userService = GetMockedUserService();
			Assert.Throws<InvalidOperationException>(() => userService.DeleteRole("Admin", It.IsAny<User>(), It.IsAny<string>()));
			Assert.Throws<InvalidOperationException>(() => userService.DeleteRole("Moderator", It.IsAny<User>(), It.IsAny<string>()));
			Assert.Throws<InvalidOperationException>(() => userService.DeleteRole("admin", It.IsAny<User>(), It.IsAny<string>()));
			Assert.Throws<InvalidOperationException>(() => userService.DeleteRole("moderator", It.IsAny<User>(), It.IsAny<string>()));
			_mockRoleRepo.Verify(r => r.DeleteRole(It.IsAny<string>()), Times.Never());
		}

		[Fact]
		public void UpdateIsApproved()
		{
			var userService = GetMockedUserService();
			var targetUser = GetDummyUser("Jeff", "a@b.com");
			var user = new User { UserID = 97 };
			userService.UpdateIsApproved(targetUser, true, user, "123");
			_mockUserRepo.Verify(u => u.UpdateIsApproved(targetUser, true), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, It.IsAny<string>(), String.Empty, SecurityLogType.IsApproved));
		}

		[Fact]
		public void UpdateIsApprovedFalse()
		{
			var userService = GetMockedUserService();
			var targetUser = GetDummyUser("Jeff", "a@b.com");
			var user = new User { UserID = 97 };
			userService.UpdateIsApproved(targetUser, false, user, "123");
			_mockUserRepo.Verify(u => u.UpdateIsApproved(targetUser, false), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, It.IsAny<string>(), String.Empty, SecurityLogType.IsNotApproved));
		}

		[Fact]
		public void UpdateAuthKey()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("Jeff", "a@b.com");
			var key = Guid.NewGuid();
			userService.UpdateAuthorizationKey(user, key);
			_mockUserRepo.Verify(u => u.UpdateAuthorizationKey(user, key), Times.Once());
		}

		[Fact]
		public void VerifyUserByAuthKey()
		{
			var key = Guid.NewGuid();
			const string name = "Jeff";
			const string email = "a@b.com";
			var dummyUser = GetDummyUser(name, email);
			dummyUser.AuthorizationKey = key;
			var userManager = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByAuthorizationKey(key)).Returns(dummyUser);
			var user = userManager.VerifyAuthorizationCode(dummyUser.AuthorizationKey, "123");
			Assert.Same(dummyUser, user);
			_mockUserRepo.Verify(u => u.UpdateIsApproved(dummyUser, true), Times.Once());
		}

		[Fact]
		public void VerifyUserByAuthKeyFail()
		{
			var service = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByAuthorizationKey(It.IsAny<Guid>())).Returns((User)null);
			var user = service.VerifyAuthorizationCode(Guid.NewGuid(), "123");
			Assert.Null(user);
			_mockUserRepo.Verify(u => u.UpdateIsApproved(It.IsAny<User>(), true), Times.Never());
		}

		[Fact]
		public void SearchByEmail()
		{
			var service = GetMockedUserService();
			var list = new List<User>();
			_mockUserRepo.Setup(u => u.SearchByEmail("blah")).Returns(list);
			var result = service.SearchByEmail("blah");
			Assert.Same(list, result);
			_mockUserRepo.Verify(u => u.SearchByEmail("blah"), Times.Once());
		}

		[Fact]
		public void SearchByName()
		{
			var service = GetMockedUserService();
			var list = new List<User>();
			_mockUserRepo.Setup(u => u.SearchByName("blah")).Returns(list);
			var result = service.SearchByName("blah");
			Assert.Same(list, result);
			_mockUserRepo.Verify(u => u.SearchByName("blah"), Times.Once());
		}

		[Fact]
		public void SearchByRole()
		{
			var service = GetMockedUserService();
			var list = new List<User>();
			_mockUserRepo.Setup(u => u.SearchByRole("blah")).Returns(list);
			var result = service.SearchByRole("blah");
			Assert.Same(list, result);
			_mockUserRepo.Verify(u => u.SearchByRole("blah"), Times.Once());
		}

		[Fact]
		public void GetUserEdit()
		{
			var service = GetMockedUserService();
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(new Profile { UserID = 1, Web = "blah"});
			var user = new User { UserID = 1 };
			user.Roles = new List<string>();
			var result = service.GetUserEdit(user);
			Assert.Equal(1, result.UserID);
			Assert.Equal("blah", result.Web);
		}

		[Fact]
		public void EditUserProfileOnly()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string>();
			var userEdit = new UserEdit();
			var profile = new Profile();
			var returnedProfile = GetReturnedProfile(userEdit);
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);

			service.EditUser(user, userEdit, false, false, null, null, "123", user);

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
		public void EditUserApprovalChange()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1, IsApproved = false};
			user.Roles = new List<string>();
			var userEdit = new UserEdit {IsApproved = true};
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(GetReturnedProfile(userEdit));
			service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockUserRepo.Verify(u => u.UpdateIsApproved(user, true), Times.Once());
		}

		[Fact]
		public void EditUserNewEmail()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1, Email = "c@d.com" };
			user.Roles = new List<string>();
			var userEdit = new UserEdit { NewEmail = "a@b.com" };
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(GetReturnedProfile(userEdit));
			service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockUserRepo.Verify(u => u.ChangeEmail(user, "a@b.com"), Times.Once());
		}

		[Fact]
		public void EditUserNewPassword()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string>();
			var userEdit = new UserEdit { NewPassword = "foo" };
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(GetReturnedProfile(userEdit));
			service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockUserRepo.Verify(u => u.SetHashedPassword(user, It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
		}

		[Fact]
		public void EditUserAddRole()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string>();
			var userEdit = new UserEdit { Roles = new [] {"Admin", "Moderator"} };
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(GetReturnedProfile(userEdit));
			service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockRoleRepo.Verify(r => r.ReplaceUserRoles(1, userEdit.Roles), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), user, "123", "Admin", SecurityLogType.UserAddedToRole), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), user, "123", "Moderator", SecurityLogType.UserAddedToRole), Times.Once());
		}

		[Fact]
		public void EditUserRemoveRole()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string> { "Admin", "Moderator" };
			var userEdit = new UserEdit { Roles = new[] { "SomethingElse" } };
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(GetReturnedProfile(userEdit));
			service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockRoleRepo.Verify(r => r.ReplaceUserRoles(1, userEdit.Roles), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), user, "123", "Admin", SecurityLogType.UserRemovedFromRole), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), user, "123", "Moderator", SecurityLogType.UserRemovedFromRole), Times.Once());
		}

		[Fact]
		public void EditUserDeleteAvatar()
		{
			var service = GetMockedUserService();
			var user = new User {UserID = 1, Roles = new List<string>()};
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.AvatarID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUser(user, userEdit, true, false, null, null, "123", user);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			Assert.Null(profile.AvatarID);
		}

		[Fact]
		public void EditUserNoDeleteAvatar()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string>();
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.AvatarID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			Assert.Equal(3, profile.AvatarID);
		}

		[Fact]
		public void EditUserDeletePhoto()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string>();
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.ImageID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUser(user, userEdit, false, true, null, null, "123", user);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			Assert.Null(profile.ImageID);
		}

		[Fact]
		public void EditUserNoDeletePhoto()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string>();
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.ImageID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			Assert.Equal(3, profile.ImageID);
		}

		[Fact]
		public void EditUserNewAvatar()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string>();
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Returns(true);
			_mockUserAvatarRepo.Setup(a => a.SaveNewAvatar(1, It.IsAny<byte[]>(), It.IsAny<DateTime>())).Returns(12);
			var image = new byte[1];

			service.EditUser(user, userEdit, false, false, image, null, "123", user);

			_mockUserAvatarRepo.Verify(a => a.SaveNewAvatar(1, image, It.IsAny<DateTime>()), Times.Once());
		}

		[Fact]
		public void EditUserNewPhoto()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string>();
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Returns(true);
			_mockUserImageRepo.Setup(a => a.SaveNewImage(1, 0, true, It.IsAny<byte[]>(), It.IsAny<DateTime>())).Returns(12);
			var image = new byte[1];

			service.EditUser(user, userEdit, false, false, null, image, "123", user);

			_mockUserImageRepo.Verify(a => a.SaveNewImage(1, 0, true, image, It.IsAny<DateTime>()), Times.Once());
		}

		[Fact]
		public void EditUserProfile()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			user.Roles = new List<string>();
			var returnedProfile = new Profile { UserID = 1 };
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			_mockTextParser.Setup(t => t.ForumCodeToHtml(It.IsAny<string>())).Returns("parsed");
			var userEdit = new UserEditProfile
			               	{
			               		Dob = new DateTime(2000,1,1), HideVanity = true, Instagram = "i", IsDaylightSaving = true, IsPlainText = true, IsSubscribed = true, Location = "l", Facebook = "fb", Twitter = "tw", ShowDetails = true, Signature = "s", TimeZone = -7, Web = "w"
			               	};
			service.EditUserProfile(user, userEdit);
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
		public void VerifyPasswordSuccess()
		{
			const string password = "blah";
			var hashed = password.GetMD5Hash();
			var service = GetMockedUserService();
			Guid? salt;
			_mockUserRepo.Setup(u => u.GetHashedPasswordByEmail("a@b.com", out salt)).Returns(hashed);

			var result = service.VerifyPassword(new User { UserID = 123, Email = "a@b.com"}, password);

			_mockUserRepo.Verify(u => u.GetHashedPasswordByEmail("a@b.com", out salt), Times.Once());
			Assert.True(result);
		}

		[Fact]
		public void VerifyPasswordWithSaltSuccess()
		{
			const string password = "blah";
			var service = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			var hashed = password.GetMD5Hash(salt.Value);
			_mockUserRepo.Setup(u => u.GetHashedPasswordByEmail("a@b.com", out salt)).Returns(hashed);

			var result = service.VerifyPassword(new User { UserID = 1, Email = "a@b.com" }, password);

			_mockUserRepo.Verify(u => u.GetHashedPasswordByEmail("a@b.com", out salt), Times.Once());
			Assert.True(result);
		}

		[Fact]
		public void VerifyPasswordFail()
		{
			const string password = "blah";
			var service = GetMockedUserService();
			Guid? salt;
			_mockUserRepo.Setup(u => u.GetHashedPasswordByEmail("a@b.com", out salt)).Returns("2233435");

			var result = service.VerifyPassword(new User { UserID = 1, Email = "a@b.com" }, password);

			_mockUserRepo.Verify(u => u.GetHashedPasswordByEmail("a@b.com", out salt), Times.Once());
			Assert.False(result);
		}

		[Fact]
		public void VerifyPasswordWithSaltFail()
		{
			const string password = "blah";
			var service = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			_mockUserRepo.Setup(u => u.GetHashedPasswordByEmail("a@b.com", out salt)).Returns("2233435");

			var result = service.VerifyPassword(new User { UserID = 1, Email = "a@b.com" }, password);

			_mockUserRepo.Verify(u => u.GetHashedPasswordByEmail("a@b.com", out salt), Times.Once());
			Assert.False(result);
		}

		[Fact]
		public void UserEditPhotosDeleteAvatar()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.AvatarID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUserProfileImages(user, true, false, null, null);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			_mockUserAvatarRepo.Verify(u => u.DeleteAvatarsByUserID(user.UserID));
			Assert.Null(profile.AvatarID);
		}

		[Fact]
		public void UserEditPhotosNoDeleteAvatar()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.AvatarID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUserProfileImages(user, false, false, null, null);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			_mockUserAvatarRepo.Verify(u => u.DeleteAvatarsByUserID(user.UserID), Times.Never());
			Assert.Equal(3, profile.AvatarID);
		}

		[Fact]
		public void UserEditPhotosDeletePhoto()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.ImageID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUserProfileImages(user, false, true, null, null);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			_mockUserImageRepo.Verify(u => u.DeleteImagesByUserID(user.UserID));
			Assert.Null(profile.ImageID);
		}

		[Fact]
		public void UserEditPhotosNoDeletePhoto()
		{
			var service = GetMockedUserService();
			var user = new User { UserID = 1 };
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.ImageID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUserProfileImages(user, false, false, null, null);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			_mockUserImageRepo.Verify(u => u.DeleteImagesByUserID(user.UserID), Times.Never());
			Assert.Equal(3, profile.ImageID);
		}

		[Fact]
		public void GetUsersOnlineCallsRepo()
		{
			var service = GetMockedUserService();
			var users = new List<User>();
			_mockUserRepo.Setup(u => u.GetUsersOnline()).Returns(users);
			var result = service.GetUsersOnline();
			_mockUserRepo.Verify(u => u.GetUsersOnline(), Times.Once());
			Assert.Same(users, result);
		}

		[Fact]
		public void DeleteUserLogs()
		{
			var targetUser = new User { UserID = 1 };
			var user = new User { UserID = 2 };
			var service = GetMockedUserService();
			service.DeleteUser(targetUser, user, "127.0.0.1", true);
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, "127.0.0.1", It.IsAny<string>(), SecurityLogType.UserDeleted));
		}

		[Fact]
		public void DeleteUserCallsRepo()
		{
			var targetUser = new User { UserID = 1 };
			var user = new User { UserID = 2 };
			var service = GetMockedUserService();
			service.DeleteUser(targetUser, user, "127.0.0.1", true);
			_mockUserRepo.Verify(u => u.DeleteUser(targetUser), Times.Once());
		}

		[Fact]
		public void DeleteUserCallsBanRepoIfBanIsTrue()
		{
			var targetUser = new User { UserID = 1, Email = "a@b.com" };
			var user = new User { UserID = 2 };
			var service = GetMockedUserService();
			service.DeleteUser(targetUser, user, "127.0.0.1", true);
			_mockBanRepo.Verify(b => b.BanEmail(targetUser.Email), Times.Once());
		}

		[Fact]
		public void DeleteUserDoesNotCallBanRepoIfBanIsFalse()
		{
			var targetUser = new User { UserID = 1, Email = "a@b.com" };
			var user = new User { UserID = 2 };
			var service = GetMockedUserService();
			service.DeleteUser(targetUser, user, "127.0.0.1", false);
			_mockBanRepo.Verify(b => b.BanEmail(targetUser.Email), Times.Never());
		}

		[Fact]
		public void ForgotPasswordCallsMailerForGoodUser()
		{
			var user = new User { UserID = 2, Email = "a@b.com" };
			var service = GetMockedUserService();
			_mockUserRepo.Setup(u => u.GetUserByEmail(user.Email)).Returns(user);
			service.GeneratePasswordResetEmail(user, "http");
			_mockForgotMailer.Verify(f => f.ComposeAndQueue(user, It.IsAny<string>()), Times.Exactly(1));
		}

		[Fact]
		public void ForgotPasswordGeneratesNewAuthKey()
		{
			var user = new User { UserID = 2, Email = "a@b.com" };
			var service = GetMockedUserService();
			_mockUserRepo.Setup(u => u.GetUserByEmail(user.Email)).Returns(user);
			service.GeneratePasswordResetEmail(user, "http");
			_mockUserRepo.Verify(u => u.UpdateAuthorizationKey(user, It.IsAny<Guid>()), Times.Exactly(1));
		}

		[Fact]
		public void ForgotPasswordThrowsForNoUser()
		{
			var service = GetMockedUserService();
			_mockUserRepo.Setup(u => u.GetUserByEmail(It.IsAny<string>())).Returns((User)null);
			Assert.Throws<ArgumentNullException>(() => service.GeneratePasswordResetEmail(null, "http"));
			_mockForgotMailer.Verify(f => f.ComposeAndQueue(It.IsAny<User>(), It.IsAny<string>()), Times.Exactly(0));
		}
	}
}
