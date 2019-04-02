using Moq;
using Xunit;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.Test.Services
{
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
		
		[Fact]
		public void UrlWithQuotes()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [url=\"http://popw.com/\"]my link[/url].");
			Assert.Equal("<p>this is <a href=\"http://popw.com/\" target=\"_blank\">my link</a>.</p>", result);
		}

		[Fact]
		public void UrlWithoutQuotes()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [url=http://popw.com/]my link[/url].");
			Assert.Equal("<p>this is <a href=\"http://popw.com/\" target=\"_blank\">my link</a>.</p>", result);
		}

		[Fact]
		public void MailLinkWithQuotes()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [url=\"mailto:jeff@popw.com\"]my link[/url].");
			Assert.Equal("<p>this is <a href=\"mailto:jeff@popw.com\">my link</a>.</p>", result);
		}

		[Fact]
		public void MailLinkWithoutQuotes()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [url=mailto:jeff@popw.com]my link[/url].");
			Assert.Equal("<p>this is <a href=\"mailto:jeff@popw.com\">my link</a>.</p>", result);
		}

		[Fact]
		public void DitchNaughtyJavascriptLinkWithQuotes()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [url=\"javascript:alert('blah')\"]my link[/url].");
			Assert.Equal("<p>this is <a href=\"alert('blah')\" target=\"_blank\">my link</a>.</p>", result);
		}

		[Fact]
		public void DitchNaughtyJavascriptLinkWithoutQuotes()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [url=javascript:alert('blah')]my link[/url].");
			Assert.Equal("<p>this is <a href=\"alert('blah')\" target=\"_blank\">my link</a>.</p>", result);
		}

		[Fact]
		public void ReplaceImageTagsWithQuotes()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCodeToHtml("check out the image [image=\"my.jpg\"] here");
			Assert.Equal("<p>check out the image <img src=\"my.jpg\" /> here</p>", result);
		}

		[Fact]
		public void ReplaceImageTagsWithoutQuotes()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCodeToHtml("check out the image [image=my.jpg] here");
			Assert.Equal("<p>check out the image <img src=\"my.jpg\" /> here</p>", result);
		}

		[Fact]
		public void RemoveImageTagsWithQuotes()
		{
			var service = GetService();
			_settings.AllowImages = false;
			var result = service.CleanForumCodeToHtml("check out the image [image=\"my.jpg\"] here");
			Assert.Equal("<p>check out the image  here</p>", result);
		}

		[Fact]
		public void RemoveImageTagsWithoutQuotes()
		{
			var service = GetService();
			_settings.AllowImages = false;
			var result = service.CleanForumCodeToHtml("check out the image [image=my.jpg] here");
			Assert.Equal("<p>check out the image  here</p>", result);
		}

		[Fact]
		public void ParseClassicImgTags()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCodeToHtml("check out the image [img]http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800[/img] here");
			Assert.Equal("<p>check out the image <img src=\"http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800\" /> here</p>", result);
		}

		[Fact]
		public void ParseAllThreeImageVariants()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCodeToHtml("[image=http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800]\r\n\r\n[image=\"http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800\"]\r\n\r\n[img]http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800[/img]");
			Assert.Equal("<p><img src=\"http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800\" /></p><p><img src=\"http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800\" /></p><p><img src=\"http://coasterbuzz.com/CoasterPhoto/CoasterPhotoImage/4800\" /></p>", result);
		}

		[Fact]
		public void ReplaceItalic()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [i]italic[/i].");
			Assert.Equal("<p>this is <em>italic</em>.</p>", result);
		}

		[Fact]
		public void ReplaceBold()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [b]bold[/b].");
			Assert.Equal("<p>this is <strong>bold</strong>.</p>", result);
		}

		[Fact]
		public void ReplaceCode()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [code]code[/code].");
			Assert.Equal("<p>this is <code>code</code>.</p>", result);
		}

		[Fact]
		public void ReplacePre()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [pre]pre[/pre].");
			Assert.Equal("<p>this is <pre>pre</pre>.</p>", result);
		}

		[Fact]
		public void ReplaceLi()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [li]li[/li].");
			Assert.Equal("<p>this is <li>li</li>.</p>", result);
		}

		[Fact]
		public void ReplaceOl()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [ol]ol[/ol].");
			Assert.Equal("<p>this is <ol>ol</ol>.</p>", result);
		}

		[Fact]
		public void ReplaceUl()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [ul]ul[/ul].");
			Assert.Equal("<p>this is <ul>ul</ul>.</p>", result);
		}

		[Fact]
		public void SurroundWithPara()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is some text.");
			Assert.Equal("<p>this is some text.</p>", result);
		}

		[Fact]
		public void NoParaIfStartsOrEndWithQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote]this is some text.[/quote]");
			Assert.False(result.StartsWith("<p>") || result.EndsWith("</p>"));
		}

		[Fact]
		public void ReplaceQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is [quote]some[/quote] text.");
			Assert.Equal("<p>this is </p><blockquote><p>some</p></blockquote><p> text.</p>", result);
		}

		[Fact]
		public void DoubleLineBreakToParaEndStart()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is\r\n\r\nsome text.");
			Assert.Equal("<p>this is</p><p>some text.</p>", result);
		}

		[Fact]
		public void NoDoubleLineBreakToParaEndStartIfAtQuoteEnd()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote]this is[/quote]\r\n\r\nsome text.");
			Assert.Equal("<blockquote><p>this is</p></blockquote><p>some text.</p>", result);
		}

		[Fact]
		public void NoDoubleLineBreakToParaEndStartIfAtQuoteStart()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("this is\r\n\r\n[quote]some text.[/quote]");
			Assert.Equal("<p>this is</p><blockquote><p>some text.</p></blockquote>", result);
		}

		[Fact]
		public void EliminateLineBreaksBetweenEndParaAndQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("test text\r\n[quote]test quote[/quote]test text\r\n\r\n[quote]test quote[/quote]");
			Assert.Equal("<p>test text</p><blockquote><p>test quote</p></blockquote><p>test text</p><blockquote><p>test quote</p></blockquote>", result);
		}

		[Fact]
		public void EliminateLineBreaksBetweenEndQuoteAndQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("test text\r\n[quote]test quote[/quote]\r\n\r\n[quote]test quote[/quote]");
			Assert.Equal("<p>test text</p><blockquote><p>test quote</p></blockquote><blockquote><p>test quote</p></blockquote>", result);
		}

		[Fact]
		public void EliminateLineBreaksBetweenStartAndQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("\r\n\r\n[quote]test quote[/quote]");
			Assert.Equal("<blockquote><p>test quote</p></blockquote>", result);
		}

		[Fact]
		public void CloseParaBeforeQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("test text\r\n[quote]test quote[/quote]test text\r\n\r\n[quote]test quote[/quote]test text[quote]test quote[/quote]");
			Assert.Equal("<p>test text</p><blockquote><p>test quote</p></blockquote><p>test text</p><blockquote><p>test quote</p></blockquote><p>test text</p><blockquote><p>test quote</p></blockquote>", result);
		}

		[Fact]
		public void StartInsideOfQuoteWithParaUnlessFirstThingIsSubQuoteOrPara()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote]test quote[/quote][quote][quote]test quote[/quote][/quote]");
			Assert.Equal("<blockquote><p>test quote</p></blockquote><blockquote><blockquote><p>test quote</p></blockquote></blockquote>", result);
		}

		[Fact]
		public void EndInsideOfQuoteWithParaUnlessFirstThingIsSubQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote]test quote[/quote][quote][quote]test quote[/quote][/quote]");
			Assert.Equal("<blockquote><p>test quote</p></blockquote><blockquote><blockquote><p>test quote</p></blockquote></blockquote>", result);
		}

		[Fact]
		public void StartParaAfterQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote]test quote[/quote]test text[quote]test quote[/quote]\r\ntest text[quote]test quote[/quote]\r\n\r\ntest text");
			Assert.Equal("<blockquote><p>test quote</p></blockquote><p>test text</p><blockquote><p>test quote</p></blockquote><p>test text</p><blockquote><p>test quote</p></blockquote><p>test text</p>", result);
		}

		[Fact]
		public void RemoveTrailingLineBreaksBetweenEndsOfQuotes()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote][quote]test quote[/quote]\r\n\r\n[/quote][quote][quote]test quote[/quote]\r\n[/quote]");
			Assert.Equal("<blockquote><blockquote><p>test quote</p></blockquote></blockquote><blockquote><blockquote><p>test quote</p></blockquote></blockquote>", result);
		}

		[Fact]
		public void EndParaInQuote()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote]test quote[/quote][quote]test quote\r\n[/quote][quote]test quote\r\n\r\n[/quote]");
			Assert.Equal("<blockquote><p>test quote</p></blockquote><blockquote><p>test quote</p></blockquote><blockquote><p>test quote</p></blockquote>", result);
		}

		[Fact]
		public void StartParaAfterQuote2()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("[quote]test quote[/quote]test text[quote]test quote[/quote]\r\ntest text[quote]test quote[/quote]\r\n\r\ntest text");
			Assert.Equal("<blockquote><p>test quote</p></blockquote><p>test text</p><blockquote><p>test quote</p></blockquote><p>test text</p><blockquote><p>test quote</p></blockquote><p>test text</p>", result);
		}

		[Fact]
		public void SingleLineBreak()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("test text\r\ntest text");
			Assert.Equal("<p>test text<br />test text</p>", result);
		}

		[Fact]
		public void DoubleHttpArchiveUrl()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml("blah [url=http://web.archive.org/web/20001002225219/http://coasterbuzz.com/forums/]http://web.archive.org/web/20001002225219/http://coasterbuzz.com/forums/[/url] blah");
			Assert.Equal("<p>blah <a href=\"http://web.archive.org/web/20001002225219/http://coasterbuzz.com/forums/\" target=\"_blank\">http://web.archive.org/web/20001002225219/http://coasterbuzz.com/forums/</a> blah</p>", result);
		}

		[Fact]
		public void YouTubeTagMainDomainConvertedToIframe()
		{
			var service = GetService();
			_settings.YouTubeHeight = 123;
			_settings.YouTubeWidth = 456;
			_settings.AllowImages = true;
			var result = service.CleanForumCodeToHtml("test [youtube=http://youtube.com/watch?v=789] text");
			Assert.Equal("<p>test <iframe width=\"456\" height=\"123\" src=\"https://www.youtube.com/embed/789\" frameborder=\"0\" allowfullscreen></iframe> text</p>", result);
		}

		[Fact]
		public void YouTubeTagMainDomainHttpsConvertedToIframe()
		{
			var service = GetService();
			_settings.YouTubeHeight = 123;
			_settings.YouTubeWidth = 456;
			_settings.AllowImages = true;
			var result = service.CleanForumCodeToHtml("test [youtube=https://youtube.com/watch?v=789] text");
			Assert.Equal("<p>test <iframe width=\"456\" height=\"123\" src=\"https://www.youtube.com/embed/789\" frameborder=\"0\" allowfullscreen></iframe> text</p>", result);
		}

		[Fact]
		public void YouTubeTagShortDomainConvertedToIframe()
		{
			var service = GetService();
			_settings.YouTubeHeight = 123;
			_settings.YouTubeWidth = 456;
			_settings.AllowImages = true;
			var result = service.CleanForumCodeToHtml("test [youtube=http://youtu.be/789] text");
			Assert.Equal("<p>test <iframe width=\"456\" height=\"123\" src=\"https://www.youtube.com/embed/789\" frameborder=\"0\" allowfullscreen></iframe> text</p>", result);
		}

		[Fact]
		public void YouTubeTagShortDomainHttpsConvertedToIframe()
		{
			var service = GetService();
			_settings.YouTubeHeight = 123;
			_settings.YouTubeWidth = 456;
			_settings.AllowImages = true;
			var result = service.CleanForumCodeToHtml("test [youtube=https://youtu.be/789] text");
			Assert.Equal("<p>test <iframe width=\"456\" height=\"123\" src=\"https://www.youtube.com/embed/789\" frameborder=\"0\" allowfullscreen></iframe> text</p>", result);
		}

		[Fact]
		public void YouTubeTagConvertedToIframe()
		{
			var service = GetService();
			_settings.YouTubeHeight = 123;
			_settings.YouTubeWidth = 456;
			_settings.AllowImages = true;
			var result = service.ForumCodeToHtml("test test [youtube=http://www.youtube.com/watch?v=NL125lBWYc4] test");
			Assert.Equal("<p>test test <iframe width=\"456\" height=\"123\" src=\"https://www.youtube.com/embed/NL125lBWYc4\" frameborder=\"0\" allowfullscreen></iframe> test</p>", result);
		}

		[Fact]
		public void YouTubeTagHttpsConvertedToIframe()
		{
			var service = GetService();
			_settings.YouTubeHeight = 123;
			_settings.YouTubeWidth = 456;
			_settings.AllowImages = true;
			var result = service.ForumCodeToHtml("test test [youtube=https://www.youtube.com/watch?v=NL125lBWYc4] test");
			Assert.Equal("<p>test test <iframe width=\"456\" height=\"123\" src=\"https://www.youtube.com/embed/NL125lBWYc4\" frameborder=\"0\" allowfullscreen></iframe> test</p>", result);
		}

		[Fact]
		public void UrlWithBangParsesCorrectly()
		{
			var service = GetService();
			var result = service.ForumCodeToHtml("(and [url=\"https://groups.google.com/forum/#!original/rec.roller-coaster/iwTIvU2IXKI/hKB_D9uRbaEJ\"]'millennium' is spelled with two n's[/url] regardless of whether the new one starts in 2000 or 2001.)");
			Assert.Equal("<p>(and <a href=\"https://groups.google.com/forum/#!original/rec.roller-coaster/iwTIvU2IXKI/hKB_D9uRbaEJ\" target=\"_blank\">'millennium' is spelled with two n's</a> regardless of whether the new one starts in 2000 or 2001.)</p>", result);
		}

		[Fact]
		public void DontParagraphAnEmptyString()
		{
			var service = GetService();
			var result = service.CleanForumCodeToHtml(string.Empty);
			Assert.Equal(result, string.Empty);
		}
	}
}