using Moq;
using Xunit;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.Test.Services
{
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

		[Fact]
		public void RemoveLineBreaks()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test</p>\r\n<p>test</p>");
			Assert.Equal("test\r\n\r\ntest", result);
		}

		[Fact]
		public void RemoveEmptyLinesWithOnlySpace()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>&nbsp;</p>\r\n<p>&nbsp;</p>");
			Assert.Equal(result, string.Empty);
		}

		[Fact]
		public void DitchStartAndEndPara()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test</p>");
			Assert.Equal("test", result);
		}

		[Fact]
		public void PutQuoteOnItsOwnLines()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test</p><blockquote>quote</blockquote><p>test</p>");
			Assert.Equal("test\r\n\r\n[quote]\r\nquote\r\n[/quote]\r\n\r\ntest", result);
		}

		[Fact]
		public void StartAndEndParaWithBreaks()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test</p><p>test</p>");
			Assert.Equal("test\r\n\r\ntest", result);
		}

		[Fact]
		public void SingleLineBreaks()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test<br/>test<br />test<br>test</p>");
			Assert.Equal("test\r\ntest\r\ntest\r\ntest", result);
		}

		[Fact]
		public void LineBreakInAndOutOfQuote()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<blockquote>quote</blockquote>");
			Assert.Equal("[quote]\r\nquote\r\n[/quote]", result);
		}

		[Fact]
		public void ItalicVariations()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <em>test</Em> <I>test</i> test</p>");
			Assert.Equal("test [i]test[/i] [i]test[/i] test", result);
		}

		[Fact]
		public void BoldVariations()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <sTrOnG>test</Strong> <b>test</B> test</p>");
			Assert.Equal("test [b]test[/b] [b]test[/b] test", result);
		}

		[Fact]
		public void Code()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <cOde>test</code> test</p>");
			Assert.Equal("test [code]test[/code] test", result);
		}

		[Fact]
		public void Pre()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <Pre>test</pRe> test</p>");
			Assert.Equal("test [pre]test[/pre] test", result);
		}

		[Fact]
		public void Li()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <LI>test</lI> test</p>");
			Assert.Equal("test [li]test[/li] test", result);
		}

		[Fact]
		public void Ol()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <oL>test</Ol> test</p>");
			Assert.Equal("test [ol]test[/ol] test", result);
		}

		[Fact]
		public void Ul()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <uL>test</Ul> test</p>");
			Assert.Equal("test [ul]test[/ul] test", result);
		}

		[Fact]
		public void AnchorToUrl()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <a href=\"http://popw.com/\">test</a> test</p>");
			Assert.Equal("test [url=http://popw.com/]test[/url] test", result);
		}

		[Fact]
		public void AnchorToUrlWithTarget()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <a href=\"http://popw.com/\" target=\"_blank\">test</a> test</p>");
			Assert.Equal("test [url=http://popw.com/]test[/url] test", result);
		}

		[Fact]
		public void AnchorToUrlWithTargetNoQuotes()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <a href=\"http://popw.com/\" target=_blank>test</a> test</p>");
			Assert.Equal("test [url=http://popw.com/]test[/url] test", result);
		}

		// These are unlikely to happen, and an updated regex isn't looking for them.
		//[Fact]
		//public void ImageNoClose()
		//{
		//	var service = GetService();
		//	var result = service.ClientHtmlToForumCode("<p>test <img src=\"blah.jpg\"> test</p>");
		//	Assert.Equal("test [image=blah.jpg] test", result);
		//}

		//[Fact]
		//public void ImageNoCloseNoSpace()
		//{
		//	var service = GetService();
		//	var result = service.ClientHtmlToForumCode("<p>test <img src=\"blah.jpg\"/> test</p>");
		//	Assert.Equal("test [image=blah.jpg] test", result);
		//}

		[Fact]
		public void ImageNoCloseSpace()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <img src=\"blah.jpg\" /> test</p>");
			Assert.Equal("test [image=blah.jpg] test", result);
		}

		[Fact]
		public void ImageOtherAttribute()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <img randomatter=\"swegf\" src=\"blah.jpg\" /> test</p>");
			Assert.Equal("test [image=blah.jpg] test", result);
			result = service.ClientHtmlToForumCode("<p>test <img randomatter=\"swegf\" omg=\"wef\" src=\"blah.jpg\" /> test</p>");
			Assert.Equal("test [image=blah.jpg] test", result);
		}

		// these are unlikely to be ever saved in this state
		//[Fact]
		//public void ImageOtherAttributeAfterSrc()
		//{
		//	var service = GetService();
		//	var result = service.ClientHtmlToForumCode("<p>test <img src=\"blah.jpg\" randomatter=\"swegf\" /> test</p>");
		//	Assert.Equal("test [image=blah.jpg] test", result);
		//	result = service.ClientHtmlToForumCode("<p>test <img src=\"blah.jpg\" randomatter=\"swegf\" omg=\"wef\" /> test</p>");
		//	Assert.Equal("test [image=blah.jpg] test", result);
		//}

		//[Fact]
		//public void ImageOtherAttributeBeforeAndAfterSrc()
		//{
		//	var service = GetService();
		//	var result = service.ClientHtmlToForumCode("<p>test <img randomatter=\"swegf\" omg=\"wef\" src=\"blah.jpg\" randomatter=\"swegf\" omg=\"wef\" /> test</p>");
		//	Assert.Equal("test [image=blah.jpg] test", result);
		//}

		[Fact]
		public void NukeInvalidHtml()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test <script>alert('blah');</script> test</p>");
			Assert.Equal("test alert('blah'); test", result);
		}

		[Fact]
		public void NukeInvalidHtml2()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>[quote]<em>WolfBobs said:</em><br><span style=\"font-size: 1rem;\">People need to learn to not blindly follow people based on red and blue. That's why our country has been stagnant with no real change for the good of the people for decades. Good for corporations? Sure. But the people? Not so much.</span><br></p><p>[/quote]It's reassuring to see that some people feel that way.</p>");
			Assert.Equal("[quote]\r\n[i]WolfBobs said:[/i]\r\nPeople need to learn to not blindly follow people based on red and blue. That's why our country has been stagnant with no real change for the good of the people for decades. Good for corporations? Sure. But the people? Not so much.\r\n\r\n\r\n[/quote]It's reassuring to see that some people feel that way.", result);
		}

		[Fact]
		public void ConvertHtmlEscapes()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test &lt; &gt; &amp; &nbsp; test</p>");
			Assert.Equal("test < > &   test", result);
		}

		[Fact]
		public void RemoveLineBreaksInList()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>blah</p><ul>\n<li>first</li>\n<li>second</li>\n<li>third</li>\n</ul><p>blah</p>");
			Assert.Equal("blah\r\n\r\n[ul][li]first[/li][li]second[/li][li]third[/li][/ul]\r\n\r\nblah", result);
		}

		[Fact]
		public void YouTubeUnparse()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test</p><p><iframe width=\"123\" height=\"545\" src=\"http://www.youtube.com/embed/789\" frameborder=\"0\" allowfullscreen></iframe></p><p>test</p>");
			Assert.Equal("test\r\n\r\n[youtube=https://www.youtube.com/watch?v=789]\r\n\r\ntest", result);
		}

		[Fact]
		public void YouTubeUnparseHttps()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test</p><p><iframe width=\"123\" height=\"545\" src=\"https://www.youtube.com/embed/789\" frameborder=\"0\" allowfullscreen></iframe></p><p>test</p>");
			Assert.Equal("test\r\n\r\n[youtube=https://www.youtube.com/watch?v=789]\r\n\r\ntest", result);
		}

		[Fact]
		public void ParseImageWithExtraAttributesLikeAlt()
		{
			var service = GetService();
			var result = service.ClientHtmlToForumCode("<p>test</p>\r\n<p><img src=\"https://scontent.ftpa1-2.fna.fbcdn.net/v/t31.0-8/12119905_10153331542212955_4087525267669435874_o.jpg?_nc_cat=104&amp;_nc_ht=scontent.ftpa1-2.fna&amp;oh=bde1d73b39027f410a9506c19dfb4428&amp;oe=5D95ACD5\" alt=\"\" /></p><p>test</p>");
			Assert.Equal("test\r\n\r\n[image=https://scontent.ftpa1-2.fna.fbcdn.net/v/t31.0-8/12119905_10153331542212955_4087525267669435874_o.jpg?_nc_cat=104&_nc_ht=scontent.ftpa1-2.fna&oh=bde1d73b39027f410a9506c19dfb4428&oe=5D95ACD5]\r\n\r\ntest", result);
		}
	}
}