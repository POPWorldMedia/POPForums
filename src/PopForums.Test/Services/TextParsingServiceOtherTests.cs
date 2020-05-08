using System;
using Moq;
using Xunit;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.Test.Services
{
	public class TextParsingServiceOtherTests
	{
		private TextParsingService GetService()
		{
			_mockSettingsManager = new Mock<ISettingsManager>();
			_settings = new Settings();
			_mockSettingsManager.Setup(s => s.Current).Returns(_settings);
			return new TextParsingService(_mockSettingsManager.Object);
		}

		private Mock<ISettingsManager> _mockSettingsManager;
		private Settings _settings;

		[Fact]
		public void ConvertHtmlQuoteToForumCodeQuote()
		{
			// yes, this is a test to avoid old branches that treated quotes as BB code relics
			var service = GetService();
			var result = service.HtmlToClientHtml("<p>some text</p><blockquote><p>quote text</p></blockquote>");
			Assert.Equal("<p>some text</p><blockquote><p>quote text</p></blockquote>", result);
		}

		[Fact]
		public void RemoveAnchorTargets()
		{
			var service = GetService();
			var result = service.HtmlToClientHtml("<p>some text <a href=\"http://popw.com/\" target=\"what_ever\">link</a></p>");
			Assert.Equal("<p>some text <a href=\"http://popw.com/\">link</a></p>", result);
		}

		[Fact]
		public void RemoveYouTubeIframe()
		{
			var service = GetService();
			var result = service.HtmlToClientHtml("<p>test test <iframe width=\"640\" height=\"360\" src=\"http://www.youtube.com/embed/NL125lBWYc4\" frameborder=\"0\" allowfullscreen></iframe> test</p>");
			Assert.Equal("<p>test test https://www.youtube.com/watch?v=NL125lBWYc4 test</p>", result);
		}

		[Fact]
		public void RemoveYouTubeIframeHttps()
		{
			var service = GetService();
			var result = service.HtmlToClientHtml("<p>test test <iframe width=\"640\" height=\"360\" src=\"https://www.youtube.com/embed/NL125lBWYc4\" frameborder=\"0\" allowfullscreen></iframe> test</p>");
			Assert.Equal("<p>test test https://www.youtube.com/watch?v=NL125lBWYc4 test</p>", result);
		}

		[Fact]
		public void CensorTheNaughty()
		{
			var service = GetService();
			_settings.CensorWords = "shit bitch  fuck";
			_settings.CensorCharacter = "+";
			var result = service.Censor("this is some shitty fucked up code, bitch");
			Assert.Equal("this is some ++++ty ++++ed up code, +++++", result);
		}

		[Fact]
		public void CensorTheNaughtyCaseInsensitive()
		{
			var service = GetService();
			_settings.CensorWords = "shit bitch  fuck";
			_settings.CensorCharacter = "+";
			var result = service.Censor("this is some sHitty FucKed up code, bitCH");
			Assert.Equal("this is some ++++ty ++++ed up code, +++++", result);
		}

		[Fact]
		public void CensorEmptyReturnsEmpty()
		{
			var service = GetService();
			var result = service.Censor(String.Empty);
			Assert.Equal(String.Empty, result);
		}

		[Fact]
		public void CensorNullReturnsEmpty()
		{
			var service = GetService();
			var result = service.Censor(null);
			Assert.Equal(String.Empty, result);
		}

		[Fact]
		public void ForumCodeToHtmlReturnsEmptyInsteadOfParaTags()
		{
			var service = GetService();
			var result = service.ForumCodeToHtml("");
			Assert.Equal(String.Empty, result);
		}
	}
}