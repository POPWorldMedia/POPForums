using System;
using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.Test.Services
{
	[TestFixture]
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

		[Test]
		public void ConvertHtmlQuoteToForumCodeQuote()
		{
			var service = GetService();
			var result = service.HtmlToClientHtml("<p>some text</p><blockquote><p>quote text</p></blockquote>");
			Assert.AreEqual("<p>some text</p>[quote]<p>quote text</p>[/quote]", result);
		}

		[Test]
		public void RemoveAnchorTargets()
		{
			var service = GetService();
			var result = service.HtmlToClientHtml("<p>some text <a href=\"http://popw.com/\" target=\"what_ever\">link</a></p>");
			Assert.AreEqual("<p>some text <a href=\"http://popw.com/\">link</a></p>", result);
		}

		[Test]
		public void RemoveYouTubeIframe()
		{
			var service = GetService();
			var result = service.HtmlToClientHtml("<p>test test <iframe width=\"640\" height=\"360\" src=\"http://www.youtube.com/embed/NL125lBWYc4\" frameborder=\"0\" allowfullscreen></iframe> test</p>");
			Assert.AreEqual("<p>test test http://www.youtube.com/watch?v=NL125lBWYc4 test</p>", result);
		}

		[Test]
		public void CensorTheNaughty()
		{
			var service = GetService();
			_settings.CensorWords = "shit bitch  fuck";
			_settings.CensorCharacter = "+";
			var result = service.Censor("this is some shitty fucked up code, bitch");
			Assert.AreEqual("this is some ++++ty ++++ed up code, +++++", result);
		}

		[Test]
		public void CensorTheNaughtyCaseInsensitive()
		{
			var service = GetService();
			_settings.CensorWords = "shit bitch  fuck";
			_settings.CensorCharacter = "+";
			var result = service.Censor("this is some sHitty FucKed up code, bitCH");
			Assert.AreEqual("this is some ++++ty ++++ed up code, +++++", result);
		}

		[Test]
		public void CensorEmptyReturnsEmpty()
		{
			var service = GetService();
			var result = service.Censor(String.Empty);
			Assert.AreEqual(String.Empty, result);
		}

		[Test]
		public void CensorNullReturnsEmpty()
		{
			var service = GetService();
			var result = service.Censor(null);
			Assert.AreEqual(String.Empty, result);
		}

		[Test]
		public void ForumCodeToHtmlReturnsEmptyInsteadOfParaTags()
		{
			var service = GetService();
			var result = service.ForumCodeToHtml("");
			Assert.AreEqual(String.Empty, result);
		}
	}
}