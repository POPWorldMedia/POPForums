using System.Net;
using Microsoft.AspNetCore.Http;
using PopForums.Mvc.Areas.Forums.Models;
using PopIdentity;

namespace PopForums.Test.Mvc.Controllers;

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
	private Mock<IImageService> _imageService;
	private Mock<IFeedService> _feedService;
	private Mock<IUserAwardService> _userAwardService;
	private Mock<IExternalUserAssociationManager> _externalUserAssocManager;
	private Mock<IUserRetrievalShim> _userRetrievalShim;
	private Mock<IExternalLoginRoutingService> _externalLoginRoutingService;
	private Mock<IExternalLoginTempService> _externalLoginTempService;
	private Mock<IConfig> _config;
	private Mock<IReCaptchaService> _recaptchaService;

	private AccountController GetController()
	{
		_userService = new Mock<IUserService>();
		_profileService = new Mock<IProfileService>();
		_settingsManager = new Mock<ISettingsManager>();
		_newAccountMailer = new Mock<INewAccountMailer>();
		_postService = new Mock<IPostService>();
		_topicService = new Mock<ITopicService>();
		_forumService = new Mock<IForumService>();
		_lastReadService = new Mock<ILastReadService>();
		_imageService = new Mock<IImageService>();
		_feedService = new Mock<IFeedService>();
		_userAwardService = new Mock<IUserAwardService>();
		_externalUserAssocManager = new Mock<IExternalUserAssociationManager>();
		_userRetrievalShim = new Mock<IUserRetrievalShim>();
		_externalLoginRoutingService = new Mock<IExternalLoginRoutingService>();
		_externalLoginTempService = new Mock<IExternalLoginTempService>();
		_config = new Mock<IConfig>();
		_recaptchaService = new Mock<IReCaptchaService>();
		var controller = new AccountController(_userService.Object, _profileService.Object, _newAccountMailer.Object, _settingsManager.Object, _postService.Object, _topicService.Object, _forumService.Object, _lastReadService.Object, _imageService.Object, _feedService.Object, _userAwardService.Object, _externalUserAssocManager.Object, _userRetrievalShim.Object, _externalLoginRoutingService.Object, _externalLoginTempService.Object, _config.Object, _recaptchaService.Object);
		controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext()
		};
		controller.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
		return controller;
	}
	
	public class Create : AccountControllerTests
	{
		[Fact]
		public void PopulatesDefaultValues()
		{
			var controller = GetController();
			_settingsManager.Setup(x => x.Current.TermsOfService).Returns("tos");

			var result = controller.Create();
			
			Assert.Equal("tos", result.ViewData[AccountController.TosKey]);
		}
		
		[Fact]
		public void PopulatesValuesFromExternalLogin()
		{
			var controller = GetController();
			_settingsManager.Setup(x => x.Current.TermsOfService).Returns("tos");
			var externalLoginState = new ExternalLoginState
			{
				ResultData = new ResultData
				{
					Email = "a@b.com",
					Name = "Diana"
				}
			};
			_externalLoginTempService.Setup(x => x.Read()).Returns(externalLoginState);

			var result = controller.Create();

			var signupData = (SignupData)result.Model;
			Assert.Equal("tos", result.ViewData[AccountController.TosKey]);
			Assert.Equal(externalLoginState.ResultData.Email, signupData.Email);
			Assert.Equal(externalLoginState.ResultData.Name, signupData.Name);
		}
	}

	public class Verify : AccountControllerTests
	{
		[Fact]
		public async Task ReturnVerifyFailViewWhenNonGuidCode()
		{
			var controller = GetController();

			var result = await controller.Verify("notaguid");
			
			Assert.Equal("VerifyFail", result.ViewName);
		}
		
		[Fact]
		public async Task ReturnDefaultViewWhenNoCode()
		{
			var controller = GetController();

			var result = await controller.Verify("");
			
			Assert.Null(result.ViewName);
		}
		
		[Fact]
		public async Task ReturnVerifyFailViewWhenGuidMatchesNoUser()
		{
			var controller = GetController();
			_userService.Setup(x => x.VerifyAuthorizationCode(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync((User)null);

			var result = await controller.Verify("920A89D6-CE1B-4EBE-B758-50DB514B0ABF");
			
			Assert.Equal("VerifyFail", result.ViewName);
		}
		
		[Fact]
		public async Task SuccessReturnViewWithMessage()
		{
			var controller = GetController();
			var user = new User();
			_userService.Setup(x => x.VerifyAuthorizationCode(Guid.Parse("920A89D6-CE1B-4EBE-B758-50DB514B0ABF"), It.IsAny<string>())).ReturnsAsync(user);

			var result = await controller.Verify("920A89D6-CE1B-4EBE-B758-50DB514B0ABF");
			
			Assert.Null(result.ViewName);
			Assert.Equal(Resources.AccountVerified, result.ViewData["Result"]);
		}
		
		[Fact]
		public async Task SuccessLoginUser()
		{
			var controller = GetController();
			var user = new User();
			_userService.Setup(x => x.VerifyAuthorizationCode(Guid.Parse("920A89D6-CE1B-4EBE-B758-50DB514B0ABF"), It.IsAny<string>())).ReturnsAsync(user);

			var result = await controller.Verify("920A89D6-CE1B-4EBE-B758-50DB514B0ABF");
			
			_userService.Verify(x => x.Login(user, It.IsAny<string>()), Times.Once);
		}
	}
	
	public class VerifyCode : AccountControllerTests
	{
		[Fact]
		public void RedirectsToVerify()
		{
			var controller = GetController();
			var code = "ED89EDB4-FBDD-494E-9ACF-2FE2AD69D21D";

			var result = controller.VerifyCode(code);

			Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal("Verify", result.ActionName);
			Assert.Equal(code, result.RouteValues?["id"]);
		}
	}
}