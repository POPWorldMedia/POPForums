using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using PopForums.Test.Models;
using PopForums.Web;

namespace PopForums.Test.Services
{
	[TestFixture]
	public class UserServiceTests
	{
		private Mock<IUserRepository> _mockUserRepo;
		private Mock<IRoleRepository> _mockRoleRepo;
		private Mock<IProfileRepository> _mockProfileRepo;
		private Mock<IFormsAuthenticationWrapper> _mockFormsAuth;
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
			_mockFormsAuth = new Mock<IFormsAuthenticationWrapper>();
			_mockSettingsManager = new Mock<ISettingsManager>();
			_mockUserAvatarRepo = new Mock<IUserAvatarRepository>();
			_mockUserImageRepo = new Mock<IUserImageRepository>();
			_mockSecurityLogService = new Mock<ISecurityLogService>();
			_mockTextParser = new Mock<ITextParsingService>();
			_mockBanRepo = new Mock<IBanRepository>();
			_mockForgotMailer = new Mock<IForgotPasswordMailer>();
			_mockImageService = new Mock<IImageService>();
			_mockRoleRepo.Setup(r => r.GetUserRoles(It.IsAny<int>())).Returns(new List<string>());
			return new UserService(_mockUserRepo.Object, _mockRoleRepo.Object, _mockProfileRepo.Object, _mockFormsAuth.Object, _mockSettingsManager.Object, _mockUserAvatarRepo.Object, _mockUserImageRepo.Object, _mockSecurityLogService.Object, _mockTextParser.Object, _mockBanRepo.Object, _mockForgotMailer.Object, _mockImageService.Object);
		}

		[Test]
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

		[Test]
		public void CheckPassword()
		{
			var userService = GetMockedUserService();
			Guid? salt;
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(String.Empty, out salt)).Returns("VwqQv7+MfqtdxdTiaDLVsQ==");

			Assert.IsTrue(userService.CheckPassword(String.Empty, "fred", out salt));
		}

		[Test]
		public void CheckPasswordFail()
		{
			var userService = GetMockedUserService();
			Guid? salt;
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(String.Empty, out salt)).Returns("VwqQv7+MfqtdxdTiaDLVsQ==");

			Assert.IsFalse(userService.CheckPassword(String.Empty, "fsdfsdfsdfsdf", out salt));
		}

		[Test]
		public void CheckPasswordHasSalt()
		{
			var userService = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			var hashedPassword = "fred".GetMD5Hash(salt.Value);
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(String.Empty, out salt)).Returns(hashedPassword);

			Assert.IsTrue(userService.CheckPassword(String.Empty, "fred", out salt));
		}

		[Test]
		public void CheckPasswordHasSaltFail()
		{
			var userService = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			var hashedPassword = "fred".GetMD5Hash(salt.Value);
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(String.Empty, out salt)).Returns(hashedPassword);

			Assert.IsFalse(userService.CheckPassword(String.Empty, "dsfsdfsdfsdf", out salt));
		}

		[Test]
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
			Assert.AreSame(dummyUser, user);
		}

		[Test]
		public void GetUserFail()
		{
			const int id = 1;
			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUser(It.Is<int>(i => i != 1))).Returns(GetDummyUser("", ""));
			_mockUserRepo.Setup(r => r.GetUser(It.Is<int>(i => i == 1))).Returns((User)null);
			var user = userService.GetUser(id);
			Assert.IsNull(user);
		}

		[Test]
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
			Assert.AreSame(dummyUser, user);
		}

		[Test]
		public void GetUserByNameFail()
		{
			const string name = "Jeff";
			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByName(It.Is<string>(i => i != name))).Returns(GetDummyUser(name, ""));
			_mockUserRepo.Setup(r => r.GetUserByName(It.Is<string>(i => i == name))).Returns((User)null);
			var user = userService.GetUserByName(name);
			Assert.IsNull(user);
		}

		[Test]
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
			Assert.AreSame(dummyUser, user);
		}

		[Test]
		public void GetUserByEmailFail()
		{
			const string email = "a@b.com";
			var userManager = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByEmail(It.Is<string>(i => i != email))).Returns(GetDummyUser("", email));
			_mockUserRepo.Setup(r => r.GetUserByEmail(It.Is<string>(i => i == email))).Returns((User)null);
			var user = userManager.GetUserByEmail(email);
			Assert.IsNull(user);
		}

		public static User GetDummyUser(string name, string email)
		{
			var almostNow = DateTime.UtcNow.AddDays(-1);
			return new User(1, almostNow) {Name = name, Email = email, IsApproved = true, LastActivityDate = almostNow, LastLoginDate = almostNow, AuthorizationKey = new Guid()};
		}

		[Test]
		public void NameIsInUse()
		{
			const string name = "jeff";
			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByName(It.IsRegex("^" + name + "$", RegexOptions.IgnoreCase))).Returns(GetDummyUser(name, "a@b.com"));
			Assert.IsTrue(userService.IsNameInUse(name));
			_mockUserRepo.Verify(r => r.GetUserByName(name), Times.Exactly(1));
			Assert.IsFalse(userService.IsNameInUse("notjeff"));
			Assert.IsTrue(userService.IsNameInUse(name.ToUpper()));
		}

		[Test]
		public void EmailIsInUse()
		{
			const string email = "a@b.com";
			var userManager = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByEmail(It.IsRegex("^" + email + "$", RegexOptions.IgnoreCase))).Returns(GetDummyUser("jeff", email));
			Assert.IsTrue(userManager.IsEmailInUse(email));
			_mockUserRepo.Verify(r => r.GetUserByEmail(email), Times.Exactly(1));
			Assert.IsFalse(userManager.IsEmailInUse("nota@b.com"));
			Assert.IsTrue(userManager.IsEmailInUse(email.ToUpper()));
		}

		[Test]
		public void EmailInUserByAnotherTrue()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("jeff", "a@b.com");
			_mockUserRepo.Setup(u => u.GetUserByEmail("c@d.com")).Returns(new User(123, DateTime.MinValue));
			var result = userService.IsEmailInUseByDifferentUser(user, "c@d.com");
			_mockUserRepo.Verify(u => u.GetUserByEmail("c@d.com"), Times.Once());
			Assert.True(result);
		}

		[Test]
		public void EmailInUserByAnotherFalseBecauseSameUser()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("jeff", "a@b.com");
			_mockUserRepo.Setup(u => u.GetUserByEmail("a@b.com")).Returns(user);
			var result = userService.IsEmailInUseByDifferentUser(user, "a@b.com");
			_mockUserRepo.Verify(u => u.GetUserByEmail("a@b.com"), Times.Once());
			Assert.False(result);
		}

		[Test]
		public void EmailInUserByAnotherFalseBecauseNoUser()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("jeff", "a@b.com");
			_mockUserRepo.Setup(u => u.GetUserByEmail("c@d.com")).Returns((User)null);
			var result = userService.IsEmailInUseByDifferentUser(user, "c@d.com");
			_mockUserRepo.Verify(u => u.GetUserByEmail("c@d.com"), Times.Once());
			Assert.False(result);
		}

		[Test]
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
			Assert.AreEqual(dummyUser.Name, user.Name);
			Assert.AreEqual(dummyUser.Email, user.Email);
			_mockTextParser.Verify(t => t.Censor(name), Times.Once());
			_mockUserRepo.Verify(r => r.CreateUser(nameCensor, email, It.IsAny<DateTime>(), true, It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.UserCreated));
		}


		[Test]
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

		[Test]
		public void CreateInvalidEmail()
		{
			var userService = GetMockedUserService();
			_mockTextParser.Setup(t => t.Censor(It.IsAny<string>())).Returns("blah");
			Assert.Throws(typeof(Exception), () => userService.CreateUser("", "a b@oihfwe", "", true, ""));
			Assert.DoesNotThrow(() => userService.CreateUser("name", "any@mail.com", "", true, "123"));
		}

		[Test]
		public void CreateUsedName()
		{
			const string usedName = "jeff";
			const string email = "a@b.com";
			var userService = GetMockedUserService();
			_mockTextParser.Setup(t => t.Censor("jeff")).Returns("jeff");
			_mockTextParser.Setup(t => t.Censor("anynamejeff")).Returns("anynamejeff");
			_mockUserRepo.Setup(r => r.GetUserByName(It.IsRegex("^" + usedName + "$", RegexOptions.IgnoreCase))).Returns(GetDummyUser(usedName, email));
			Assert.Throws(typeof(Exception), () => userService.CreateUser(usedName, email, "", true, ""));
			Assert.Throws(typeof(Exception), () => userService.CreateUser(usedName.ToUpper(), email, "", true, ""));
			Assert.DoesNotThrow(() => userService.CreateUser("anynamejeff", email, "", true, ""));
		}

		[Test]
		public void CreateNameNull()
		{
			var userManager = GetMockedUserService();
			Assert.Throws(typeof(Exception), () => userManager.CreateUser(null, "a@b.com", "", true, ""));
		}

		[Test]
		public void CreateNameEmpty()
		{
			var userManager = GetMockedUserService();
			Assert.Throws(typeof(Exception), () => userManager.CreateUser(String.Empty, "a@b.com", "", true, ""));
		}

		[Test]
		public void CreateUsedEmail()
		{
			const string usedEmail = "a@b.com";
			var userService = GetMockedUserService();
			_mockTextParser.Setup(t => t.Censor(It.IsAny<string>())).Returns("blah");
			_mockUserRepo.Setup(r => r.GetUserByEmail(It.IsRegex("^" + usedEmail + "$", RegexOptions.IgnoreCase))).Returns(GetDummyUser("jeff", usedEmail));
			Assert.Throws(typeof(Exception), () => userService.CreateUser("", usedEmail, "", true, ""), "The e-mail \"" + usedEmail + "\" is already in use.");
			Assert.DoesNotThrow(() => userService.CreateUser("name", "any@mail.com", "", true, ""));
		}

		[Test]
		public void CreateEmailBanned()
		{
			const string bannedEmail = "a@b.com";
			var userService = GetMockedUserService();
			_mockTextParser.Setup(t => t.Censor(It.IsAny<string>())).Returns("blah");
			_mockBanRepo.Setup(b => b.EmailIsBanned(bannedEmail)).Returns(true);
			Assert.Throws(typeof(Exception), () => userService.CreateUser("name", bannedEmail, "", true, ""), "The e-mail \"" + bannedEmail + "\" is banned.");
			Assert.DoesNotThrow(() => userService.CreateUser("name", "any@mail.com", "", true, ""));
		}

		[Test]
		public void CreateIPBanned()
		{
			const string bannedIP = "1.2.3.4";
			var userManager = GetMockedUserService();
			_mockTextParser.Setup(t => t.Censor(It.IsAny<string>())).Returns("blah");
			_mockBanRepo.Setup(b => b.IPIsBanned(bannedIP)).Returns(true);
			Assert.Throws(typeof(Exception), () => userManager.CreateUser("", "a@b.com", "", true, bannedIP), "The IP " + bannedIP + " is banned.");
			Assert.DoesNotThrow(() => userManager.CreateUser("name", "any@mail.com", "", true, ""));
		}

		[Test]
		public void UpdateLastActivityDate()
		{
			var userManager = GetMockedUserService();
			var user = UserTest.GetTestUser();
			var oldActivityDate = user.LastActivityDate;
			userManager.UpdateLastActicityDate(user);
			_mockUserRepo.Verify(r => r.UpdateLastActivityDate(user, It.IsAny<DateTime>()), Times.Once());
			Assert.AreNotEqual(oldActivityDate, user.LastActivityDate);
		}

		[Test]
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
			var user = new User(34243, DateTime.MinValue);
			userManager.ChangeEmail(targetUser, newEmail, user, "123");
			_mockUserRepo.Verify(r => r.ChangeEmail(targetUser, newEmail), Times.Exactly(1));
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, "123", "Old: a@b.com, New: c@d.com", SecurityLogType.EmailChange));
		}

		[Test]
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
			Assert.Throws(typeof(Exception), () => userService.ChangeEmail(user, newEmail, It.IsAny<User>(), It.IsAny<string>()), "The e-mail \"" + newEmail + "\" is already in use.");
			_mockUserRepo.Verify(r => r.ChangeEmail(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void ChangeEmailBad()
		{
			const string badEmail = "a b @ c";
			var userManager = GetMockedUserService();
			_mockSettingsManager.Setup(x => x.Current.IsNewUserApproved).Returns(true);
			var user = GetDummyUser("", "");
			Assert.Throws(typeof(Exception), () => userManager.ChangeEmail(user, badEmail, It.IsAny<User>(), It.IsAny<string>()), "E-mail address invalid.");
			_mockUserRepo.Verify(r => r.ChangeEmail(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Test]
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
			var user = new User(34243, DateTime.MinValue);
			userManager.ChangeEmail(targetUser, newEmail, user, "123");
			_mockUserRepo.Verify(x => x.UpdateIsApproved(targetUser, true), Times.Once());
		}

		[Test]
		public void ChangeNameSuccess()
		{
			const string oldName = "Jeff";
			const string oldEmail = "a@b.com";
			const string newName = "Diana";

			var userManager = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByName(oldName)).Returns(GetDummyUser(oldName, oldEmail));
			_mockUserRepo.Setup(r => r.GetUserByName(newName)).Returns((User)null);
			var targetUser = GetDummyUser(oldName, oldEmail);
			var user = new User(1234531, DateTime.MinValue);
			userManager.ChangeName(targetUser, newName, user, "123");
			_mockUserRepo.Verify(r => r.ChangeName(targetUser, newName));
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, "123", "Old: Jeff, New: Diana", SecurityLogType.NameChange));
		}

		[Test]
		public void ChangeNameFailUsed()
		{
			const string oldName = "Jeff";
			const string oldEmail = "a@b.com";
			const string newName = "Diana";

			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByName(oldName)).Returns(GetDummyUser(oldName, oldEmail));
			_mockUserRepo.Setup(r => r.GetUserByName(newName)).Returns(GetDummyUser(newName, oldEmail));
			var user = GetDummyUser(oldName, oldEmail);
			Assert.Throws(typeof(Exception), () => userService.ChangeName(user, newName, It.IsAny<User>(), It.IsAny<string>()));
			_mockUserRepo.Verify(r => r.ChangeName(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void ChangeNameNull()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("Jeff", "a@b.com");
			Assert.Throws(typeof(Exception), () => userService.ChangeName(user, null, It.IsAny<User>(), It.IsAny<string>()));
			_mockUserRepo.Verify(r => r.ChangeName(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void ChangeNameEmpty()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("Jeff", "a@b.com");
			Assert.Throws(typeof(Exception), () => userService.ChangeName(user, String.Empty, It.IsAny<User>(), It.IsAny<string>()));
			_mockUserRepo.Verify(r => r.ChangeName(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void SetupUserViewDataNoUser()
		{
			var userService = GetMockedUserService();
			var viewData = new ViewDataDictionary();
			userService.SetupUserViewData(null, viewData);
			Assert.AreEqual(0, viewData.Count);
		}

		[Test]
		public void SetupUserViewDataUserPresent()
		{
			var user = UserTest.GetTestUser();
			var userService = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByName(It.IsAny<string>())).Returns(user);
			var viewData = new ViewDataDictionary();
			var principalMock = new Mock<IPrincipal>();
			var identity = new Mock<IIdentity>().Object;
			principalMock.Setup(p => p.Identity).Returns(identity);
			userService.SetupUserViewData(principalMock.Object, viewData);
			Assert.AreEqual(1, viewData.Count);
			Assert.AreEqual(user, viewData[ViewDataDictionaries.ViewDataUserKey]);
		}

		[Test]
		public void Logout()
		{
			var userService = GetMockedUserService();
			var user = UserTest.GetTestUser();
			userService.Logout(user, "123");
			_mockFormsAuth.Verify(f => f.SignOut(), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, "123", String.Empty, SecurityLogType.Logout));
		}

		[Test]
		public void LoginSuccess()
		{
			const string email = "a@b.com";
			const string password = "fred";
			var user = UserTest.GetTestUser();
			var userService = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			var saltedHash = password.GetMD5Hash(salt.Value);
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(email, out salt)).Returns(saltedHash);
			_mockUserRepo.Setup(r => r.GetUserByEmail(email)).Returns(user);
			_mockUserRepo.Setup(r => r.UpdateLastLoginDate(user, It.IsAny<DateTime>()));
			var contextMock = new Mock<HttpContextBase>();
			contextMock.Setup(c => c.Request.UserHostAddress).Returns("123");

			var result = userService.Login(email, password, true, contextMock.Object);

			Assert.IsTrue(result);
			_mockFormsAuth.Verify(f => f.SetAuthCookie(contextMock.Object, user, true), Times.Once());
			_mockUserRepo.Verify(r => r.UpdateLastLoginDate(user, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, "123", String.Empty, SecurityLogType.Login), Times.Once());
			_mockUserRepo.Verify(x => x.SetHashedPassword(user, It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
		}

		[Test]
		public void LoginSuccessNoSalt()
		{
			const string email = "a@b.com";
			const string password = "fred";
			var user = UserTest.GetTestUser();
			var userService = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			Guid? nosalt;
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(email, out nosalt)).Returns(password.GetMD5Hash());
			_mockUserRepo.Setup(r => r.GetUserByEmail(email)).Returns(user);
			_mockUserRepo.Setup(r => r.UpdateLastLoginDate(user, It.IsAny<DateTime>()));
			var contextMock = new Mock<HttpContextBase>();
			contextMock.Setup(c => c.Request.UserHostAddress).Returns("123");
			_mockUserRepo.Setup(x => x.SetHashedPassword(user, It.IsAny<string>(), It.IsAny<Guid>())).Callback<User, string, Guid>((u, p, s) => salt = s);

			var result = userService.Login(email, password, true, contextMock.Object);

			Assert.IsTrue(result);
			_mockFormsAuth.Verify(f => f.SetAuthCookie(contextMock.Object, user, true), Times.Once());
			_mockUserRepo.Verify(r => r.UpdateLastLoginDate(user, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, "123", String.Empty, SecurityLogType.Login), Times.Once());
			var saltyPassword = password.GetMD5Hash(salt.Value);
			_mockUserRepo.Verify(x => x.SetHashedPassword(user, saltyPassword, salt.Value), Times.Once());
		}

		[Test]
		public void LoginFail()
		{
			const string email = "a@b.com";
			const string password = "fred";
			var userService = GetMockedUserService();
			Guid? salt;
			_mockUserRepo.Setup(r => r.GetHashedPasswordByEmail(It.IsAny<string>(), out salt)).Returns("1234");
			var contextMock = new Mock<HttpContextBase>();
			contextMock.Setup(c => c.Request.UserHostAddress).Returns("123");
			var result = userService.Login(email, password, true, contextMock.Object);
			Assert.IsFalse(result);
			_mockFormsAuth.Verify(f => f.SetAuthCookie(It.IsAny<HttpContextBase>(), It.IsAny<User>(), It.IsAny<bool>()), Times.Never());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry((User)null, null, "123", "E-mail attempted: " + email, SecurityLogType.FailedLogin), Times.Once());
			_mockUserRepo.Verify(x => x.SetHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
		}

		[Test]
		public void LoginWithUser()
		{
			var user = UserTest.GetTestUser();
			var service = GetMockedUserService();
			var contextMock = new Mock<HttpContextBase>();
			contextMock.Setup(c => c.Request.UserHostAddress).Returns("123");
			service.Login(user, contextMock.Object);
			_mockFormsAuth.Verify(f => f.SetAuthCookie(contextMock.Object, user, false), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastLoginDate(user, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, "123", String.Empty, SecurityLogType.Login), Times.Once());
		}

		[Test]
		public void LoginWithUserPersistCookie()
		{
			var user = UserTest.GetTestUser();
			var service = GetMockedUserService();
			var contextMock = new Mock<HttpContextBase>();
			contextMock.Setup(c => c.Request.UserHostAddress).Returns("123");
			service.Login(user, true, contextMock.Object);
			_mockFormsAuth.Verify(f => f.SetAuthCookie(contextMock.Object, user, true), Times.Once());
			_mockUserRepo.Verify(u => u.UpdateLastLoginDate(user, It.IsAny<DateTime>()), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(null, user, "123", String.Empty, SecurityLogType.Login), Times.Once());
		}

		[Test]
		public void GetAllRoles()
		{
			var userService = GetMockedUserService();
			var list = new List<string>();
			_mockRoleRepo.Setup(r => r.GetAllRoles()).Returns(list);
			var result = userService.GetAllRoles();
			_mockRoleRepo.Verify(r => r.GetAllRoles(), Times.Once());
			Assert.AreSame(list, result);
		}

		[Test]
		public void CreateRole()
		{
			var userService = GetMockedUserService();
			const string role = "blah";
			userService.CreateRole(role, It.IsAny<User>(), It.IsAny<string>());
			_mockRoleRepo.Verify(r => r.CreateRole(role), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), null, It.IsAny<string>(), "Role: blah", SecurityLogType.RoleCreated), Times.Once());
		}

		[Test]
		public void DeleteRole()
		{
			var userService = GetMockedUserService();
			const string role = "blah";
			userService.DeleteRole(role, It.IsAny<User>(), It.IsAny<string>());
			_mockRoleRepo.Verify(r => r.DeleteRole(role), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), null, It.IsAny<string>(), "Role: blah", SecurityLogType.RoleDeleted), Times.Once());
		}

		[Test]
		public void DeleteRoleThrowsOnAdminOrMod()
		{
			var userService = GetMockedUserService();
			Assert.Throws<InvalidOperationException>(() => userService.DeleteRole("Admin", It.IsAny<User>(), It.IsAny<string>()));
			Assert.Throws<InvalidOperationException>(() => userService.DeleteRole("Moderator", It.IsAny<User>(), It.IsAny<string>()));
			Assert.Throws<InvalidOperationException>(() => userService.DeleteRole("admin", It.IsAny<User>(), It.IsAny<string>()));
			Assert.Throws<InvalidOperationException>(() => userService.DeleteRole("moderator", It.IsAny<User>(), It.IsAny<string>()));
			_mockRoleRepo.Verify(r => r.DeleteRole(It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void UpdateIsApproved()
		{
			var userService = GetMockedUserService();
			var targetUser = GetDummyUser("Jeff", "a@b.com");
			var user = new User(97, DateTime.MinValue);
			userService.UpdateIsApproved(targetUser, true, user, "123");
			_mockUserRepo.Verify(u => u.UpdateIsApproved(targetUser, true), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, It.IsAny<string>(), String.Empty, SecurityLogType.IsApproved));
		}

		[Test]
		public void UpdateIsApprovedFalse()
		{
			var userService = GetMockedUserService();
			var targetUser = GetDummyUser("Jeff", "a@b.com");
			var user = new User(97, DateTime.MinValue);
			userService.UpdateIsApproved(targetUser, false, user, "123");
			_mockUserRepo.Verify(u => u.UpdateIsApproved(targetUser, false), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, It.IsAny<string>(), String.Empty, SecurityLogType.IsNotApproved));
		}

		[Test]
		public void UpdateAuthKey()
		{
			var userService = GetMockedUserService();
			var user = GetDummyUser("Jeff", "a@b.com");
			var key = Guid.NewGuid();
			userService.UpdateAuthorizationKey(user, key);
			_mockUserRepo.Verify(u => u.UpdateAuthorizationKey(user, key), Times.Once());
		}

		[Test]
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
			Assert.AreSame(dummyUser, user);
			_mockUserRepo.Verify(u => u.UpdateIsApproved(dummyUser, true), Times.Once());
		}

		[Test]
		public void VerifyUserByAuthKeyFail()
		{
			var service = GetMockedUserService();
			_mockUserRepo.Setup(r => r.GetUserByAuthorizationKey(It.IsAny<Guid>())).Returns((User)null);
			var user = service.VerifyAuthorizationCode(Guid.NewGuid(), "123");
			Assert.IsNull(user);
			_mockUserRepo.Verify(u => u.UpdateIsApproved(It.IsAny<User>(), true), Times.Never());
		}

		[Test]
		public void SearchByEmail()
		{
			var service = GetMockedUserService();
			var list = new List<User>();
			_mockUserRepo.Setup(u => u.SearchByEmail("blah")).Returns(list);
			var result = service.SearchByEmail("blah");
			Assert.AreSame(list, result);
			_mockUserRepo.Verify(u => u.SearchByEmail("blah"), Times.Once());
		}

		[Test]
		public void SearchByName()
		{
			var service = GetMockedUserService();
			var list = new List<User>();
			_mockUserRepo.Setup(u => u.SearchByName("blah")).Returns(list);
			var result = service.SearchByName("blah");
			Assert.AreSame(list, result);
			_mockUserRepo.Verify(u => u.SearchByName("blah"), Times.Once());
		}

		[Test]
		public void SearchByRole()
		{
			var service = GetMockedUserService();
			var list = new List<User>();
			_mockUserRepo.Setup(u => u.SearchByRole("blah")).Returns(list);
			var result = service.SearchByRole("blah");
			Assert.AreSame(list, result);
			_mockUserRepo.Verify(u => u.SearchByRole("blah"), Times.Once());
		}

		[Test]
		public void GetUserEdit()
		{
			var service = GetMockedUserService();
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(new Profile(1) {Web = "blah"});
			var user = new User(1, DateTime.MinValue);
			user.Roles = new List<string>();
			var result = service.GetUserEdit(user);
			Assert.AreEqual(1, result.UserID);
			Assert.AreEqual("blah", result.Web);
		}

		[Test]
		public void EditUserProfileOnly()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
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
			return new Profile(1) {
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
			                      	Aim = userEdit.Aim,
			                      	Icq = userEdit.Icq,
			                      	YahooMessenger = userEdit.YahooMessenger,
									Facebook = userEdit.Facebook,
									Twitter = userEdit.Twitter
			                      };
		}

		[Test]
		public void EditUserApprovalChange()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue) {IsApproved = false};
			user.Roles = new List<string>();
			var userEdit = new UserEdit {IsApproved = true};
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(GetReturnedProfile(userEdit));
			service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockUserRepo.Verify(u => u.UpdateIsApproved(user, true), Times.Once());
		}

		[Test]
		public void EditUserNewEmail()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue) { Email = "c@d.com" };
			user.Roles = new List<string>();
			var userEdit = new UserEdit { NewEmail = "a@b.com" };
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(GetReturnedProfile(userEdit));
			service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockUserRepo.Verify(u => u.ChangeEmail(user, "a@b.com"), Times.Once());
		}

		[Test]
		public void EditUserNewPassword()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			user.Roles = new List<string>();
			var userEdit = new UserEdit { NewPassword = "foo" };
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(GetReturnedProfile(userEdit));
			service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockUserRepo.Verify(u => u.SetHashedPassword(user, It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
		}

		[Test]
		public void EditUserAddRole()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			user.Roles = new List<string>();
			var userEdit = new UserEdit { Roles = new [] {"Admin", "Moderator"} };
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(GetReturnedProfile(userEdit));
			service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockRoleRepo.Verify(r => r.ReplaceUserRoles(1, userEdit.Roles), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), user, "123", "Admin", SecurityLogType.UserAddedToRole), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), user, "123", "Moderator", SecurityLogType.UserAddedToRole), Times.Once());
		}

		[Test]
		public void EditUserRemoveRole()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			user.Roles = new List<string> { "Admin", "Moderator" };
			var userEdit = new UserEdit { Roles = new[] { "SomethingElse" } };
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(GetReturnedProfile(userEdit));
			service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockRoleRepo.Verify(r => r.ReplaceUserRoles(1, userEdit.Roles), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), user, "123", "Admin", SecurityLogType.UserRemovedFromRole), Times.Once());
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(It.IsAny<User>(), user, "123", "Moderator", SecurityLogType.UserRemovedFromRole), Times.Once());
		}

		[Test]
		public void EditUserDeleteAvatar()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			user.Roles = new List<string>();
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.AvatarID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUser(user, userEdit, true, false, null, null, "123", user);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			Assert.IsNull(profile.AvatarID);
		}

		[Test]
		public void EditUserNoDeleteAvatar()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			user.Roles = new List<string>();
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.AvatarID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			Assert.AreEqual(3, profile.AvatarID);
		}

		[Test]
		public void EditUserDeletePhoto()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			user.Roles = new List<string>();
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.ImageID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUser(user, userEdit, false, true, null, null, "123", user);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			Assert.IsNull(profile.ImageID);
		}

		[Test]
		public void EditUserNoDeletePhoto()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			user.Roles = new List<string>();
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.ImageID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUser(user, userEdit, false, false, null, null, "123", user);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			Assert.AreEqual(3, profile.ImageID);
		}

		[Test]
		public void EditUserNewAvatar()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			user.Roles = new List<string>();
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Returns(true);
			var avatar = new Mock<HttpPostedFileBase>();
			avatar.Setup(a => a.ContentLength).Returns(100);
			var stream = new MemoryStream(new byte[]{1,2,1});
			avatar.Setup(a => a.InputStream).Returns(stream);
			_mockUserAvatarRepo.Setup(a => a.SaveNewAvatar(1, It.IsAny<byte[]>(), It.IsAny<DateTime>())).Returns(12);
			service.EditUser(user, userEdit, false, false, avatar.Object, null, "123", user);
			_mockUserAvatarRepo.Verify(a => a.SaveNewAvatar(1, It.IsAny<byte[]>(), It.IsAny<DateTime>()), Times.Once());
		}

		[Test]
		public void EditUserNewPhoto()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			user.Roles = new List<string>();
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Returns(true);
			var photo = new Mock<HttpPostedFileBase>();
			photo.Setup(a => a.ContentLength).Returns(100);
			var stream = new MemoryStream(new byte[] { 1, 2, 1 });
			photo.Setup(a => a.InputStream).Returns(stream);
			_mockUserImageRepo.Setup(a => a.SaveNewImage(1, 0, true, It.IsAny<byte[]>(), It.IsAny<DateTime>())).Returns(12);
			service.EditUser(user, userEdit, false, false, null, photo.Object, "123", user);
			_mockUserImageRepo.Verify(a => a.SaveNewImage(1, 0, true, It.IsAny<byte[]>(), It.IsAny<DateTime>()), Times.Once());
		}

		[Test]
		public void EditUserProfile()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			user.Roles = new List<string>();
			var returnedProfile = new Profile(1);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			_mockTextParser.Setup(t => t.ForumCodeToHtml(It.IsAny<string>())).Returns("parsed");
			var userEdit = new UserEditProfile
			               	{
			               		Aim = "a", Dob = new DateTime(2000,1,1), HideVanity = true, Icq = "i", IsDaylightSaving = true, IsPlainText = true, IsSubscribed = true, Location = "l", Facebook = "fb", Twitter = "tw", ShowDetails = true, Signature = "s", TimeZone = -7, Web = "w", YahooMessenger = "y"
			               	};
			service.EditUserProfile(user, userEdit);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			Assert.AreEqual("a", profile.Aim);
			Assert.AreEqual(new DateTime(2000, 1, 1), profile.Dob);
			Assert.IsTrue(profile.HideVanity);
			Assert.AreEqual("i", profile.Icq);
			Assert.IsTrue(profile.IsDaylightSaving);
			Assert.IsTrue(profile.IsPlainText);
			Assert.IsTrue(profile.IsSubscribed);
			Assert.AreEqual("l", profile.Location);
			Assert.AreEqual("fb", profile.Facebook);
			Assert.AreEqual("tw", profile.Twitter);
			Assert.IsTrue(profile.ShowDetails);
			Assert.AreEqual("parsed", profile.Signature);
			Assert.AreEqual(-7, profile.TimeZone);
			Assert.AreEqual("w", profile.Web);
			Assert.AreEqual("y", profile.YahooMessenger);
		}

		[Test]
		public void VerifyPasswordSuccess()
		{
			const string password = "blah";
			var hashed = password.GetMD5Hash();
			var service = GetMockedUserService();
			Guid? salt;
			_mockUserRepo.Setup(u => u.GetHashedPasswordByEmail("a@b.com", out salt)).Returns(hashed);

			var result = service.VerifyPassword(new User(1, DateTime.MinValue) {Email = "a@b.com"}, password);

			_mockUserRepo.Verify(u => u.GetHashedPasswordByEmail("a@b.com", out salt), Times.Once());
			Assert.True(result);
		}

		[Test]
		public void VerifyPasswordWithSaltSuccess()
		{
			const string password = "blah";
			var service = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			var hashed = password.GetMD5Hash(salt.Value);
			_mockUserRepo.Setup(u => u.GetHashedPasswordByEmail("a@b.com", out salt)).Returns(hashed);

			var result = service.VerifyPassword(new User(1, DateTime.MinValue) { Email = "a@b.com" }, password);

			_mockUserRepo.Verify(u => u.GetHashedPasswordByEmail("a@b.com", out salt), Times.Once());
			Assert.True(result);
		}

		[Test]
		public void VerifyPasswordFail()
		{
			const string password = "blah";
			var service = GetMockedUserService();
			Guid? salt;
			_mockUserRepo.Setup(u => u.GetHashedPasswordByEmail("a@b.com", out salt)).Returns("2233435");

			var result = service.VerifyPassword(new User(1, DateTime.MinValue) { Email = "a@b.com" }, password);

			_mockUserRepo.Verify(u => u.GetHashedPasswordByEmail("a@b.com", out salt), Times.Once());
			Assert.False(result);
		}

		[Test]
		public void VerifyPasswordWithSaltFail()
		{
			const string password = "blah";
			var service = GetMockedUserService();
			Guid? salt = Guid.NewGuid();
			_mockUserRepo.Setup(u => u.GetHashedPasswordByEmail("a@b.com", out salt)).Returns("2233435");

			var result = service.VerifyPassword(new User(1, DateTime.MinValue) { Email = "a@b.com" }, password);

			_mockUserRepo.Verify(u => u.GetHashedPasswordByEmail("a@b.com", out salt), Times.Once());
			Assert.False(result);
		}

		[Test]
		public void IsPasswordValidTrue()
		{
			var service = GetMockedUserService();
			var modelState = new ModelStateDictionary();
			var result = service.IsPasswordValid("123456", modelState);
			Assert.True(result);
			Assert.AreEqual(0, modelState.Count);
		}

		[Test]
		public void IsPasswordValidFalse()
		{
			var service = GetMockedUserService();
			var modelState = new ModelStateDictionary();
			var result = service.IsPasswordValid("12345", modelState);
			Assert.False(result);
			Assert.AreEqual(1, modelState.Count);
			Assert.True(modelState.ContainsKey("Password"));
		}

		[Test]
		public void UserEditPhotosDeleteAvatar()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.AvatarID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUserProfileImages(user, true, false, null, null);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			_mockUserAvatarRepo.Verify(u => u.DeleteAvatarsByUserID(user.UserID));
			Assert.IsNull(profile.AvatarID);
		}

		[Test]
		public void UserEditPhotosNoDeleteAvatar()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.AvatarID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUserProfileImages(user, false, false, null, null);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			_mockUserAvatarRepo.Verify(u => u.DeleteAvatarsByUserID(user.UserID), Times.Never());
			Assert.AreEqual(3, profile.AvatarID);
		}

		[Test]
		public void UserEditPhotosDeletePhoto()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.ImageID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUserProfileImages(user, false, true, null, null);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			_mockUserImageRepo.Verify(u => u.DeleteImagesByUserID(user.UserID));
			Assert.IsNull(profile.ImageID);
		}

		[Test]
		public void UserEditPhotosNoDeletePhoto()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			returnedProfile.ImageID = 3;
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			var profile = new Profile();
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Callback<Profile>(p => profile = p);
			service.EditUserProfileImages(user, false, false, null, null);
			_mockProfileRepo.Verify(p => p.Update(It.IsAny<Profile>()), Times.Once());
			_mockUserImageRepo.Verify(u => u.DeleteImagesByUserID(user.UserID), Times.Never());
			Assert.AreEqual(3, profile.ImageID);
		}

		[Test]
		public void UserEditPhotosNewAvatar()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Returns(true);
			var avatar = new Mock<HttpPostedFileBase>();
			avatar.Setup(a => a.ContentLength).Returns(100);
			var stream = new MemoryStream(new byte[] { 1, 2, 1 });
			avatar.Setup(a => a.InputStream).Returns(stream);
			_mockUserAvatarRepo.Setup(a => a.SaveNewAvatar(1, It.IsAny<byte[]>(), It.IsAny<DateTime>())).Returns(12);
			_mockSettingsManager.Setup(s => s.Current.UserAvatarMaxHeight).Returns(1);
			_mockSettingsManager.Setup(s => s.Current.UserAvatarMaxWidth).Returns(1);
			service.EditUserProfileImages(user, false, false, avatar.Object, null);
			_mockUserAvatarRepo.Verify(a => a.SaveNewAvatar(1, It.IsAny<byte[]>(), It.IsAny<DateTime>()), Times.Once());
			_mockUserAvatarRepo.Verify(u => u.DeleteAvatarsByUserID(user.UserID));
		}

		[Test]
		public void UserEditPhotosNewPhoto()
		{
			var service = GetMockedUserService();
			var user = new User(1, DateTime.MinValue);
			var userEdit = new UserEdit();
			var returnedProfile = GetReturnedProfile(userEdit);
			_mockProfileRepo.Setup(p => p.GetProfile(1)).Returns(returnedProfile);
			_mockProfileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Returns(true);
			var photo = new Mock<HttpPostedFileBase>();
			photo.Setup(a => a.ContentLength).Returns(100);
			var stream = new MemoryStream(new byte[] { 1, 2, 1 });
			photo.Setup(a => a.InputStream).Returns(stream);
			_mockUserImageRepo.Setup(a => a.SaveNewImage(1, 0, true, It.IsAny<byte[]>(), It.IsAny<DateTime>())).Returns(12);
			_mockSettingsManager.Setup(s => s.Current.UserImageMaxHeight).Returns(1);
			_mockSettingsManager.Setup(s => s.Current.UserImageMaxWidth).Returns(1);
			_mockSettingsManager.Setup(s => s.Current.IsNewUserImageApproved).Returns(true);
			service.EditUserProfileImages(user, false, false, null, photo.Object);
			_mockUserImageRepo.Verify(a => a.SaveNewImage(1, 0, true, It.IsAny<byte[]>(), It.IsAny<DateTime>()), Times.Once());
			_mockUserImageRepo.Verify(u => u.DeleteImagesByUserID(user.UserID));
		}

		[Test]
		public void GetUsersOnlineCallsRepo()
		{
			var service = GetMockedUserService();
			var users = new List<User>();
			_mockUserRepo.Setup(u => u.GetUsersOnline()).Returns(users);
			var result = service.GetUsersOnline();
			_mockUserRepo.Verify(u => u.GetUsersOnline(), Times.Once());
			Assert.AreSame(users, result);
		}

		[Test]
		public void DeleteUserLogs()
		{
			var targetUser = new User(1, DateTime.MinValue);
			var user = new User(2, DateTime.MinValue);
			var service = GetMockedUserService();
			service.DeleteUser(targetUser, user, "127.0.0.1", true);
			_mockSecurityLogService.Verify(s => s.CreateLogEntry(user, targetUser, "127.0.0.1", It.IsAny<string>(), SecurityLogType.UserDeleted));
		}

		[Test]
		public void DeleteUserCallsRepo()
		{
			var targetUser = new User(1, DateTime.MinValue);
			var user = new User(2, DateTime.MinValue);
			var service = GetMockedUserService();
			service.DeleteUser(targetUser, user, "127.0.0.1", true);
			_mockUserRepo.Verify(u => u.DeleteUser(targetUser), Times.Once());
		}

		[Test]
		public void DeleteUserCallsBanRepoIfBanIsTrue()
		{
			var targetUser = new User(1, DateTime.MinValue) { Email = "a@b.com" };
			var user = new User(2, DateTime.MinValue);
			var service = GetMockedUserService();
			service.DeleteUser(targetUser, user, "127.0.0.1", true);
			_mockBanRepo.Verify(b => b.BanEmail(targetUser.Email), Times.Once());
		}

		[Test]
		public void DeleteUserDoesNotCallBanRepoIfBanIsFalse()
		{
			var targetUser = new User(1, DateTime.MinValue) { Email = "a@b.com" };
			var user = new User(2, DateTime.MinValue);
			var service = GetMockedUserService();
			service.DeleteUser(targetUser, user, "127.0.0.1", false);
			_mockBanRepo.Verify(b => b.BanEmail(targetUser.Email), Times.Never());
		}

		[Test]
		public void ForgotPasswordCallsMailerForGoodUser()
		{
			var user = new User(2, DateTime.MinValue) { Email = "a@b.com" };
			var service = GetMockedUserService();
			_mockUserRepo.Setup(u => u.GetUserByEmail(user.Email)).Returns(user);
			service.GeneratePasswordResetEmail(user, "http");
			_mockForgotMailer.Verify(f => f.ComposeAndQueue(user, It.IsAny<string>()), Times.Exactly(1));
		}

		[Test]
		public void ForgotPasswordGeneratesNewAuthKey()
		{
			var user = new User(2, DateTime.MinValue) { Email = "a@b.com" };
			var service = GetMockedUserService();
			_mockUserRepo.Setup(u => u.GetUserByEmail(user.Email)).Returns(user);
			service.GeneratePasswordResetEmail(user, "http");
			_mockUserRepo.Verify(u => u.UpdateAuthorizationKey(user, It.IsAny<Guid>()), Times.Exactly(1));
		}

		[Test]
		public void ForgotPasswordThrowsForNoUser()
		{
			var service = GetMockedUserService();
			_mockUserRepo.Setup(u => u.GetUserByEmail(It.IsAny<string>())).Returns((User)null);
			Assert.Throws<ArgumentNullException>(() => service.GeneratePasswordResetEmail(null, "http"));
			_mockForgotMailer.Verify(f => f.ComposeAndQueue(It.IsAny<User>(), It.IsAny<string>()), Times.Exactly(0));
		}
	}
}
