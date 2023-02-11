using System.Security.Claims;
using PopIdentity;
using PopIdentity.Providers.OAuth2;

namespace PopForums.Test.Mvc.Services;

public class OAuthOnlyServiceTests
{
	private Mock<IConfig> _config;
	private Mock<IOAuth2LoginUrlGenerator> _oAuth2LoginUrlGen;
	private Mock<IStateHashingService> _stateHashingService;
	private Mock<IOAuth2JwtCallbackProcessor> _oAuth2JwtCallbackProcessor;
	private Mock<IExternalUserAssociationManager> _externalUserAssociationManager;
	private Mock<IUserService> _userService;
	private Mock<IOAuthOnlyRoleMapper> oAuthOnlyRoleMapper;
	
	private OAuthOnlyService GetService()
	{
		_config = new Mock<IConfig>();
		_oAuth2LoginUrlGen = new Mock<IOAuth2LoginUrlGenerator>();
		_stateHashingService = new Mock<IStateHashingService>();
		_oAuth2JwtCallbackProcessor = new Mock<IOAuth2JwtCallbackProcessor>();
		_externalUserAssociationManager = new Mock<IExternalUserAssociationManager>();
		_userService = new Mock<IUserService>();
		oAuthOnlyRoleMapper = new Mock<IOAuthOnlyRoleMapper>();
		return new OAuthOnlyService(_config.Object, _oAuth2LoginUrlGen.Object, _stateHashingService.Object, _oAuth2JwtCallbackProcessor.Object, _externalUserAssociationManager.Object, _userService.Object, oAuthOnlyRoleMapper.Object);
	}

	public class GetLoginUrl : OAuthOnlyServiceTests
	{
		[Fact]
		public void ConfigParameterAndHashValuesCalledToLoginGen()
		{
			var service = GetService();
			_config.Setup(x => x.OAuthLoginBaseUrl).Returns("baseUrl");
			_config.Setup(x => x.OAuthClientID).Returns("clientID");
			var redirectUrl = "the redirect url";
			var code = "hash";
			_stateHashingService.Setup(x => x.SetCookieAndReturnHash()).Returns(code);
			_config.Setup(x => x.OAuthScopes).Returns("openid email profile");

			service.GetLoginUrl(redirectUrl);
			
			_oAuth2LoginUrlGen.Verify(x => x.GetUrl(_config.Object.OAuthLoginBaseUrl, _config.Object.OAuthClientID, redirectUrl, code, _config.Object.OAuthScopes), Times.Once);
		}

		[Fact]
		public void GeneratedUrlReturned()
		{
			var service = GetService();
			var url = "the return url";
			_oAuth2LoginUrlGen.Setup(x => x.GetUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(url);

			var result = service.GetLoginUrl("whatever");
			
			Assert.Equal(url, result);
		}
	}

	public class ProcessOAuthLogin : OAuthOnlyServiceTests
	{
		[Fact]
		public async Task FailedCallbackReturnsFail()
		{
			var service = GetService();
			var callbackResult = new CallbackResult { IsSuccessful = false };
			_oAuth2JwtCallbackProcessor.Setup(x => x.VerifyCallback(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(callbackResult);

			var result = await service.ProcessOAuthLogin("url", "ip");
			
			Assert.False(result.IsSuccessful);
		}

		[Fact]
		public async Task CallbackValuesMappedToExternalCheck()
		{
			var service = GetService();
			var callbackResult = new CallbackResult { IsSuccessful = true, Claims = new List<Claim>(), ResultData = new ResultData{Email = "e", ID = "i", Name = "n"}};
			_oAuth2JwtCallbackProcessor.Setup(x => x.VerifyCallback(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(callbackResult);
			var externalUserMatch = new ExternalUserAssociationMatchResult { User = new User(), Successful = true };
			ExternalLoginInfo calledExternalInfo = null;
			_externalUserAssociationManager
				.Setup(x => x.ExternalUserAssociationCheck(It.IsAny<ExternalLoginInfo>(), It.IsAny<string>())).ReturnsAsync(externalUserMatch)
				.Callback<ExternalLoginInfo, string>((e, i) => calledExternalInfo = e);

			var result = await service.ProcessOAuthLogin("url", "ip");
			
			Assert.Equal(callbackResult.ResultData.ID, calledExternalInfo.ProviderKey);
			Assert.Equal(callbackResult.ResultData.Name, calledExternalInfo.ProviderDisplayName);
			Assert.Equal(ProviderType.OAuthOnly.ToString(), calledExternalInfo.LoginProvider);
		}
		
		[Fact]
		public async Task ExistingUserMappedToClaims()
		{
			var service = GetService();
			var claims = new Claim[] { new("name", "value") };
			var callbackResult = new CallbackResult { IsSuccessful = true, Claims = claims, ResultData = new ResultData{Email = "e", ID = "i", Name = "n"}};
			_oAuth2JwtCallbackProcessor.Setup(x => x.VerifyCallback(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(callbackResult);
			var externalUserMatch = new ExternalUserAssociationMatchResult { User = new User(), Successful = true };
			_externalUserAssociationManager
				.Setup(x => x.ExternalUserAssociationCheck(It.IsAny<ExternalLoginInfo>(), It.IsAny<string>()))
				.ReturnsAsync(externalUserMatch);

			var result = await service.ProcessOAuthLogin("url", "ip");
			
			oAuthOnlyRoleMapper.Verify(x => x.MapRoles(externalUserMatch.User, claims), Times.Once);
		}
		
		[Fact]
		public async Task NewUserMappedToClaims()
		{
			var service = GetService();
			var claims = new Claim[] { new("name", "value") };
			var callbackResult = new CallbackResult { IsSuccessful = true, Claims = claims, ResultData = new ResultData{Email = "e", ID = "i", Name = "n"}};
			_oAuth2JwtCallbackProcessor.Setup(x => x.VerifyCallback(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(callbackResult);
			var externalUserMatch = new ExternalUserAssociationMatchResult { Successful = false };
			_externalUserAssociationManager
				.Setup(x => x.ExternalUserAssociationCheck(It.IsAny<ExternalLoginInfo>(), It.IsAny<string>()))
				.ReturnsAsync(externalUserMatch);
			var user = new User();
			_userService.Setup(x => x.CreateUserWithProfile(It.IsAny<SignupData>(), It.IsAny<string>()))
				.ReturnsAsync(user);

			var result = await service.ProcessOAuthLogin("url", "ip");
			
			oAuthOnlyRoleMapper.Verify(x => x.MapRoles(user, claims), Times.Once);
		}
	}
}