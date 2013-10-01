using System;
using Microsoft.Owin;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using PopForums.Configuration;
using PopForums.Controllers;
using PopForums.Email;
using PopForums.ExternalLogin;
using PopForums.Feeds;
using PopForums.Models;
using PopForums.ScoringGame;
using PopForums.Services;

namespace PopForums.Test.Models
{
	[TestFixture]
	public class SignupDataTests
	{
		private Mock<IUserService> _userManager;
		private Mock<IProfileService> _profileManager;
		private Mock<ISettingsManager> _settingsManager;
		private Mock<INewAccountMailer> _newAccountMailer;
		private Mock<IPostService> _postService;
		private Mock<ITopicService> _topicService;
		private Mock<IForumService> _froumService;
		private Mock<ILastReadService> _lastReadService;
		private Mock<IClientSettingsMapper> _clientSettingsMapper;
		private Mock<IUserEmailer> _userEmailer;
		private Mock<IImageService> _imageService;
		private Mock<IFeedService> _feedService;
		private Mock<IUserAwardService> _userAwardService;
		private Mock<IOwinContext> _owinContext;
		private Mock<IExternalAuthentication> _externalAuth;
		private Mock<IUserAssociationManager> _userAssociationManager;

		private AccountController GetController()
		{
			_userManager = new Mock<IUserService>();
			_profileManager = new Mock<IProfileService>();
			_settingsManager = new Mock<ISettingsManager>();
			_newAccountMailer = new Mock<INewAccountMailer>();
			_postService = new Mock<IPostService>();
			_topicService = new Mock<ITopicService>();
			_froumService = new Mock<IForumService>();
			_lastReadService = new Mock<ILastReadService>();
			_clientSettingsMapper = new Mock<IClientSettingsMapper>();
			_userEmailer = new Mock<IUserEmailer>();
			_imageService = new Mock<IImageService>();
			_feedService = new Mock<IFeedService>();
			_userAwardService = new Mock<IUserAwardService>();
			_owinContext = new Mock<IOwinContext>();
			_externalAuth = new Mock<IExternalAuthentication>();
			_userAssociationManager = new Mock<IUserAssociationManager>();
			return new AccountController(_userManager.Object, _profileManager.Object, _newAccountMailer.Object, _settingsManager.Object, _postService.Object, _topicService.Object, _froumService.Object, _lastReadService.Object, _clientSettingsMapper.Object, _userEmailer.Object, _imageService.Object, _feedService.Object, _userAwardService.Object, _owinContext.Object, _externalAuth.Object, _userAssociationManager.Object);
		}

		[Test]
		public void ValidateTrue()
		{
			var signup = new SignupData {Email = "a@b.com", IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = "Diana", Password = "password", PasswordRetype = "password", TimeZone = -5};
			var controller = GetController();
			_userManager.SetupAllProperties();
			signup.Validate(controller.ModelState, _userManager.Object, String.Empty);
			Assert.IsTrue(controller.ModelState.IsValid);
			Assert.AreEqual(0, controller.ModelState.Keys.Count);
		}

		[Test]
		public void ValidateFalseBannedIP()
		{
			var signup = new SignupData { Email = "a@b.com", IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = "Diana", Password = "password", PasswordRetype = "password", TimeZone = -5 };
			var controller = GetController();
			const string bannedIP = "127.0.0.1";
			_userManager.SetupAllProperties();
			_userManager.Setup(u => u.IsIPBanned(bannedIP)).Returns(true);
			signup.Validate(controller.ModelState, _userManager.Object, bannedIP);
			Assert.False(controller.ModelState.IsValid);
			Assert.AreEqual(1, controller.ModelState.Keys.Count);
			Assert.IsTrue(controller.ModelState.ContainsKey("Email"));
		}

		[Test]
		public void ValidateFalseMultiple()
		{
			var signup = new SignupData { Email = "a@b.com", IsCoppa = false, IsDaylightSaving = true, IsSubscribed = true, IsTos = false, Name = "Diana", Password = "password", PasswordRetype = "password", TimeZone = -5 };
			var controller = GetController();
			_userManager.SetupAllProperties();
			signup.Validate(controller.ModelState, _userManager.Object, String.Empty);
			Assert.IsFalse(controller.ModelState.IsValid);
			Assert.AreEqual(2, controller.ModelState.Keys.Count);
			Assert.IsTrue(controller.ModelState.ContainsKey("IsTos"));
			Assert.IsTrue(controller.ModelState.ContainsKey("IsCoppa"));
		}

		[Test]
		public void ValidateFalseCoppa()
		{
			var signup = new SignupData { Email = "a@b.com", IsCoppa = false, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = "Diana", Password = "password", PasswordRetype = "password", TimeZone = -5 };
			var controller = GetController();
			_userManager.SetupAllProperties();
			signup.Validate(controller.ModelState, _userManager.Object, String.Empty);
			Assert.IsFalse(controller.ModelState.IsValid);
			Assert.AreEqual(1, controller.ModelState.Keys.Count);
			Assert.IsTrue(controller.ModelState.ContainsKey("IsCoppa"));
		}

		[Test]
		public void ValidateFalseTos()
		{
			var signup = new SignupData { Email = "a@b.com", IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = false, Name = "Diana", Password = "password", PasswordRetype = "password", TimeZone = -5 };
			var controller = GetController();
			_userManager.SetupAllProperties();
			signup.Validate(controller.ModelState, _userManager.Object, String.Empty);
			Assert.IsFalse(controller.ModelState.IsValid);
			Assert.AreEqual(1, controller.ModelState.Keys.Count);
			Assert.IsTrue(controller.ModelState.ContainsKey("IsTos"));
		}

		[Test]
		public void ValidateFalseEmailInUse()
		{
			const string email = "a@b.com";
			var signup = new SignupData { Email = email, IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = "Diana", Password = "password", PasswordRetype = "password", TimeZone = -5 };
			var controller = GetController();
			_userManager.SetupAllProperties();
			_userManager.Setup(u => u.IsEmailInUse(email)).Returns(true);
			signup.Validate(controller.ModelState, _userManager.Object, String.Empty);
			Assert.IsFalse(controller.ModelState.IsValid);
			Assert.AreEqual(1, controller.ModelState.Keys.Count);
			Assert.IsTrue(controller.ModelState.ContainsKey("Email"));
			_userManager.Verify(u => u.IsEmailInUse(email), Times.Once());
		}

		[Test]
		public void ValidateFalseNameInUse()
		{
			const string name = "Diana";
			var signup = new SignupData { Email = "a@b.com", IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = name, Password = "password", PasswordRetype = "password", TimeZone = -5 };
			var controller = GetController();
			_userManager.SetupAllProperties();
			_userManager.Setup(u => u.IsNameInUse(name)).Returns(true);
			signup.Validate(controller.ModelState, _userManager.Object, String.Empty);
			Assert.IsFalse(controller.ModelState.IsValid);
			Assert.AreEqual(1, controller.ModelState.Keys.Count);
			Assert.IsTrue(controller.ModelState.ContainsKey("Name"));
			_userManager.Verify(u => u.IsNameInUse(name), Times.Once());
		}

		[Test]
		public void ValidateFalsePasswordsDontMatch()
		{
			var signup = new SignupData { Email = "a@b.com", IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = "Diana", Password = "password", PasswordRetype = "p wd", TimeZone = -5 };
			var controller = GetController();
			_userManager.SetupAllProperties();
			signup.Validate(controller.ModelState, _userManager.Object, String.Empty);
			Assert.IsFalse(controller.ModelState.IsValid);
			Assert.AreEqual(1, controller.ModelState.Keys.Count);
			Assert.IsTrue(controller.ModelState.ContainsKey("PasswordRetype"));
		}

		[Test]
		public void ValidateFalsePasswordsTooShort()
		{
			var signup = new SignupData { Email = "a@b.com", IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = "Diana", Password = "pword", PasswordRetype = "pword", TimeZone = -5 };
			var controller = GetController();
			_userManager.SetupAllProperties();
			_userManager.Setup(u => u.IsPasswordValid(signup.Password, controller.ModelState)).Returns(false).Callback(() => controller.ModelState.AddModelError("Password", "whatever"));
			signup.Validate(controller.ModelState, _userManager.Object, String.Empty);
			Assert.IsFalse(controller.ModelState.IsValid);
			Assert.AreEqual(1, controller.ModelState.Keys.Count);
			Assert.IsTrue(controller.ModelState.ContainsKey("Password"));
		}

		[Test]
		public void ValidateFalseNameRequired()
		{
			var signup = new SignupData { Email = "a@b.com", IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = String.Empty, Password = "password", PasswordRetype = "password", TimeZone = -5 };
			var controller = GetController();
			_userManager.SetupAllProperties();
			signup.Validate(controller.ModelState, _userManager.Object, String.Empty);
			Assert.IsFalse(controller.ModelState.IsValid);
			Assert.AreEqual(1, controller.ModelState.Keys.Count);
			Assert.IsTrue(controller.ModelState.ContainsKey("Name"));
		}

		[Test]
		public void ValidateFalseNameNull()
		{
			var signup = new SignupData { Email = "a@b.com", IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = null, Password = "password", PasswordRetype = "password", TimeZone = -5 };
			var controller = GetController();
			_userManager.SetupAllProperties();
			signup.Validate(controller.ModelState, _userManager.Object, String.Empty);
			Assert.IsFalse(controller.ModelState.IsValid);
			Assert.AreEqual(1, controller.ModelState.Keys.Count);
			Assert.IsTrue(controller.ModelState.ContainsKey("Name"));
		}

		[Test]
		public void ValidateFalseEmailRequired()
		{
			var signup = new SignupData { Email = String.Empty, IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = "Diana", Password = "password", PasswordRetype = "password", TimeZone = -5 };
			var controller = GetController();
			_userManager.SetupAllProperties();
			signup.Validate(controller.ModelState, _userManager.Object, String.Empty);
			Assert.IsFalse(controller.ModelState.IsValid);
			Assert.AreEqual(1, controller.ModelState.Keys.Count);
			Assert.IsTrue(controller.ModelState.ContainsKey("Email"));
		}

		[Test]
		public void ValidateFalseEmailNull()
		{
			var signup = new SignupData { Email = null, IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = "Diana", Password = "password", PasswordRetype = "password", TimeZone = -5 };
			var controller = GetController();
			_userManager.SetupAllProperties();
			signup.Validate(controller.ModelState, _userManager.Object, String.Empty);
			Assert.IsFalse(controller.ModelState.IsValid);
			Assert.AreEqual(1, controller.ModelState.Keys.Count);
			Assert.IsTrue(controller.ModelState.ContainsKey("Email"));
		}

		[Test]
		public void ValidateFalseEmailBad()
		{
			var signup = new SignupData { Email = "oihef oefw", IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = "Diana", Password = "password", PasswordRetype = "password", TimeZone = -5 };
			var controller = GetController();
			_userManager.SetupAllProperties();
			signup.Validate(controller.ModelState, _userManager.Object, String.Empty);
			Assert.IsFalse(controller.ModelState.IsValid);
			Assert.AreEqual(1, controller.ModelState.Keys.Count);
			Assert.IsTrue(controller.ModelState.ContainsKey("Email"));
		}

		[Test]
		public void ValidateEmailBanned()
		{
			var signup = new SignupData { Email = "a@b.com", IsCoppa = true, IsDaylightSaving = true, IsSubscribed = true, IsTos = true, Name = "Diana", Password = "password", PasswordRetype = "password", TimeZone = -5 };
			var controller = GetController();
			_userManager.SetupAllProperties();
			_userManager.Setup(u => u.IsEmailBanned("a@b.com")).Returns(true);
			signup.Validate(controller.ModelState, _userManager.Object, String.Empty);
			Assert.IsFalse(controller.ModelState.IsValid);
			Assert.AreEqual(1, controller.ModelState.Keys.Count);
			Assert.IsTrue(controller.ModelState.ContainsKey("Email"));
		}

		[Test]
		public void GetCoppaDateString()
		{
			Assert.AreEqual(DateTime.Now.AddYears(-13).ToLongDateString(), SignupData.GetCoppaDate());
		}
	}
}
