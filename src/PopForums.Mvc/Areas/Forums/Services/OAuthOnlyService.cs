using PopIdentity;
using PopIdentity.Providers.OAuth2;

namespace PopForums.Mvc.Areas.Forums.Services;

public interface IOAuthOnlyService
{
	string GetLoginUrl(string redirectUrl);
	Task<CallbackResult> ProcessOAuthLogin(string redirectUrl);
}

public class OAuthOnlyService : IOAuthOnlyService
{
	private readonly IConfig _config;
	private readonly IOAuth2LoginUrlGenerator _oAuth2LoginUrlGenerator;
	private readonly IStateHashingService _stateHashingService;
	private readonly IOAuth2JwtCallbackProcessor _oAuth2JwtCallbackProcessor;

	public OAuthOnlyService(IConfig config, IOAuth2LoginUrlGenerator oAuth2LoginUrlGenerator, IStateHashingService stateHashingService, IOAuth2JwtCallbackProcessor oAuth2JwtCallbackProcessor)
	{
		_config = config;
		_oAuth2LoginUrlGenerator = oAuth2LoginUrlGenerator;
		_stateHashingService = stateHashingService;
		_oAuth2JwtCallbackProcessor = oAuth2JwtCallbackProcessor;
	}

	public string GetLoginUrl(string redirectUrl)
	{
		var state = _stateHashingService.SetCookieAndReturnHash();
		var url = _oAuth2LoginUrlGenerator.GetUrl(_config.OAuthLoginBaseUrl, _config.OAuthClientID, redirectUrl, state,
			_config.OAuthScopes);
		return url;
	}

	public async Task<CallbackResult> ProcessOAuthLogin(string redirectUrl)
	{
		var callbackResult = await _oAuth2JwtCallbackProcessor.VerifyCallback(redirectUrl, _config.OAuthTokenUrl,
			_config.OAuthClientID, _config.OAuthClientSecret);
		if (!callbackResult.IsSuccessful)
			return callbackResult;
		
		// lookup the external user
		
		// if not found, create the new user
		
		// if found, verify name/email correct
		


		return callbackResult;
	}
}