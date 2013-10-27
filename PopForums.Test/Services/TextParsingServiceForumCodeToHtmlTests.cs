using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.Test.Services
{
	[TestFixture]
	public class TextParsingServiceForumCodeToHtmlTests
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
		public void UrlWithQuotes()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [url=\"http://popw.com/\"]my link[/url].");
			Assert.AreEqual("<p>this is <a href=\"http://popw.com/\" target=\"_blank\">my link</a>.</p>", result);
		}

		[Test]
		public void UrlWithoutQuotes()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [url=http://popw.com/]my link[/url].");
			Assert.AreEqual("<p>this is <a href=\"http://popw.com/\" target=\"_blank\">my link</a>.</p>", result);
		}

		[Test]
		public void MailLinkWithQuotes()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [url=\"mailto:jeff@popw.com\"]my link[/url].");
			Assert.AreEqual("<p>this is <a href=\"mailto:jeff@popw.com\">my link</a>.</p>", result);
		}

		[Test]
		public void MailLinkWithoutQuotes()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [url=mailto:jeff@popw.com]my link[/url].");
			Assert.AreEqual("<p>this is <a href=\"mailto:jeff@popw.com\">my link</a>.</p>", result);
		}

		[Test]
		public void DitchNaughtyJavascriptLinkWithQuotes()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [url=\"javascript:alert('blah')\"]my link[/url].");
			Assert.AreEqual("<p>this is <a href=\"alert('blah')\" target=\"_blank\">my link</a>.</p>", result);
		}

		[Test]
		public void DitchNaughtyJavascriptLinkWithoutQuotes()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [url=javascript:alert('blah')]my link[/url].");
			Assert.AreEqual("<p>this is <a href=\"alert('blah')\" target=\"_blank\">my link</a>.</p>", result);
		}

		[Test]
		public void ReplaceImageTagsWithQuotes()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCodeToHtml("check out the image [image=\"my.jpg\"] here");
			Assert.AreEqual("<p>check out the image <img src=\"my.jpg\" /> here</p>", result);
		}

		[Test]
		public void ReplaceImageTagsWithoutQuotes()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCodeToHtml("check out the image [image=my.jpg] here");
			Assert.AreEqual("<p>check out the image <img src=\"my.jpg\" /> here</p>", result);
		}

		[Test]
		public void RemoveImageTagsWithQuotes()
		{
			var service = GetService();
			_settings.AllowImages = false;
			var result = service.CleanForumCodeToHtml("check out the image [image=\"my.jpg\"] here");
			Assert.AreEqual("<p>check out the image  here</p>", result);
		}

		[Test]
		public void RemoveImageTagsWithoutQuotes()
		{
			var service = GetService();
			_settings.AllowImages = false;
			var result = service.CleanForumCodeToHtml("check out the image [image=my.jpg] here");
			Assert.AreEqual("<p>check out the image  here</p>", result);
		}

		[Test]
		public void ParseClassicImgTags()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCodeToHtml("check out the image [img]http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800[/img] here");
			Assert.AreEqual("<p>check out the image <img src=\"http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800\" /> here</p>", result);
		}

		[Test]
		public void ParseAllThreeImageVariants()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCodeToHtml("[image=http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800]\r\n\r\n[image=\"http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800\"]\r\n\r\n[img]http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800[/img]");
			Assert.AreEqual("<p><img src=\"http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800\" /></p><p><img src=\"http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800\" /></p><p><img src=\"http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800\" /></p>", result);
		}

		[Test]
		public void ReplaceItalic()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [i]italic[/i].");
			Assert.AreEqual("<p>this is <em>italic</em>.</p>", result);
		}

		[Test]
		public void ReplaceBold()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [b]bold[/b].");
			Assert.AreEqual("<p>this is <strong>bold</strong>.</p>", result);
		}

		[Test]
		public void ReplaceCode()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [code]code[/code].");
			Assert.AreEqual("<p>this is <code>code</code>.</p>", result);
		}

		[Test]
		public void ReplacePre()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [pre]pre[/pre].");
			Assert.AreEqual("<p>this is <pre>pre</pre>.</p>", result);
		}

		[Test]
		public void ReplaceLi()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [li]li[/li].");
			Assert.AreEqual("<p>this is <li>li</li>.</p>", result);
		}

		[Test]
		public void ReplaceOl()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [ol]ol[/ol].");
			Assert.AreEqual("<p>this is <ol>ol</ol>.</p>", result);
		}

		[Test]
		public void ReplaceUl()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [ul]ul[/ul].");
			Assert.AreEqual("<p>this is <ul>ul</ul>.</p>", result);
		}

		[Test]
		public void SurroundWithPara()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is some text.");
			Assert.AreEqual("<p>this is some text.</p>", result);
		}

		[Test]
		public void NoParaIfStartsOrEndWithQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote]this is some text.[/quote]");
			Assert.False(result.StartsWith("<p>") || result.EndsWith("</p>"));
		}

		[Test]
		public void ReplaceQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [quote]some[/quote] text.");
			Assert.AreEqual("<p>this is </p><blockquote><p>some</p></blockquote><p> text.</p>", result);
		}

		[Test]
		public void DoubleLineBreakToParaEndStart()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is\r\n\r\nsome text.");
			Assert.AreEqual("<p>this is</p><p>some text.</p>", result);
		}

		[Test]
		public void NoDoubleLineBreakToParaEndStartIfAtQuoteEnd()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote]this is[/quote]\r\n\r\nsome text.");
			Assert.AreEqual("<blockquote><p>this is</p></blockquote><p>some text.</p>", result);
		}

		[Test]
		public void NoDoubleLineBreakToParaEndStartIfAtQuoteStart()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is\r\n\r\n[quote]some text.[/quote]");
			Assert.AreEqual("<p>this is</p><blockquote><p>some text.</p></blockquote>", result);
		}

		[Test]
		public void EliminateLineBreaksBetweenEndParaAndQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("test text\r\n[quote]test quote[/quote]test text\r\n\r\n[quote]test quote[/quote]");
			Assert.AreEqual("<p>test text</p><blockquote><p>test quote</p></blockquote><p>test text</p><blockquote><p>test quote</p></blockquote>", result);
		}

		[Test]
		public void EliminateLineBreaksBetweenEndQuoteAndQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("test text\r\n[quote]test quote[/quote]\r\n\r\n[quote]test quote[/quote]");
			Assert.AreEqual("<p>test text</p><blockquote><p>test quote</p></blockquote><blockquote><p>test quote</p></blockquote>", result);
		}

		[Test]
		public void EliminateLineBreaksBetweenStartAndQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("\r\n\r\n[quote]test quote[/quote]");
			Assert.AreEqual("<blockquote><p>test quote</p></blockquote>", result);
		}

		[Test]
		public void CloseParaBeforeQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("test text\r\n[quote]test quote[/quote]test text\r\n\r\n[quote]test quote[/quote]test text[quote]test quote[/quote]");
			Assert.AreEqual("<p>test text</p><blockquote><p>test quote</p></blockquote><p>test text</p><blockquote><p>test quote</p></blockquote><p>test text</p><blockquote><p>test quote</p></blockquote>", result);
		}

		[Test]
		public void StartInsideOfQuoteWithParaUnlessFirstThingIsSubQuoteOrPara()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote]test quote[/quote][quote][quote]test quote[/quote][/quote]");
			Assert.AreEqual("<blockquote><p>test quote</p></blockquote><blockquote><blockquote><p>test quote</p></blockquote></blockquote>", result);
		}

		[Test]
		public void EndInsideOfQuoteWithParaUnlessFirstThingIsSubQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote]test quote[/quote][quote][quote]test quote[/quote][/quote]");
			Assert.AreEqual("<blockquote><p>test quote</p></blockquote><blockquote><blockquote><p>test quote</p></blockquote></blockquote>", result);
		}

		[Test]
		public void StartParaAfterQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote]test quote[/quote]test text[quote]test quote[/quote]\r\ntest text[quote]test quote[/quote]\r\n\r\ntest text");
			Assert.AreEqual("<blockquote><p>test quote</p></blockquote><p>test text</p><blockquote><p>test quote</p></blockquote><p>test text</p><blockquote><p>test quote</p></blockquote><p>test text</p>", result);
		}

		[Test]
		public void RemoveTrailingLineBreaksBetweenEndsOfQuotes()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote][quote]test quote[/quote]\r\n\r\n[/quote][quote][quote]test quote[/quote]\r\n[/quote]");
			Assert.AreEqual("<blockquote><blockquote><p>test quote</p></blockquote></blockquote><blockquote><blockquote><p>test quote</p></blockquote></blockquote>", result);
		}

		[Test]
		public void EndParaInQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote]test quote[/quote][quote]test quote\r\n[/quote][quote]test quote\r\n\r\n[/quote]");
			Assert.AreEqual("<blockquote><p>test quote</p></blockquote><blockquote><p>test quote</p></blockquote><blockquote><p>test quote</p></blockquote>", result);
		}

		[Test]
		public void StartParaAfterQuote2()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote]test quote[/quote]test text[quote]test quote[/quote]\r\ntest text[quote]test quote[/quote]\r\n\r\ntest text");
			Assert.AreEqual("<blockquote><p>test quote</p></blockquote><p>test text</p><blockquote><p>test quote</p></blockquote><p>test text</p><blockquote><p>test quote</p></blockquote><p>test text</p>", result);
		}

		[Test]
		public void SingleLineBreak()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("test text\r\ntest text");
			Assert.AreEqual("<p>test text<br />test text</p>", result);
		}

		[Test]
		public void DoubleHttpArchiveUrl()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("blah [url=http://web.archive.org/web/20001002225219/http://coasterbuzz.com/forums/]http://web.archive.org/web/20001002225219/http://coasterbuzz.com/forums/[/url] blah");
			Assert.AreEqual("<p>blah <a href=\"http://web.archive.org/web/20001002225219/http://coasterbuzz.com/forums/\" target=\"_blank\">http://web.archive.org/web/20001002225219/http://coasterbuzz.com/forums/</a> blah</p>", result);
		}

		[Test]
		public void YouTubeTagMainDomainConvertedToIframe()
		{
			var service = GetService();
			_settings.YouTubeHeight = 123;
			_settings.YouTubeWidth = 456;
			_settings.AllowImages = true;
			var result = service.CleanForumCodeToHtml("test [youtube=http://youtube.com/watch?v=789] text");
			Assert.AreEqual("<p>test <iframe width=\"456\" height=\"123\" src=\"http://www.youtube.com/embed/789\" frameborder=\"0\" allowfullscreen></iframe> text</p>", result);
		}

		[Test]
		public void YouTubeTagShortDomainConvertedToIframe()
		{
			var service = GetService();
			_settings.YouTubeHeight = 123;
			_settings.YouTubeWidth = 456;
			_settings.AllowImages = true;
			var result = service.CleanForumCodeToHtml("test [youtube=http://youtu.be/789] text");
			Assert.AreEqual("<p>test <iframe width=\"456\" height=\"123\" src=\"http://www.youtube.com/embed/789\" frameborder=\"0\" allowfullscreen></iframe> text</p>", result);
		}

		[Test]
		public void YouTubeTagConvertedToIframe()
		{
			var service = GetService();
			_settings.YouTubeHeight = 123;
			_settings.YouTubeWidth = 456;
			_settings.AllowImages = true;
			var result = service.ForumCodeToHtml("test test [youtube=http://www.youtube.com/watch?v=NL125lBWYc4] test");
			Assert.AreEqual("<p>test test <iframe width=\"456\" height=\"123\" src=\"http://www.youtube.com/embed/NL125lBWYc4\" frameborder=\"0\" allowfullscreen></iframe> test</p>", result);
		}
	}
}