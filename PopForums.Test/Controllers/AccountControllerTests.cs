using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Controllers;
using PopForums.Email;
using PopForums.ExternalLogin;
using PopForums.Feeds;
using PopForums.Models;
using PopForums.ScoringGame;
using PopForums.Services;
using PopForums.Test.Models;
using PopForums.Test.Services;
using FormCollection = System.Web.Mvc.FormCollection;

namespace PopForums.Test.Controllers
{
	[TestFixture]
	public class AccountControllerTests
	{
		private Mock<IUserService> _userService;
		private Mock<IProfileService> _profileService;
		private Mock<ISettingsManager> _settingsManager;
		private Mock<INewAccountMailer> _newAccountMailer;
		private Mock<IPostService> _postService;
		private Mock<ITopicService> _topicService;
		private Mock<IForumService> _forumService;
		private Mock<ILastReadService> _lastReadService;
		private Mock<IClientSettingsMapper> _clientSettingsMapper;
		private Mock<IUserEmailer> _userEmailer;
		private Mock<IImageService> _imageService;
		private Mock<IFeedService> _feedService;
		private Mock<IUserAwardService> _userAwardService;
		private Mock<IOwinContext> _owinContext;
		private Mock<IExternalAuthentication> _externalAuth;
		private Mock<IUserAssociationManager> _userAssociationManager;

		private TestableAccountController GetController()
		{
			_userService = new Mock<IUserService>();
			_profileService = new Mock<IProfileService>();
			_settingsManager = new Mock<ISettingsManager>();
			_newAccountMailer = new Mock<INewAccountMailer>();
			_postService = new Mock<IPostService>();
			_topicService = new Mock<ITopicService>();
			_forumService = new Mock<IForumService>();
			_lastReadService = new Mock<ILastReadService>();
			_clientSettingsMapper = new Mock<IClientSettingsMapper>();
			_userEmailer = new Mock<IUserEmailer>();
			_imageService = new Mock<IImageService>();
			_feedService = new Mock<IFeedService>();
			_userAwardService = new Mock<IUserAwardService>();
			_owinContext = new Mock<IOwinContext>();
			_externalAuth = new Mock<IExternalAuthentication>();
			_userAssociationManager = new Mock<IUserAssociationManager>();
			return new TestableAccountController(_userService.Object, _profileService.Object, _newAccountMailer.Object, _settingsManager.Object, _postService.Object, _topicService.Object, _forumService.Object, _lastReadService.Object, _clientSettingsMapper.Object, _userEmailer.Object, _imageService.Object, _feedService.Object, _userAwardService.Object, _owinContext.Object, _externalAuth.Object, _userAssociationManager.Object);
		}

		private class TestableAccountController : AccountController
		{
			public TestableAccountController(IUserService userService, IProfileService profileService, INewAccountMailer newAccountMailer, ISettingsManager settingsManager, IPostService postService, ITopicService topicService, IForumService forumService, ILastReadService lastReadService, IClientSettingsMapper clientSettingsManager, IUserEmailer userEmailer, IImageService imageService, IFeedService feedService, IUserAwardService userAwardService, IOwinContext owinContext, IExternalAuthentication externalAuthentication, IUserAssociationManager userAssociationManager) : base(userService, profileService, newAccountMailer, settingsManager, postService, topicService, forumService, lastReadService, clientSettingsManager, userEmailer, imageService, feedService, userAwardService, owinContext, externalAuthentication, userAssociationManager) { }

			public void SetUser(User user)
			{
				HttpContext.User = user;
			}
		}

		[Test]
		public void CreateView()
		{
			var controller = GetController();
			var settings = new Settings();
			_settingsManager.Setup(s => s.Current).Returns(settings);
			var result = controller.Create();
			Assert.IsInstanceOf<ViewResult>(result);
		}

		[Test]
		public void CreateViewWithData()
		{
			var controller = GetController();
			const string tos = "blah blah blah";
			var settings = new Settings {TermsOfService = tos};
			_settingsManager.Setup(s => s.Current).Returns(settings);
			controller.Create();
			Assert.IsInstanceOf<String>(controller.ViewData[AccountController.CoppaDateKey]);
			Assert.IsInstanceOf<String>(controller.ViewData[AccountController.TosKey]);
			Assert.AreEqual(tos, controller.ViewData[AccountController.TosKey]);
		}

		[Test]
		public void CreateValid()
		{
			var controller = GetController();
			MockUpUrl(controller);
			_userService.Setup(u => u.IsEmailInUse(It.IsAny<string>())).Returns(false);
			_userService.Setup(u => u.IsNameInUse(It.IsAny<string>())).Returns(false);
			var user = UserServiceTests.GetDummyUser("Diana", "a@b.com");
			var signUp = new SignupData { Email = "a@b.com", IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = "Diana", Password = "passwerd", PasswordRetype = "passwerd", TimeZone = -5 };
			_userService.Setup(u => u.CreateUser(signUp, It.IsAny<string>())).Returns(user);
			_newAccountMailer.Setup(n => n.Send(It.IsAny<User>(), It.IsAny<string>())).Returns(System.Net.Mail.SmtpStatusCode.CommandNotImplemented);
			var settings = new Settings { IsNewUserApproved = true };
			_settingsManager.Setup(s => s.Current).Returns(settings);
			var authManager = new Mock<IAuthenticationManager>();
			_owinContext.Setup(x => x.Authentication).Returns(authManager.Object);
			var authResult = Task.FromResult<ExternalAuthenticationResult>(null);
			_externalAuth.Setup(x => x.GetAuthenticationResult(It.IsAny<IAuthenticationManager>())).Returns(authResult);

			var result = controller.Create(signUp).Result;

			Assert.IsTrue(controller.ModelState.IsValid);
			Assert.AreEqual("AccountCreated", result.ViewName);
			Assert.IsTrue(result.ViewData["EmailProblem"].ToString().Contains(System.Net.Mail.SmtpStatusCode.CommandNotImplemented.ToString()));
			_userService.Verify(u => u.CreateUser(signUp, It.IsAny<string>()), Times.Once());
			_profileService.Verify(p => p.Create(user, signUp), Times.Once());
			_newAccountMailer.Verify(n => n.Send(user, It.IsAny<string>()), Times.Once());
		}

		[Test]
		public void CreateValidCallExternalAuthAssociateWithAuthResult()
		{
			var controller = GetController();
			MockUpUrl(controller);
			_userService.Setup(u => u.IsEmailInUse(It.IsAny<string>())).Returns(false);
			_userService.Setup(u => u.IsNameInUse(It.IsAny<string>())).Returns(false);
			var user = UserServiceTests.GetDummyUser("Diana", "a@b.com");
			var signUp = new SignupData { Email = "a@b.com", IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = "Diana", Password = "passwerd", PasswordRetype = "passwerd", TimeZone = -5 };
			_userService.Setup(u => u.CreateUser(signUp, It.IsAny<string>())).Returns(user);
			_newAccountMailer.Setup(n => n.Send(It.IsAny<User>(), It.IsAny<string>())).Returns(System.Net.Mail.SmtpStatusCode.CommandNotImplemented);
			var settings = new Settings { IsNewUserApproved = true };
			_settingsManager.Setup(s => s.Current).Returns(settings);
			var authManager = new Mock<IAuthenticationManager>();
			_owinContext.Setup(x => x.Authentication).Returns(authManager.Object);
			var externalAuthResult = new ExternalAuthenticationResult();
			var authResult = Task.FromResult(externalAuthResult);
			_externalAuth.Setup(x => x.GetAuthenticationResult(authManager.Object)).Returns(authResult);

			var result = controller.Create(signUp).Result;

			_userAssociationManager.Verify(x => x.Associate(user, externalAuthResult, It.IsAny<string>()), Times.Once());
		}

		[Test]
		public void CreateValidNotCallExternalAuthAssociateWithoutAuthResult()
		{
			var controller = GetController();
			MockUpUrl(controller);
			_userService.Setup(u => u.IsEmailInUse(It.IsAny<string>())).Returns(false);
			_userService.Setup(u => u.IsNameInUse(It.IsAny<string>())).Returns(false);
			var user = UserServiceTests.GetDummyUser("Diana", "a@b.com");
			var signUp = new SignupData { Email = "a@b.com", IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = "Diana", Password = "passwerd", PasswordRetype = "passwerd", TimeZone = -5 };
			_userService.Setup(u => u.CreateUser(signUp, It.IsAny<string>())).Returns(user);
			_newAccountMailer.Setup(n => n.Send(It.IsAny<User>(), It.IsAny<string>())).Returns(System.Net.Mail.SmtpStatusCode.CommandNotImplemented);
			var settings = new Settings { IsNewUserApproved = true };
			_settingsManager.Setup(s => s.Current).Returns(settings);
			var authManager = new Mock<IAuthenticationManager>();
			_owinContext.Setup(x => x.Authentication).Returns(authManager.Object);
			var authResult = Task.FromResult <ExternalAuthenticationResult>(null);
			_externalAuth.Setup(x => x.GetAuthenticationResult(authManager.Object)).Returns(authResult);

			var result = controller.Create(signUp).Result;

			_userAssociationManager.Verify(x => x.Associate(user, It.IsAny<ExternalAuthenticationResult>(), It.IsAny<string>()), Times.Never);
		}

		private void MockUpUrl(AccountController controller)
		{
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.Url).Returns(new Uri("http://foo/"));
			contextHelper.MockRequestContext.Setup(r => r.HttpContext).Returns(new Mock<HttpContextBase>().Object);
			contextHelper.MockRequestContext.Setup(r => r.RouteData).Returns(new RouteData());
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
		}

		[Test]
		public void CreateNotValid()
		{
			var controller = GetController();
			var context = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(context.MockContext.Object, new RouteData(), controller);
			const string tos = "blah blah blah";
			var settings = new Settings {TermsOfService = tos};
			_settingsManager.Setup(s => s.Current).Returns(settings);
			_userService.Setup(u => u.IsEmailInUse(It.IsAny<string>())).Returns(true);
			_userService.Setup(u => u.IsNameInUse(It.IsAny<string>())).Returns(true);
			var signUp = new SignupData {Email = "a@b.com", IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = "Diana", Password = "pwd", PasswordRetype = "pwd", TimeZone = -5};
			var result = controller.Create(signUp).Result;
			Assert.IsFalse(controller.ModelState.IsValid);
			Assert.IsInstanceOf<ViewResult>(result);
			Assert.IsInstanceOf<String>(controller.ViewData[AccountController.CoppaDateKey]);
			Assert.IsInstanceOf<String>(controller.ViewData[AccountController.TosKey]);
			Assert.AreEqual(tos, controller.ViewData[AccountController.TosKey]);
			_userService.Verify(u => u.CreateUser(It.IsAny<SignupData>(), It.IsAny<string>()), Times.Never());
			_profileService.Verify(p => p.Create(It.IsAny<User>(), It.IsAny<SignupData>()), Times.Never());
			_newAccountMailer.Verify(n => n.Send(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void Verify()
		{
			var user = UserTest.GetTestUser();
			user.AuthorizationKey = Guid.NewGuid();
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns("123");
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			_userService.Setup(u => u.VerifyAuthorizationCode(user.AuthorizationKey, It.IsAny<string>())).Returns(user);
			var result = controller.Verify(user.AuthorizationKey.ToString());
			Assert.IsNotNullOrEmpty(result.ViewData["Result"].ToString());
			Assert.AreEqual(String.Empty, result.ViewName);
		}

		[Test]
		public void VerifyPlainView()
		{
			var controller = GetController();
			var result = controller.Verify("");
			Assert.AreEqual(String.Empty, result.ViewName);
		}

		[Test]
		public void VerifyBadGuid()
		{
			var controller = GetController();
			var result = controller.Verify("123");
			Assert.AreEqual("VerifyFail", result.ViewName);
		}

		[Test]
		public void VerifyNoUser()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns("123");
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			_userService.Setup(u => u.VerifyAuthorizationCode(It.IsAny<Guid>(), It.IsAny<string>())).Returns((User)null);
			var result = controller.Verify(Guid.NewGuid().ToString());
			Assert.AreEqual("VerifyFail", result.ViewName);
		}

		[Test]
		public void VerifyCode()
		{
			var controller = GetController();
			var guidString = Guid.NewGuid().ToString();
			var result = controller.VerifyCode(guidString);
			Assert.IsInstanceOf<RedirectToRouteResult>(result);
			Assert.AreEqual("Verify", result.RouteValues["action"]);
			Assert.AreEqual(guidString, result.RouteValues["id"]);
		}

		[Test]
		public void RequestCodeNoUser()
		{
			var controller = GetController();
			_userService.Setup(u => u.GetUserByEmail(It.IsAny<string>())).Returns((User) null);
			var result = controller.RequestCode("a@b.com");
			Assert.IsNotNullOrEmpty(result.ViewData["Result"].ToString());
			Assert.AreEqual("Verify", result.ViewName);
		}

		[Test]
		public void RequestCodeEmailSuccess()
		{
			var user = UserTest.GetTestUser();
			var controller = GetController();
			MockUpUrl(controller);
			_userService.Setup(u => u.GetUserByEmail(user.Email)).Returns(user);
			_newAccountMailer.Setup(n => n.Send(user, It.IsAny<string>())).Returns(System.Net.Mail.SmtpStatusCode.Ok);
			var result = controller.RequestCode(user.Email);
			Assert.IsNotNullOrEmpty(result.ViewData["Result"].ToString());
			Assert.AreEqual("Verify", result.ViewName);
		}

		[Test]
		public void RequestCodeEmailFail()
		{
			var user = UserTest.GetTestUser();
			var controller = GetController();
			MockUpUrl(controller);
			_userService.Setup(u => u.GetUserByEmail(user.Email)).Returns(user);
			_newAccountMailer.Setup(n => n.Send(user, It.IsAny<string>())).Returns(System.Net.Mail.SmtpStatusCode.GeneralFailure);
			var result = controller.RequestCode(user.Email);
			Assert.IsNotNullOrEmpty(result.ViewData["EmailProblem"].ToString());
			Assert.AreEqual("Verify", result.ViewName);
		}

		[Test]
		public void EditProfileView()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			var profile = new Profile(user.UserID);
			_profileService.Setup(p => p.GetProfileForEdit(user)).Returns(profile);
			var result = controller.EditProfile();
			Assert.IsInstanceOf<UserEditProfile>(result.ViewData.Model);
			Assert.AreNotEqual("EditAccountNoUser", result.ViewName);
			_profileService.Verify(p => p.GetProfileForEdit(user), Times.Once());
		}

		[Test]
		public void EditProfileViewNoUser()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.EditProfile();
			Assert.AreEqual("EditAccountNoUser", result.ViewName);
		}

		[Test]
		public void EditProfile()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			var userEdit = new UserEditProfile();
			var result = controller.EditProfile(userEdit);
			_userService.Verify(u => u.EditUserProfile(user, userEdit), Times.Once());
			Assert.IsNotNullOrEmpty(result.ViewData["Result"].ToString());
		}

		[Test]
		public void EditSecurityView()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			var result = controller.Security();
			Assert.IsInstanceOf<UserEditSecurity>(result.ViewData.Model);
			Assert.AreNotEqual("EditAccountNoUser", result.ViewName);
		}

		[Test]
		public void EditSecurityViewNoUser()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.Security();
			Assert.AreEqual("EditAccountNoUser", result.ViewName);
		}

		[Test]
		public void ChangePasswordNoUser()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.ChangePassword(new UserEditSecurity());
			Assert.AreEqual("EditAccountNoUser", result.ViewName);
		}

		[Test]
		public void ChangePasswordBadOldPassword()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			_userService.Setup(u => u.VerifyPassword(user, It.IsAny<string>())).Returns(false);
			var result = controller.ChangePassword(new UserEditSecurity { OldPassword = "no" });
			Assert.IsNotNullOrEmpty(result.ViewData["PasswordResult"].ToString());
			Assert.AreNotEqual("EditAccountNoUser", result.ViewName);
			_userService.Verify(u => u.SetPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<User>()), Times.Never());
		}

		[Test]
		public void ChangePasswordNoMatch()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			_userService.Setup(u => u.VerifyPassword(user, It.IsAny<string>())).Returns(true);
			var result = controller.ChangePassword(new UserEditSecurity{ NewPassword = "blah", NewPasswordRetype = "blah2"});
			Assert.IsNotNullOrEmpty(result.ViewData["PasswordResult"].ToString());
			Assert.AreNotEqual("EditAccountNoUser", result.ViewName);
			_userService.Verify(u => u.SetPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<User>()), Times.Never());
		}

		[Test]
		public void ChangePasswordFailsRules()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			_userService.Setup(u => u.VerifyPassword(user, It.IsAny<string>())).Returns(true);
			_userService.Setup(u => u.IsPasswordValid(It.IsAny<string>(), It.IsAny<ModelStateDictionary>())).Returns(false).Callback<string, ModelStateDictionary>((p, m) => m.AddModelError("Password", "whatever"));
			var password = "awesome";
			var result = controller.ChangePassword(new UserEditSecurity { NewPassword = password, NewPasswordRetype = password });
			Assert.IsNotNullOrEmpty(result.ViewData["PasswordResult"].ToString());
			Assert.AreNotEqual("EditAccountNoUser", result.ViewName);
			_userService.Verify(u => u.SetPassword(user, password, It.IsAny<string>(), It.IsAny<User>()), Times.Never());
			_userService.Verify(u => u.IsPasswordValid(password, It.IsAny<ModelStateDictionary>()), Times.Once());
		}

		[Test]
		public void ChangePasswordMatchAndValid()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			_userService.Setup(u => u.VerifyPassword(user, It.IsAny<string>())).Returns(true);
			_userService.Setup(u => u.IsPasswordValid(It.IsAny<string>(), It.IsAny<ModelStateDictionary>())).Returns(true);
			var password = "awesome";
			var result = controller.ChangePassword(new UserEditSecurity { NewPassword = password, NewPasswordRetype = password });
			Assert.IsNotNullOrEmpty(result.ViewData["PasswordResult"].ToString());
			Assert.AreNotEqual("EditAccountNoUser", result.ViewName);
			_userService.Verify(u => u.SetPassword(user, password, It.IsAny<string>(), user), Times.Once());
			_userService.Verify(u => u.IsPasswordValid(password, It.IsAny<ModelStateDictionary>()), Times.Once());
		}

		[Test]
		public void ChangeEmailNoUser()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.ChangeEmail(new UserEditSecurity());
			Assert.AreEqual("EditAccountNoUser", result.ViewName);
		}

		[Test]
		public void ChangeEmailBadNewAddress()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			var result = controller.ChangeEmail(new UserEditSecurity { NewEmail = "asdfg", NewEmailRetype = "asdfg" });
			Assert.IsNotNullOrEmpty(result.ViewData["EmailResult"].ToString());
			_userService.Verify(u => u.ChangeEmail(user, It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>()), Times.Never());
			_newAccountMailer.Verify(n => n.Send(user, It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void ChangeEmailNoMatchy()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			var result = controller.ChangeEmail(new UserEditSecurity { NewEmail = "a@b.com", NewEmailRetype = "c@d.com" });
			Assert.IsNotNullOrEmpty(result.ViewData["EmailResult"].ToString());
			_userService.Verify(u => u.ChangeEmail(user, It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>()), Times.Never());
			_newAccountMailer.Verify(n => n.Send(user, It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void ChangeEmailMatchNoApproval()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			_settingsManager.Setup(s => s.Current.IsNewUserApproved).Returns(true);
			var result = controller.ChangeEmail(new UserEditSecurity { NewEmail = "a@b.com", NewEmailRetype = "a@b.com" });
			Assert.IsNotNullOrEmpty(result.ViewData["EmailResult"].ToString());
			_userService.Verify(u => u.ChangeEmail(user, It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>()), Times.Once());
			_newAccountMailer.Verify(n => n.Send(user, It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void ChangeEmailMatchEmailInUser()
		{
			var controller = GetController();
			MockUpUrl(controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			_settingsManager.Setup(s => s.Current.IsNewUserApproved).Returns(false);
			_newAccountMailer.Setup(n => n.Send(user, It.IsAny<string>())).Returns(System.Net.Mail.SmtpStatusCode.Ok);
			_userService.Setup(u => u.IsEmailInUseByDifferentUser(user, It.IsAny<string>())).Returns(true);
			var result = controller.ChangeEmail(new UserEditSecurity { NewEmail = "a@b.com", NewEmailRetype = "a@b.com" });
			Assert.IsNotNullOrEmpty(result.ViewData["EmailResult"].ToString());
			_userService.Verify(u => u.ChangeEmail(user, It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>()), Times.Never());
			_newAccountMailer.Verify(n => n.Send(user, It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void ChangeEmailMatchApprovalEmail()
		{
			var controller = GetController();
			MockUpUrl(controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			_settingsManager.Setup(s => s.Current.IsNewUserApproved).Returns(false);
			_newAccountMailer.Setup(n => n.Send(user, It.IsAny<string>())).Returns(System.Net.Mail.SmtpStatusCode.Ok);
			_userService.Setup(u => u.IsEmailInUseByDifferentUser(user, It.IsAny<string>())).Returns(false);
			var result = controller.ChangeEmail(new UserEditSecurity { NewEmail = "a@b.com", NewEmailRetype = "a@b.com" });
			Assert.IsNotNullOrEmpty(result.ViewData["EmailResult"].ToString());
			_userService.Verify(u => u.ChangeEmail(user, It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>()), Times.Once());
			_newAccountMailer.Verify(n => n.Send(user, It.IsAny<string>()), Times.Once());
		}

		[Test]
		public void ManagePhotosView()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			var profile = new Profile(user.UserID);
			_profileService.Setup(p => p.GetProfile(user)).Returns(profile);
			var result = controller.ManagePhotos();
			Assert.IsInstanceOf<UserEditPhoto>(result.ViewData.Model);
			Assert.AreNotEqual("EditAccountNoUser", result.ViewName);
			_profileService.Verify(p => p.GetProfile(user), Times.Once());
		}

		[Test]
		public void ManagePhotosViewNoUser()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.ManagePhotos();
			Assert.AreEqual("EditAccountNoUser", result.ViewName);
		}

		[Test]
		public void ManagePhotosNoUser()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.ManagePhotos(new UserEditPhoto { DeleteAvatar = true, DeleteImage = true });
			_userService.Verify(u => u.EditUserProfileImages(It.IsAny<User>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<HttpPostedFileBase>(), It.IsAny<HttpPostedFileBase>()), Times.Never());
			Assert.AreEqual("EditAccountNoUser", ((ViewResult)result).ViewName);
		}

		[Test]
		public void ManagePhotos()
		{
			var controller = GetController();
			var context = new HttpContextHelper();
			var mockFileCollection = new Mock<HttpFileCollectionBase>();
			var avatar = new Mock<HttpPostedFileBase>();
			var photo = new Mock<HttpPostedFileBase>();
			mockFileCollection.Setup(f => f["avatarFile"]).Returns(avatar.Object);
			mockFileCollection.Setup(f => f["photoFile"]).Returns(photo.Object);
			context.MockRequest.Setup(r => r.Files).Returns(mockFileCollection.Object);
			controller.ControllerContext = new ControllerContext(context.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			controller.ManagePhotos(new UserEditPhoto{ DeleteAvatar = true, DeleteImage = true });
			_userService.Verify(u => u.EditUserProfileImages(user, true, true, avatar.Object, photo.Object), Times.Once());
		}

		[Test]
		public void MiniProfileNotFound()
		{
			var controller = GetController();
			_userService.Setup(u => u.GetUser(123)).Returns((User) null);
			var result = controller.MiniProfile(123);
			Assert.AreEqual("MiniUserNotFound", result.ViewName);
		}

		[Test]
		public void MiniProfile()
		{
			var controller = GetController();
			var user = UserTest.GetTestUser();
			_userService.Setup(u => u.GetUser(123)).Returns(user);
			_profileService.Setup(u => u.GetProfile(user)).Returns(new Profile {Web = "blahWeb"});
			_postService.Setup(p => p.GetPostCount(user)).Returns(42);
			var result = controller.MiniProfile(123);
			Assert.AreNotEqual("MiniUserNotFound", result.ViewName);
			Assert.AreEqual(123, ((DisplayProfile)result.ViewData.Model).UserID);
			Assert.AreEqual("blahWeb", ((DisplayProfile)result.ViewData.Model).Web);
			Assert.AreEqual(42, ((DisplayProfile) result.ViewData.Model).PostCount);
		}

		[Test]
		public void BadUserEmailDoesntGenForgotPasswordEmail()
		{
			var controller = GetController();
			_userService.Setup(u => u.GetUserByEmail(It.IsAny<string>())).Returns((User)null);
			controller.Forgot(new FormCollection {{"Email", "blah"}});
			_userService.Verify(u => u.GeneratePasswordResetEmail(It.IsAny<User>(), It.IsAny<String>()), Times.Exactly(0));
		}

		[Test]
		public void GoodUserEmailGensForgotPasswordEmail()
		{
			var user = UserTest.GetTestUser();
			var controller = GetController();
			MockUpUrl(controller);
			_userService.Setup(u => u.GetUserByEmail(user.Email)).Returns(user);
			controller.Forgot(new FormCollection { { "Email", user.Email } });
			_userService.Verify(u => u.GeneratePasswordResetEmail(user, It.IsAny<String>()), Times.Exactly(1));
		}

		[Test]
		public void ClientSettingsAreDefaultWithNoUser()
		{
			var controller = GetController();
			controller.ClientSettings();
			_clientSettingsMapper.Verify(c => c.GetDefault(), Times.Exactly(1));
		}

		[Test]
		public void ClientSettingsFromMapperWithUser()
		{
			var controller = GetController();
			var context = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(context.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			var profile = new Profile();
			_profileService.Setup(p => p.GetProfile(user)).Returns(profile);
			controller.ClientSettings();
			_clientSettingsMapper.Verify(c => c.GetClientSettings(profile), Times.Exactly(1));
		}

		[Test]
		public void EmailForbiddenWhenUserHidesDetails()
		{
			var controller = GetController();
			var context = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(context.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			var toUser = new User(124, DateTime.MinValue);
			_userService.Setup(u => u.GetUser(toUser.UserID)).Returns(toUser);
			_userEmailer.Setup(u => u.IsUserEmailable(toUser)).Returns(false);
			controller.EmailUser(toUser.UserID);
			context.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Forbidden);
		}

		[Test]
		public void EmailNotFoundWhenUserDoesntExist()
		{
			var controller = GetController();
			var context = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(context.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			_userService.Setup(u => u.GetUser(It.IsAny<int>())).Returns((User)null);
			controller.EmailUser(987);
			context.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.NotFound);
		}

		[Test]
		public void EmailForbiddenWhenNoUser()
		{
			var controller = GetController();
			var context = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(context.MockContext.Object, new RouteData(), controller);
			controller.EmailUser(654);
			context.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Forbidden);
		}

		[Test]
		public void EmailPostForbiddenWhenUserHidesDetails()
		{
			var controller = GetController();
			var context = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(context.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			var toUser = new User(124, DateTime.MinValue);
			_userService.Setup(u => u.GetUser(toUser.UserID)).Returns(toUser);
			_userEmailer.Setup(u => u.IsUserEmailable(toUser)).Returns(false);
			controller.EmailUser(toUser.UserID, "blah", "mah message");
			context.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Forbidden);
		}

		[Test]
		public void EmailPostNotFoundWhenUserDoesntExist()
		{
			var controller = GetController();
			var context = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(context.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			_userService.Setup(u => u.GetUser(It.IsAny<int>())).Returns((User)null);
			controller.EmailUser(987, "blah", "mah message");
			context.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.NotFound);
		}

		[Test]
		public void EmailPostForbiddenWhenNoUser()
		{
			var controller = GetController();
			var context = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(context.MockContext.Object, new RouteData(), controller);
			controller.EmailUser(654, "blah", "mah message");
			context.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Forbidden);
		}

		[Test]
		public void EmailPostCallsMailer()
		{
			var controller = GetController();
			var context = new HttpContextHelper();
			context.MockRequest.Setup(r => r.UserHostAddress).Returns("1.1.1.1");
			controller.ControllerContext = new ControllerContext(context.MockContext.Object, new RouteData(), controller);
			var user = UserTest.GetTestUser();
			controller.SetUser(user);
			var toUser = new User(124, DateTime.MinValue);
			_userService.Setup(u => u.GetUser(toUser.UserID)).Returns(toUser);
			_userEmailer.Setup(u => u.IsUserEmailable(toUser)).Returns(true);
			controller.EmailUser(toUser.UserID, "blah", "mah message");
			_userEmailer.Verify(u => u.ComposeAndQueue(toUser, user, "1.1.1.1", "blah", "mah message"), Times.Exactly(1));
		}

		[Test]
		public void ViewProfile404ForNoUser()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			_userService.Setup(x => x.GetUser(It.IsAny<int>())).Returns((User) null);
			var result = controller.ViewProfile(123);
			Assert.AreEqual("NotFound", result.ViewName);
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.NotFound);
		}

		[Test]
		public void ViewProfileGetsPostCount()
		{
			var controller = GetController();
			var user = new User(123, DateTime.MinValue);
			const int count = 897;
			_postService.Setup(x => x.GetPostCount(user)).Returns(count);
			_userService.Setup(x => x.GetUser(user.UserID)).Returns(user);
			_profileService.Setup(x => x.GetProfile(user)).Returns(new Profile(user.UserID));
			var result = controller.ViewProfile(user.UserID);
			var displayProfile = (DisplayProfile)result.Model;
			Assert.AreEqual(count, displayProfile.PostCount);
		}

		[Test]
		public void ViewProfileGetsFeed()
		{
			var controller = GetController();
			var user = new User(123, DateTime.MinValue);
			var feed = new List<FeedEvent>();
			_userService.Setup(x => x.GetUser(user.UserID)).Returns(user);
			_feedService.Setup(x => x.GetFeed(user)).Returns(feed);
			_profileService.Setup(x => x.GetProfile(user)).Returns(new Profile(user.UserID));
			var result = controller.ViewProfile(user.UserID);
			var displayProfile = (DisplayProfile) result.Model;
			Assert.AreSame(feed, displayProfile.Feed);
		}

		[Test]
		public void ViewProfileGetsUserAwards()
		{
			var controller = GetController();
			var user = new User(123, DateTime.MinValue);
			var awards = new List<UserAward>();
			_userService.Setup(x => x.GetUser(user.UserID)).Returns(user);
			_userAwardService.Setup(x => x.GetAwards(user)).Returns(awards);
			_profileService.Setup(x => x.GetProfile(user)).Returns(new Profile(user.UserID));
			var result = controller.ViewProfile(user.UserID);
			var displayProfile = (DisplayProfile)result.Model;
			Assert.AreSame(awards, displayProfile.UserAwards);
		}
	}
}
