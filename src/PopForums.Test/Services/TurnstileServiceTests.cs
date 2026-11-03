using System.IO;
using System.Net.Http;

namespace PopForums.Test.Services;

public class TurnstileServiceTests
{
	private const string RealSecret = "0x4AAAAAAAreal-secret-key";
	private const string TestSecret = "1x0000000000000000000000000000000AA";

	private IConfig _config;
	private ISecurityLogService _securityLogService;

	private class TestableTurnstileService(IConfig config, ISecurityLogService securityLogService, Func<FormUrlEncodedContent, Task<string>> responder) : TurnstileService(config, securityLogService)
	{
		public string SentForm { get; private set; }

		protected override async Task<string> PostVerifyRequest(FormUrlEncodedContent content)
		{
			SentForm = await content.ReadAsStringAsync();
			return await responder(content);
		}
	}

	private TestableTurnstileService GetService(string secret, string json)
	{
		return GetService(secret, _ => Task.FromResult(json));
	}

	private TestableTurnstileService GetService(string secret, Func<FormUrlEncodedContent, Task<string>> responder)
	{
		_config = Substitute.For<IConfig>();
		_config.TurnstileSecretKey.Returns(secret);
		_securityLogService = Substitute.For<ISecurityLogService>();
		return new TestableTurnstileService(_config, _securityLogService, responder);
	}

	[Fact]
	public async Task SendsSecretTokenAndRemoteIP()
	{
		var service = GetService(RealSecret, """{"success":true,"action":"createaccount"}""");

		await service.VerifyToken("tok", "1.2.3.4");

		Assert.Contains("secret=" + Uri.EscapeDataString(RealSecret), service.SentForm);
		Assert.Contains("response=tok", service.SentForm);
		Assert.Contains("remoteip=1.2.3.4", service.SentForm);
	}

	[Fact]
	public async Task RealKeyWithMatchingActionSucceedsWithoutLog()
	{
		var service = GetService(RealSecret, """{"success":true,"action":"createaccount","hostname":"example.com","error-codes":[]}""");

		var result = await service.VerifyToken("tok", "1.2.3.4");

		Assert.True(result.IsSuccess);
		Assert.Equal("example.com", result.HostName);
		await _securityLogService.DidNotReceiveWithAnyArgs().CreateLogEntry((User)null, null, null, null, default);
	}

	[Fact]
	public async Task RealKeyWithWrongActionFailsAndLogs()
	{
		var service = GetService(RealSecret, """{"success":true,"action":"somethingelse"}""");

		var result = await service.VerifyToken("tok", "1.2.3.4");

		Assert.False(result.IsSuccess);
		await _securityLogService.Received().CreateLogEntry((User)null, null, "1.2.3.4", "unexpected-action:somethingelse", SecurityLogType.CaptchaFailed);
	}

	[Fact]
	public async Task RealKeyWithEmptyActionFails()
	{
		var service = GetService(RealSecret, """{"success":true,"action":""}""");

		var result = await service.VerifyToken("tok", "1.2.3.4");

		Assert.False(result.IsSuccess);
	}

	[Fact]
	public async Task TestKeyWithEmptyActionSucceeds()
	{
		// what Cloudflare's dummy "always passes" secret actually returns
		var service = GetService(TestSecret, """{"success":true,"error-codes":[],"challenge_ts":"2026-11-03T20:21:18.000Z","hostname":"example.com","action":"","cdata":""}""");

		var result = await service.VerifyToken("XXXX.DUMMY.TOKEN.XXXX", "::1");

		Assert.True(result.IsSuccess);
		await _securityLogService.DidNotReceiveWithAnyArgs().CreateLogEntry((User)null, null, null, null, default);
	}

	[Fact]
	public async Task TestKeyMatchesTemplateAppSettings()
	{
		// guards against the template's appsettings and the exemption list drifting apart
		var json = await File.ReadAllTextAsync(Path.Combine(FindRepoRoot(), "src", "PopForums.Web", "appsettings.json"));
		Assert.Contains($"\"SecretKey\": \"{TestSecret}\"", json);
	}

	[Fact]
	public async Task FailureLogsErrorCodes()
	{
		var service = GetService(RealSecret, """{"success":false,"error-codes":["invalid-input-response","timeout-or-duplicate"]}""");

		var result = await service.VerifyToken("tok", "1.2.3.4");

		Assert.False(result.IsSuccess);
		await _securityLogService.Received().CreateLogEntry((User)null, null, "1.2.3.4", "invalid-input-response, timeout-or-duplicate", SecurityLogType.CaptchaFailed);
	}

	[Fact]
	public async Task NetworkErrorFailsClosed()
	{
		var service = GetService(RealSecret, _ => throw new HttpRequestException("nope"));

		var result = await service.VerifyToken("tok", "1.2.3.4");

		Assert.False(result.IsSuccess);
		await _securityLogService.Received().CreateLogEntry((User)null, null, "1.2.3.4", "HttpRequestException", SecurityLogType.CaptchaFailed);
	}

	[Fact]
	public async Task NonJsonResponseFailsClosed()
	{
		var service = GetService(RealSecret, "<html>bad gateway</html>");

		var result = await service.VerifyToken("tok", "1.2.3.4");

		Assert.False(result.IsSuccess);
		await _securityLogService.Received().CreateLogEntry((User)null, null, "1.2.3.4", "JsonException", SecurityLogType.CaptchaFailed);
	}

	private static string FindRepoRoot()
	{
		var dir = new DirectoryInfo(AppContext.BaseDirectory);
		while (dir != null && !File.Exists(Path.Combine(dir.FullName, "PopForums.sln")))
			dir = dir.Parent;
		return dir?.FullName ?? throw new DirectoryNotFoundException("Couldn't find PopForums.sln above the test output folder.");
	}
}
