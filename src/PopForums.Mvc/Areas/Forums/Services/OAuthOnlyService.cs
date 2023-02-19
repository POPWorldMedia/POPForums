using PopIdentity;
using PopIdentity.Providers.OAuth2;

namespace PopForums.Mvc.Areas.Forums.Services;

public interface IOAuthOnlyService
{
	string GetLoginUrl(string redirectUrl);
	Task<CallbackResult> ProcessOAuthLogin(string redirectUrl, string ip);
	Task<bool> AttemptTokenRefresh(User user);
}

public class OAuthOnlyService : IOAuthOnlyService
{
	private readonly IConfig _config;
	private readonly IOAuth2LoginUrlGenerator _oAuth2LoginUrlGenerator;
	private readonly IStateHashingService _stateHashingService;
	private readonly IOAuth2JwtCallbackProcessor _oAuth2JwtCallbackProcessor;
	private readonly IExternalUserAssociationManager _externalUserAssociationManager;
	private readonly IUserService _userService;
	private readonly IClaimsToRoleMapper _claimsToRoleMapper;
	private readonly IUserNameReconciler _userNameReconciler;
	private readonly IUserEmailReconciler _userEmailReconciler;
	private readonly ISecurityLogService _securityLogService;

	public OAuthOnlyService(IConfig config, IOAuth2LoginUrlGenerator oAuth2LoginUrlGenerator, IStateHashingService stateHashingService, IOAuth2JwtCallbackProcessor oAuth2JwtCallbackProcessor, IExternalUserAssociationManager externalUserAssociationManager, IUserService userService, IClaimsToRoleMapper claimsToRoleMapper, IUserNameReconciler userNameReconciler, IUserEmailReconciler userEmailReconciler, ISecurityLogService securityLogService)
	{
		_config = config;
		_oAuth2LoginUrlGenerator = oAuth2LoginUrlGenerator;
		_stateHashingService = stateHashingService;
		_oAuth2JwtCallbackProcessor = oAuth2JwtCallbackProcessor;
		_externalUserAssociationManager = externalUserAssociationManager;
		_userService = userService;
		_claimsToRoleMapper = claimsToRoleMapper;
		_userNameReconciler = userNameReconciler;
		_userEmailReconciler = userEmailReconciler;
		_securityLogService = securityLogService;
	}

	public string GetLoginUrl(string redirectUrl)
	{
		var state = _stateHashingService.SetCookieAndReturnHash();
		var url = _oAuth2LoginUrlGenerator.GetUrl(_config.OAuthLoginBaseUrl, _config.OAuthClientID, redirectUrl, state,
			_config.OAuthScopes);
		return url;
	}

	public async Task<CallbackResult> ProcessOAuthLogin(string redirectUrl, string ip)
	{
		var callbackResult = await _oAuth2JwtCallbackProcessor.VerifyCallback(redirectUrl, _config.OAuthTokenUrl,
			_config.OAuthClientID, _config.OAuthClientSecret);
		if (!callbackResult.IsSuccessful)
		{
			await _securityLogService.CreateLogEntry((User)null, null, ip, callbackResult.Message, SecurityLogType.ExternalLoginChallengeFailed);
			return callbackResult;
		}
		if (string.IsNullOrEmpty(callbackResult.ResultData.Name))
		{
			callbackResult.IsSuccessful = false;
			callbackResult.Message = "Identity provider did not return a name.";
		}
		if (string.IsNullOrEmpty(callbackResult.ResultData.Email))
		{
			callbackResult.IsSuccessful = false;
			callbackResult.Message = "Identity provider did not return an email.";
		}
		if (string.IsNullOrEmpty(callbackResult.ResultData.ID))
		{
			callbackResult.IsSuccessful = false;
			callbackResult.Message = "Identity provider did not return a unique identifier.";
		}
		
		// lookup the external user
		var externalLoginInfo = new ExternalLoginInfo(
			ProviderType.OAuthOnly.ToString(), 
			callbackResult.ResultData.ID, 
			callbackResult.ResultData.Name);
		var matchResult = await _externalUserAssociationManager.ExternalUserAssociationCheck(externalLoginInfo, ip);

		User user;
		if (!matchResult.Successful)
		{
			// if not found, create the new user
			// reconcile email
			var uniqueEmail = await _userEmailReconciler.GetUniqueEmail(callbackResult.ResultData.Email, callbackResult.ResultData.ID);
			// reconcile name
			var uniqueName = await _userNameReconciler.GetUniqueNameForUser(callbackResult.ResultData.Name);
			var signupData = new SignupData
			{
				Name = uniqueName,
				Email = uniqueEmail,
				Password = Guid.NewGuid().ToString(),
				IsCoppa = true,
				IsTos = true,
				IsSubscribed = true,
				IsAutoFollowOnReply = true
			};
			user = await _userService.CreateUserWithProfile(signupData, ip);
			await _externalUserAssociationManager.Associate(user, externalLoginInfo, ip);
		}
		else
		{
			// if found, verify name is correct
			user = matchResult.User;
			// reconcile name
			if (user.Name != callbackResult.ResultData.Name)
			{
				var updatedName = await _userNameReconciler.GetUniqueNameForUser(callbackResult.ResultData.Name);
				await _userService.ChangeName(user, updatedName, null, ip);
			}
		}
		
		// set the token expiration
		await _userService.UpdateTokenExpiration(user, DateTime.UtcNow.AddMinutes(_config.OAuthRefreshExpirationMinutes));
		// update refresh token
		await _userService.UpdateRefreshToken(user, callbackResult.RefreshToken);
		
		// set admin/mod based on claims
		await _claimsToRoleMapper.MapRoles(user, callbackResult.Claims);

		return callbackResult;
	}

	public async Task<bool> AttemptTokenRefresh(User user)
	{
		var previousToken = await _userService.GetRefreshToken(user);
		var callbackResult = await _oAuth2JwtCallbackProcessor.GetRefreshToken(previousToken, _config.OAuthTokenUrl,
			_config.OAuthClientID, _config.OAuthClientSecret);
		if (callbackResult.IsSuccessful)
		{
			await _userService.UpdateRefreshToken(user, callbackResult.RefreshToken);
			await _userService.UpdateTokenExpiration(user, DateTime.UtcNow.AddMinutes(_config.OAuthRefreshExpirationMinutes));
		}
		return callbackResult.IsSuccessful;
	}
}