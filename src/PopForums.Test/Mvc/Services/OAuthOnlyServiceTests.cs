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
	private Mock<IClaimsToRoleMapper> _claimsToRoleMapper;
	private Mock<IUserNameReconciler> _userNameReconciler;
	private Mock<IUserEmailReconciler> _userEmailReconciler;
	
	private OAuthOnlyService GetService()
	{
		_config = new Mock<IConfig>();
		_oAuth2LoginUrlGen = new Mock<IOAuth2LoginUrlGenerator>();
		_stateHashingService = new Mock<IStateHashingService>();
		_oAuth2JwtCallbackProcessor = new Mock<IOAuth2JwtCallbackProcessor>();
		_externalUserAssociationManager = new Mock<IExternalUserAssociationManager>();
		_userService = new Mock<IUserService>();
		_claimsToRoleMapper = new Mock<IClaimsToRoleMapper>();
		_userNameReconciler = new Mock<IUserNameReconciler>();
		_userEmailReconciler = new Mock<IUserEmailReconciler>();
		return new OAuthOnlyService(_config.Object, _oAuth2LoginUrlGen.Object, _stateHashingService.Object, _oAuth2JwtCallbackProcessor.Object, _externalUserAssociationManager.Object, _userService.Object, _claimsToRoleMapper.Object, _userNameReconciler.Object, _userEmailReconciler.Object);
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
			_config.Setup(x => x.OAuthRefreshExpirationMinutes).Returns(60);

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
			_config.Setup(x => x.OAuthRefreshExpirationMinutes).Returns(60);

			var result = await service.ProcessOAuthLogin("url", "ip");
			
			_claimsToRoleMapper.Verify(x => x.MapRoles(externalUserMatch.User, claims), Times.Once);
		}
		
		[Fact]
		public async Task ExistingUserNameUnchangedNotChanged()
		{
			var service = GetService();
			var user = new User { Name = "Simon" };
			var callbackResult = new CallbackResult { IsSuccessful = true, Claims = new Claim[]{}, ResultData = new ResultData{Email = "e", ID = "i", Name = user.Name}};
			_oAuth2JwtCallbackProcessor.Setup(x => x.VerifyCallback(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(callbackResult);
			var externalUserMatch = new ExternalUserAssociationMatchResult { User = user, Successful = true };
			_externalUserAssociationManager
				.Setup(x => x.ExternalUserAssociationCheck(It.IsAny<ExternalLoginInfo>(), It.IsAny<string>()))
				.ReturnsAsync(externalUserMatch);
			_userNameReconciler.Setup(x => x.GetUniqueNameForUser(It.IsAny<string>())).ReturnsAsync(user.Name);

			var result = await service.ProcessOAuthLogin("url", "ip");
			
			_userService.Verify(x => x.ChangeName(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>()), Times.Never);
		}
		
		[Fact]
		public async Task ExistingUserNameChangedIsChanged()
		{
			var service = GetService();
			var user = new User { Name = "Simon" };
			var callbackResult = new CallbackResult { IsSuccessful = true, Claims = new Claim[]{}, ResultData = new ResultData{Email = "e", ID = "i", Name = "Jeff"}};
			_oAuth2JwtCallbackProcessor.Setup(x => x.VerifyCallback(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(callbackResult);
			var externalUserMatch = new ExternalUserAssociationMatchResult { User = user, Successful = true };
			_externalUserAssociationManager
				.Setup(x => x.ExternalUserAssociationCheck(It.IsAny<ExternalLoginInfo>(), It.IsAny<string>()))
				.ReturnsAsync(externalUserMatch);
			_userNameReconciler.Setup(x => x.GetUniqueNameForUser(It.IsAny<string>())).ReturnsAsync("unique");

			var result = await service.ProcessOAuthLogin("url", "ip");
			
			_userService.Verify(x => x.ChangeName(user, "unique", It.IsAny<User>(), It.IsAny<string>()), Times.Once);
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
			_config.Setup(x => x.OAuthRefreshExpirationMinutes).Returns(60);

			var result = await service.ProcessOAuthLogin("url", "ip");
			
			_claimsToRoleMapper.Verify(x => x.MapRoles(user, claims), Times.Once);
		}

		[Fact]
		public async Task UnmatchedUserIsCreatedAndAssociated()
		{
			var service = GetService();
			var redirUrl = "redir";
			var ip = "127.0.0.1";
			var callbackResult = new CallbackResult
			{
				ResultData = new ResultData
				{
					Email = "a@b.com",
					ID = "id",
					Name = "Diana"
				},
				IsSuccessful = true
			};
			_config.Setup(x => x.OAuthTokenUrl).Returns("t");
			_config.Setup(x => x.OAuthClientID).Returns("c");
			_config.Setup(x => x.OAuthClientSecret).Returns("s");
			_oAuth2JwtCallbackProcessor.Setup(x => x.VerifyCallback(redirUrl, "t", "c", "s")).ReturnsAsync(callbackResult);
			ExternalLoginInfo externalLoginInfo = null;
			_externalUserAssociationManager.Setup(x => x.ExternalUserAssociationCheck(It.IsAny<ExternalLoginInfo>(), ip)).ReturnsAsync(new ExternalUserAssociationMatchResult { Successful = false }).Callback<ExternalLoginInfo, string>((e, i) => externalLoginInfo = e);
			var user = new User();
			SignupData signupData = null;
			_userService.Setup(x => x.CreateUserWithProfile(It.IsAny<SignupData>(), ip)).ReturnsAsync(user).Callback<SignupData, string>((s, i) => signupData = s);
			_userNameReconciler.Setup(x => x.GetUniqueNameForUser("Diana")).ReturnsAsync("UniqueD");
			_userEmailReconciler.Setup(x => x.GetUniqueEmail("a@b.com", "id")).ReturnsAsync("uid");

			await service.ProcessOAuthLogin(redirUrl, ip);
			
			Assert.Equal(callbackResult.ResultData.ID, externalLoginInfo.ProviderKey);
			Assert.Equal("UniqueD", signupData.Name);
			Assert.Equal("uid", signupData.Email);
			_userService.Verify(x => x.CreateUserWithProfile(signupData, ip));
			_externalUserAssociationManager.Verify(x => x.Associate(user, externalLoginInfo, ip));
		}
	}

	public class AttemptTokenRefresh : OAuthOnlyServiceTests
	{
		[Fact]
		public async Task FailedCallbackMakesNoUpdates()
		{
			var service = GetService();
			_oAuth2JwtCallbackProcessor.Setup(x => x.GetRefreshToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new CallbackResult { IsSuccessful = false });

			await service.AttemptTokenRefresh(It.IsAny<User>());
			
			_userService.Verify(x => x.UpdateTokenExpiration(It.IsAny<User>(), It.IsAny<DateTime>()), Times.Never);
			_userService.Verify(x => x.UpdateRefreshToken(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
		}

		[Fact]
		public async Task GoodTokenUpdatesStuff()
		{
			var service = GetService();
			var user = new User();
			var token = "token";
			var newToken = "newtoken";
			_userService.Setup(x => x.GetRefreshToken(user)).ReturnsAsync(token);
			_config.Setup(x => x.OAuthTokenUrl).Returns("u");
			_config.Setup(x => x.OAuthClientID).Returns("c");
			_config.Setup(x => x.OAuthClientSecret).Returns("s");
			_oAuth2JwtCallbackProcessor.Setup(x => x.GetRefreshToken(token, "u", "c", "s")).ReturnsAsync(new CallbackResult { IsSuccessful = true, RefreshToken = newToken });

			await service.AttemptTokenRefresh(user);
			
			_oAuth2JwtCallbackProcessor.Verify(x => x.GetRefreshToken(token, "u", "c", "s"), Times.Once);
			_userService.Verify(x => x.UpdateTokenExpiration(user, It.IsAny<DateTime>()), Times.Once);
			_userService.Verify(x => x.UpdateRefreshToken(user, newToken), Times.Once);
		}
	}
}