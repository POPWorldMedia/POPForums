using Microsoft.AspNetCore.Http;
using PopForums.Mvc.Areas.Forums.Authorization;

namespace PopForums.Test.Mvc.Authorization;

public class PopForumsUserAttributeTests
{
	private const string BrowserUserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36";

	private HttpRequest GetRequest(string userAgent, string acceptLanguage)
	{
		var context = new DefaultHttpContext();
		if (userAgent != null)
			context.Request.Headers.UserAgent = userAgent;
		if (acceptLanguage != null)
			context.Request.Headers.AcceptLanguage = acceptLanguage;
		return context.Request;
	}

	public class IsNonSessionClient : PopForumsUserAttributeTests
	{
		[Fact]
		public void BrowserIsSessionClient()
		{
			var request = GetRequest(BrowserUserAgent, "en-US,en;q=0.9");

			Assert.False(PopForumsUserAttribute.IsNonSessionClient(request));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		public void MissingUserAgentIsNonSessionClient(string userAgent)
		{
			var request = GetRequest(userAgent, "en-US");

			Assert.True(PopForumsUserAttribute.IsNonSessionClient(request));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		public void MissingAcceptLanguageIsNonSessionClient(string acceptLanguage)
		{
			var request = GetRequest(BrowserUserAgent, acceptLanguage);

			Assert.True(PopForumsUserAttribute.IsNonSessionClient(request));
		}

		[Theory]
		[InlineData("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)")]
		[InlineData("Mozilla/5.0 (compatible; Baiduspider/2.0)")]
		[InlineData("Mozilla/5.0 (compatible; Yahoo! Slurp)")]
		[InlineData("curl/8.4.0")]
		[InlineData("Wget/1.21")]
		[InlineData("python-requests/2.31.0")]
		[InlineData("Go-http-client/2.0")]
		[InlineData("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) HeadlessChrome/120.0.0.0 Safari/537.36")]
		[InlineData("Apache-HttpClient/4.5.14")]
		[InlineData("okhttp/4.12.0")]
		[InlineData("Java/17.0.2")]
		[InlineData("Scrapy/2.11")]
		[InlineData("axios/1.6.0")]
		[InlineData("node-fetch/1.0")]
		public void KnownNonBrowserUserAgentIsNonSessionClient(string userAgent)
		{
			var request = GetRequest(userAgent, "en-US");

			Assert.True(PopForumsUserAttribute.IsNonSessionClient(request));
		}
	}
}
