namespace PopForums.Services;

public interface ITurnstileService
{
	Task<TurnstileResponse> VerifyToken(string token, string ip);
}

public class TurnstileService : ITurnstileService
{
	private readonly IConfig _config;
	private readonly ISecurityLogService _securityLogService;
	private const string VerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
	public const string CreateAccountAction = "createaccount";

	// Cloudflare's published dummy secret keys (always pass, always fail, token already spent). Their canned siteverify responses don't echo the widget's action, so the action check can't apply to them.
	// https://developers.cloudflare.com/turnstile/troubleshooting/testing/
	private static readonly HashSet<string> TestSecretKeys = new()
	{
		"1x0000000000000000000000000000000AA",
		"2x0000000000000000000000000000000AA",
		"3x0000000000000000000000000000000AA"
	};

	public TurnstileService(IConfig config, ISecurityLogService securityLogService)
	{
		_config = config;
		_securityLogService = securityLogService;
	}

	public async Task<TurnstileResponse> VerifyToken(string token, string ip)
	{
		var values = new Dictionary<string, string>
		{
			{"secret", _config.TurnstileSecretKey},
			{"response", token ?? string.Empty},
			{"remoteip", ip ?? string.Empty}
		};
		TurnstileResponse verifyResult;
		try
		{
			var content = await PostVerifyRequest(new FormUrlEncodedContent(values));
			verifyResult = JsonSerializer.Deserialize<TurnstileResponse>(content);
		}
		catch (HttpRequestException)
		{
			verifyResult = new TurnstileResponse { IsSuccess = false, ErrorCodes = new[] { "HttpRequestException" } };
		}
		catch (JsonException)
		{
			verifyResult = new TurnstileResponse { IsSuccess = false, ErrorCodes = new[] { "JsonException" } };
		}
		// a valid token rendered for some other widget on the same site key shouldn't count for sign-up
		if (verifyResult.IsSuccess && !TestSecretKeys.Contains(_config.TurnstileSecretKey ?? string.Empty) && verifyResult.Action != CreateAccountAction)
		{
			verifyResult.IsSuccess = false;
			verifyResult.ErrorCodes = new[] { $"unexpected-action:{verifyResult.Action}" };
		}
		if (!verifyResult.IsSuccess)
		{
			var message = verifyResult.ErrorCodes is { Length: > 0 } ? string.Join(", ", verifyResult.ErrorCodes) : string.Empty;
			await _securityLogService.CreateLogEntry((User) null, null, ip, message, SecurityLogType.CaptchaFailed);
		}
		return verifyResult;
	}

	// the network call to Cloudflare, separated so tests can supply canned responses
	protected virtual async Task<string> PostVerifyRequest(FormUrlEncodedContent content)
	{
		using var client = new HttpClient();
		var httpResult = await client.PostAsync(VerifyUrl, content);
		return await httpResult.Content.ReadAsStringAsync();
	}
}

public class TurnstileResponse
{
	[JsonPropertyName("success")]
	public bool IsSuccess { get; set; }
	[JsonPropertyName("challenge_ts")]
	public DateTime ChallengeTimeStamp { get; set; }
	[JsonPropertyName("hostname")]
	public string HostName { get; set; }
	[JsonPropertyName("error-codes")]
	public string[] ErrorCodes { get; set; }
	[JsonPropertyName("action")]
	public string Action { get; set; }
}
