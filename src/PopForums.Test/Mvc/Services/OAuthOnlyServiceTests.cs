using PopIdentity;
using PopIdentity.Providers.OAuth2;

namespace PopForums.Test.Mvc.Services;

public class OAuthOnlyServiceTests
{
	private Mock<IConfig> _config;
	private Mock<IOAuth2LoginUrlGenerator> _oAuth2LoginUrlGen;
	private Mock<IStateHashingService> _stateHashingService;
	private Mock<IOAuth2JwtCallbackProcessor> _oAuth2JwtCallbackProcessor;
	
	private OAuthOnlyService GetService()
	{
		_config = new Mock<IConfig>();
		_oAuth2LoginUrlGen = new Mock<IOAuth2LoginUrlGenerator>();
		_stateHashingService = new Mock<IStateHashingService>();
		_oAuth2JwtCallbackProcessor = new Mock<IOAuth2JwtCallbackProcessor>();
		return new OAuthOnlyService(_config.Object, _oAuth2LoginUrlGen.Object, _stateHashingService.Object, _oAuth2JwtCallbackProcessor.Object);
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

			var result = await service.ProcessOAuthLogin("url");
			
			Assert.False(result.IsSuccessful);
		}
	}
}