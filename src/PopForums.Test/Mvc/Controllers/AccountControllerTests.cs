using System.Net;
using Microsoft.AspNetCore.Http;
using PopForums.Mvc.Areas.Forums.Models;
using PopIdentity;

namespace PopForums.Test.Mvc.Controllers;

public class AccountControllerTests
{
	private IUserService _userService;
	private IProfileService _profileService;
	private ISettingsManager _settingsManager;
	private INewAccountMailer _newAccountMailer;
	private IPostService _postService;
	private ITopicService _topicService;
	private IForumService _forumService;
	private ILastReadService _lastReadService;
	private IImageService _imageService;
	private IFeedService _feedService;
	private IUserAwardService _userAwardService;
	private IExternalUserAssociationManager _externalUserAssocManager;
	private IUserRetrievalShim _userRetrievalShim;
	private IExternalLoginRoutingService _externalLoginRoutingService;
	private IExternalLoginTempService _externalLoginTempService;
	private IConfig _config;
	private IReCaptchaService _recaptchaService;
	private IOAuthOnlyService _oAuthOnlyService;

	private AccountController GetController()
	{
		_userService = Substitute.For<IUserService>();
		_profileService = Substitute.For<IProfileService>();
		_settingsManager = Substitute.For<ISettingsManager>();
		_newAccountMailer = Substitute.For<INewAccountMailer>();
		_postService = Substitute.For<IPostService>();
		_topicService = Substitute.For<ITopicService>();
		_forumService = Substitute.For<IForumService>();
		_lastReadService = Substitute.For<ILastReadService>();
		_imageService = Substitute.For<IImageService>();
		_feedService = Substitute.For<IFeedService>();
		_userAwardService = Substitute.For<IUserAwardService>();
		_externalUserAssocManager = Substitute.For<IExternalUserAssociationManager>();
		_userRetrievalShim = Substitute.For<IUserRetrievalShim>();
		_externalLoginRoutingService = Substitute.For<IExternalLoginRoutingService>();
		_externalLoginTempService = Substitute.For<IExternalLoginTempService>();
		_config = Substitute.For<IConfig>();
		_recaptchaService = Substitute.For<IReCaptchaService>();
		_oAuthOnlyService = Substitute.For<IOAuthOnlyService>();
		var controller = new AccountController(_userService, _profileService, _newAccountMailer, _settingsManager, _postService, _topicService, _forumService, _lastReadService, _imageService, _feedService, _userAwardService, _externalUserAssocManager, _userRetrievalShim, _externalLoginRoutingService, _externalLoginTempService, _config, _recaptchaService, _oAuthOnlyService);
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
			_settingsManager.Current.TermsOfService.Returns("tos");

			var result = controller.Create();
			
			Assert.Equal("tos", (result as ViewResult)?.ViewData[AccountController.TosKey]);
		}
		
		[Fact]
		public void PopulatesValuesFromExternalLogin()
		{
			var controller = GetController();
			_settingsManager.Current.TermsOfService.Returns("tos");
			var externalLoginState = new ExternalLoginState
			{
				ResultData = new ResultData
				{
					Email = "a@b.com",
					Name = "Diana"
				}
			};
			_externalLoginTempService.Read().Returns(externalLoginState);

			var result = controller.Create();

			var signupData = (SignupData)(result as ViewResult)?.Model;
			Assert.Equal("tos", (result as ViewResult)?.ViewData[AccountController.TosKey]);
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
			_userService.VerifyAuthorizationCode(Arg.Any<Guid>(), Arg.Any<string>()).Returns((User)null);

			var result = await controller.Verify("920A89D6-CE1B-4EBE-B758-50DB514B0ABF");
			
			Assert.Equal("VerifyFail", result.ViewName);
		}
		
		[Fact]
		public async Task SuccessReturnViewWithMessage()
		{
			var controller = GetController();
			var user = new User();
			_userService.VerifyAuthorizationCode(Guid.Parse("920A89D6-CE1B-4EBE-B758-50DB514B0ABF"), Arg.Any<string>()).Returns(Task.FromResult(user));

			var result = await controller.Verify("920A89D6-CE1B-4EBE-B758-50DB514B0ABF");
			
			Assert.Null(result.ViewName);
			Assert.Equal(Resources.AccountVerified, result.ViewData["Result"]);
		}
		
		[Fact]
		public async Task SuccessLoginUser()
		{
			var controller = GetController();
			var user = new User();
			_userService.VerifyAuthorizationCode(Guid.Parse("920A89D6-CE1B-4EBE-B758-50DB514B0ABF"), Arg.Any<string>()).Returns(Task.FromResult(user));

			var result = await controller.Verify("920A89D6-CE1B-4EBE-B758-50DB514B0ABF");
			
			await _userService.Received().Login(user, Arg.Any<string>());
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