using System.Security.Claims;
using PopIdentity;
using PopIdentity.Providers.OAuth2;

namespace PopForums.Test.Mvc.Services;

public class OAuthOnlyServiceTests
{
	private IConfig _config;
	private IOAuth2LoginUrlGenerator _oAuth2LoginUrlGen;
	private IStateHashingService _stateHashingService;
	private IOAuth2JwtCallbackProcessor _oAuth2JwtCallbackProcessor;
	private IExternalUserAssociationManager _externalUserAssociationManager;
	private IUserService _userService;
	private IClaimsToRoleMapper _claimsToRoleMapper;
	private IUserNameReconciler _userNameReconciler;
	private IUserEmailReconciler _userEmailReconciler;
	private ISecurityLogService _securityLogService;
	
	private OAuthOnlyService GetService()
	{
		_config = Substitute.For<IConfig>();
		_oAuth2LoginUrlGen = Substitute.For<IOAuth2LoginUrlGenerator>();
		_stateHashingService = Substitute.For<IStateHashingService>();
		_oAuth2JwtCallbackProcessor = Substitute.For<IOAuth2JwtCallbackProcessor>();
		_externalUserAssociationManager = Substitute.For<IExternalUserAssociationManager>();
		_userService = Substitute.For<IUserService>();
		_claimsToRoleMapper = Substitute.For<IClaimsToRoleMapper>();
		_userNameReconciler = Substitute.For<IUserNameReconciler>();
		_userEmailReconciler = Substitute.For<IUserEmailReconciler>();
		_securityLogService = Substitute.For<ISecurityLogService>();
		return new OAuthOnlyService(_config, _oAuth2LoginUrlGen, _stateHashingService, _oAuth2JwtCallbackProcessor, _externalUserAssociationManager, _userService, _claimsToRoleMapper, _userNameReconciler, _userEmailReconciler, _securityLogService);
	}

	public class GetLoginUrl : OAuthOnlyServiceTests
	{
		[Fact]
		public void ConfigParameterAndHashValuesCalledToLoginGen()
		{
			var service = GetService();
			_config.OAuthLoginBaseUrl.Returns("baseUrl");
			_config.OAuthClientID.Returns("clientID");
			var redirectUrl = "the redirect url";
			var code = "hash";
			_stateHashingService.SetCookieAndReturnHash().Returns(code);
			_config.OAuthScopes.Returns("openid email profile");

			service.GetLoginUrl(redirectUrl);
			
			_oAuth2LoginUrlGen.Received().GetUrl(_config.OAuthLoginBaseUrl, _config.OAuthClientID, redirectUrl, code, _config.OAuthScopes);
		}

		[Fact]
		public void GeneratedUrlReturned()
		{
			var service = GetService();
			var url = "the return url";
			_oAuth2LoginUrlGen.GetUrl(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(url);

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
			_oAuth2JwtCallbackProcessor.VerifyCallback(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(callbackResult));

			var result = await service.ProcessOAuthLogin("url", "ip");
			
			Assert.False(result.IsSuccessful);
		}

		[Fact]
		public async Task CallbackValuesMappedToExternalCheck()
		{
			var service = GetService();
			var callbackResult = new CallbackResult { IsSuccessful = true, Claims = new List<Claim>(), ResultData = new ResultData{Email = "e", ID = "i", Name = "n"}};
			_oAuth2JwtCallbackProcessor.VerifyCallback(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(callbackResult));
			var externalUserMatch = new ExternalUserAssociationMatchResult { User = new User(), Successful = true };
			ExternalLoginInfo calledExternalInfo = null;
			_externalUserAssociationManager
				.ExternalUserAssociationCheck(Arg.Do<ExternalLoginInfo>(x => calledExternalInfo = x), Arg.Any<string>())
				.Returns(Task.FromResult(externalUserMatch));
			_config.OAuthRefreshExpirationMinutes.Returns(60);

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
			_oAuth2JwtCallbackProcessor.VerifyCallback(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(callbackResult));
			var externalUserMatch = new ExternalUserAssociationMatchResult { User = new User(), Successful = true };
			_externalUserAssociationManager
				.ExternalUserAssociationCheck(Arg.Any<ExternalLoginInfo>(), Arg.Any<string>())
				.Returns(Task.FromResult(externalUserMatch));
			_config.OAuthRefreshExpirationMinutes.Returns(60);

			var result = await service.ProcessOAuthLogin("url", "ip");
			
			await _claimsToRoleMapper.Received().MapRoles(externalUserMatch.User, claims);
		}
		
		[Fact]
		public async Task ExistingUserNameUnchangedNotChanged()
		{
			var service = GetService();
			var user = new User { Name = "Simon" };
			var callbackResult = new CallbackResult { IsSuccessful = true, Claims = new Claim[]{}, ResultData = new ResultData{Email = "e", ID = "i", Name = user.Name}};
			_oAuth2JwtCallbackProcessor.VerifyCallback(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(callbackResult));
			var externalUserMatch = new ExternalUserAssociationMatchResult { User = user, Successful = true };
			_externalUserAssociationManager
				.ExternalUserAssociationCheck(Arg.Any<ExternalLoginInfo>(), Arg.Any<string>())
				.Returns(Task.FromResult(externalUserMatch));
			_userNameReconciler.GetUniqueNameForUser(Arg.Any<string>()).Returns(Task.FromResult(user.Name));

			var result = await service.ProcessOAuthLogin("url", "ip");
			
			await _userService.DidNotReceive().ChangeName(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<User>(), Arg.Any<string>());
		}
		
		[Fact]
		public async Task ExistingUserNameChangedIsChanged()
		{
			var service = GetService();
			var user = new User { Name = "Simon" };
			var callbackResult = new CallbackResult { IsSuccessful = true, Claims = new Claim[]{}, ResultData = new ResultData{Email = "e", ID = "i", Name = "Jeff"}};
			_oAuth2JwtCallbackProcessor.VerifyCallback(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(callbackResult));
			var externalUserMatch = new ExternalUserAssociationMatchResult { User = user, Successful = true };
			_externalUserAssociationManager
				.ExternalUserAssociationCheck(Arg.Any<ExternalLoginInfo>(), Arg.Any<string>())
				.Returns(Task.FromResult(externalUserMatch));
			_userNameReconciler.GetUniqueNameForUser(Arg.Any<string>()).Returns(Task.FromResult("unique"));

			var result = await service.ProcessOAuthLogin("url", "ip");
			
			await _userService.Received().ChangeName(user, "unique", Arg.Any<User>(), Arg.Any<string>());
		}
		
		[Fact]
		public async Task NewUserMappedToClaims()
		{
			var service = GetService();
			var claims = new Claim[] { new("name", "value") };
			var callbackResult = new CallbackResult { IsSuccessful = true, Claims = claims, ResultData = new ResultData{Email = "e", ID = "i", Name = "n"}};
			_oAuth2JwtCallbackProcessor.VerifyCallback(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(callbackResult));
			var externalUserMatch = new ExternalUserAssociationMatchResult { Successful = false };
			_externalUserAssociationManager
				.ExternalUserAssociationCheck(Arg.Any<ExternalLoginInfo>(), Arg.Any<string>())
				.Returns(Task.FromResult(externalUserMatch));
			var user = new User();
			_userService.CreateUserWithProfile(Arg.Any<SignupData>(), Arg.Any<string>())
				.Returns(Task.FromResult(user));
			_config.OAuthRefreshExpirationMinutes.Returns(60);

			var result = await service.ProcessOAuthLogin("url", "ip");
			
			await _claimsToRoleMapper.Received().MapRoles(user, claims);
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
			_config.OAuthTokenUrl.Returns("t");
			_config.OAuthClientID.Returns("c");
			_config.OAuthClientSecret.Returns("s");
			_oAuth2JwtCallbackProcessor.VerifyCallback(redirUrl, "t", "c", "s").Returns(Task.FromResult(callbackResult));
			ExternalLoginInfo externalLoginInfo = null;
			_externalUserAssociationManager
				.ExternalUserAssociationCheck(Arg.Do<ExternalLoginInfo>(x => externalLoginInfo = x), ip)
				.Returns(Task.FromResult(new ExternalUserAssociationMatchResult { Successful = false }));
			var user = new User();
			SignupData signupData = null;
			_userService.CreateUserWithProfile(Arg.Do<SignupData>(x => signupData = x), ip).Returns(Task.FromResult(user));
			_userNameReconciler.GetUniqueNameForUser("Diana").Returns(Task.FromResult("UniqueD"));
			_userEmailReconciler.GetUniqueEmail("a@b.com", "id").Returns(Task.FromResult("uid"));

			await service.ProcessOAuthLogin(redirUrl, ip);
			
			Assert.Equal(callbackResult.ResultData.ID, externalLoginInfo.ProviderKey);
			Assert.Equal("UniqueD", signupData.Name);
			Assert.Equal("uid", signupData.Email);
			await _userService.Received().CreateUserWithProfile(signupData, ip);
			await _externalUserAssociationManager.Received().Associate(user, externalLoginInfo, ip);
		}
	}

	public class AttemptTokenRefresh : OAuthOnlyServiceTests
	{
		[Fact]
		public async Task FailedCallbackMakesNoUpdates()
		{
			var service = GetService();
			_oAuth2JwtCallbackProcessor.GetRefreshToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(new CallbackResult { IsSuccessful = false }));

			await service.AttemptTokenRefresh(Arg.Any<User>());
			
			await _userService.DidNotReceive().UpdateTokenExpiration(Arg.Any<User>(), Arg.Any<DateTime>());
			await _userService.DidNotReceive().UpdateRefreshToken(Arg.Any<User>(), Arg.Any<string>());
		}

		[Fact]
		public async Task GoodTokenUpdatesStuff()
		{
			var service = GetService();
			var user = new User();
			var token = "token";
			var newToken = "newtoken";
			_userService.GetRefreshToken(user).Returns(Task.FromResult(token));
			_config.OAuthTokenUrl.Returns("u");
			_config.OAuthClientID.Returns("c");
			_config.OAuthClientSecret.Returns("s");
			_oAuth2JwtCallbackProcessor.GetRefreshToken(token, "u", "c", "s").Returns(Task.FromResult(new CallbackResult { IsSuccessful = true, RefreshToken = newToken }));

			await service.AttemptTokenRefresh(user);
			
			await _oAuth2JwtCallbackProcessor.Received().GetRefreshToken(token, "u", "c", "s");
			await _userService.Received().UpdateTokenExpiration(user, Arg.Any<DateTime>());
			await _userService.Received().UpdateRefreshToken(user, newToken);
		}
	}
}