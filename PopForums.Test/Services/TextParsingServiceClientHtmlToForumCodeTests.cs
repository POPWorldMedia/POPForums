using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.Test.Services
{
	[TestFixture]
	public class TextParsingServiceClientHtmlToForumCodeTests
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
		public void RemoveLineBreaks()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test</p>\r\n<p>test</p>");
			Assert.AreEqual("test\r\n\r\ntest", result);
		}

		[Test]
		public void DitchStartAndEndPara()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test</p>");
			Assert.AreEqual("test", result);
		}

		[Test]
		public void PutQuoteOnItsOwnLines()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test</p><blockquote>quote</blockquote><p>test</p>");
			Assert.AreEqual("test\r\n\r\n[quote]\r\nquote\r\n[/quote]\r\n\r\ntest", result);
		}

		[Test]
		public void StartAndEndParaWithBreaks()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test</p><p>test</p>");
			Assert.AreEqual("test\r\n\r\ntest", result);
		}

		[Test]
		public void SingleLineBreaks()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test<br/>test<br />test<br>test</p>");
			Assert.AreEqual("test\r\ntest\r\ntest\r\ntest", result);
		}

		[Test]
		public void LineBreakInAndOutOfQuote()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<blockquote>quote</blockquote>");
			Assert.AreEqual("[quote]\r\nquote\r\n[/quote]", result);
		}

		[Test]
		public void ItalicVariations()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <em>test</Em> <I>test</i> test</p>");
			Assert.AreEqual("test [i]test[/i] [i]test[/i] test", result);
		}

		[Test]
		public void BoldVariations()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <sTrOnG>test</Strong> <b>test</B> test</p>");
			Assert.AreEqual("test [b]test[/b] [b]test[/b] test", result);
		}

		[Test]
		public void Code()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <cOde>test</code> test</p>");
			Assert.AreEqual("test [code]test[/code] test", result);
		}

		[Test]
		public void Pre()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <Pre>test</pRe> test</p>");
			Assert.AreEqual("test [pre]test[/pre] test", result);
		}

		[Test]
		public void Li()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <LI>test</lI> test</p>");
			Assert.AreEqual("test [li]test[/li] test", result);
		}

		[Test]
		public void Ol()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <oL>test</Ol> test</p>");
			Assert.AreEqual("test [ol]test[/ol] test", result);
		}

		[Test]
		public void Ul()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <uL>test</Ul> test</p>");
			Assert.AreEqual("test [ul]test[/ul] test", result);
		}

		[Test]
		public void AnchorToUrl()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <a href=\"http://popw.com/\">test</a> test</p>");
			Assert.AreEqual("test [url=http://popw.com/]test[/url] test", result);
		}

		[Test]
		public void AnchorToUrlWithTarget()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <a href=\"http://popw.com/\" target=\"_blank\">test</a> test</p>");
			Assert.AreEqual("test [url=http://popw.com/]test[/url] test", result);
		}

		[Test]
		public void AnchorToUrlWithTargetNoQuotes()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <a href=\"http://popw.com/\" target=_blank>test</a> test</p>");
			Assert.AreEqual("test [url=http://popw.com/]test[/url] test", result);
		}

		[Test]
		public void ImageNoClose()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <img src=\"blah.jpg\"> test</p>");
			Assert.AreEqual("test [image=blah.jpg] test", result);
		}

		[Test]
		public void ImageNoCloseNoSpace()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <img src=\"blah.jpg\"/> test</p>");
			Assert.AreEqual("test [image=blah.jpg] test", result);
		}

		[Test]
		public void ImageNoCloseSpace()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <img src=\"blah.jpg\" /> test</p>");
			Assert.AreEqual("test [image=blah.jpg] test", result);
		}

		[Test]
		public void ImageOtherAttribute()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <img randomatter=\"swegf\" src=\"blah.jpg\" /> test</p>");
			Assert.AreEqual("test [image=blah.jpg] test", result);
			result = service.ClientHtmlToForumCode("<p>test <img randomatter=\"swegf\" omg=\"wef\" src=\"blah.jpg\" /> test</p>");
			Assert.AreEqual("test [image=blah.jpg] test", result);
		}

		[Test]
		public void ImageOtherAttributeAfterSrc()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <img src=\"blah.jpg\" randomatter=\"swegf\" /> test</p>");
			Assert.AreEqual("test [image=blah.jpg] test", result);
			result = service.ClientHtmlToForumCode("<p>test <img src=\"blah.jpg\" randomatter=\"swegf\" omg=\"wef\" /> test</p>");
			Assert.AreEqual("test [image=blah.jpg] test", result);
		}

		[Test]
		public void ImageOtherAttributeBeforeAndAfterSrc()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <img randomatter=\"swegf\" omg=\"wef\" src=\"blah.jpg\" randomatter=\"swegf\" omg=\"wef\" /> test</p>");
			Assert.AreEqual("test [image=blah.jpg] test", result);
		}

		[Test]
		public void NukeInvalidHtml()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <script>alert('blah');</script> test</p>");
			Assert.AreEqual("test  test", result);
		}

		[Test]
		public void ConvertHtmlEscapes()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test &lt; &gt; &amp; &nbsp; test</p>");
			Assert.AreEqual("test < > &   test", result);
		}

		[Test]
		public void RemoveLineBreaksInList()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>blah</p><ul>\n<li>first</li>\n<li>second</li>\n<li>third</li>\n</ul><p>blah</p>");
			Assert.AreEqual("blah\r\n\r\n[ul][li]first[/li][li]second[/li][li]third[/li][/ul]\r\n\r\nblah", result);
		}

		[Test]
		public void YouTubeUnparse()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test</p><p><iframe width=\"123\" height=\"545\" src=\"http://www.youtube.com/embed/789\" frameborder=\"0\" allowfullscreen></iframe></p><p>test</p>");
			Assert.AreEqual("test\r\n\r\n[youtube=http://www.youtube.com/watch?v=789]\r\n\r\ntest", result);
		}
	}
}