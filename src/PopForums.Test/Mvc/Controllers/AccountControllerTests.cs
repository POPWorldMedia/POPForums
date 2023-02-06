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
		return new AccountController(_userService.Object, _profileService.Object, _newAccountMailer.Object, _settingsManager.Object, _postService.Object, _topicService.Object, _forumService.Object, _lastReadService.Object, _imageService.Object, _feedService.Object, _userAwardService.Object, _externalUserAssocManager.Object, _userRetrievalShim.Object, _externalLoginRoutingService.Object, _externalLoginTempService.Object, _config.Object, _recaptchaService.Object);
	}
	
	public class View : AccountControllerTests
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
}