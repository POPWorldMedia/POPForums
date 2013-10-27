using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.Test.Services
{
	[TestFixture]
	public class TextParsingServiceCleanForumCodeTests
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
		public void FilterDupeLineBreaks()
		{
			var service = GetService();
			var result = service.CleanForumCode("ahoihfohfo\r\noishfoihg\r\n\r\n\r\nehufhffh \r\n\r\n\r\n\r\nbbb");
			Assert.AreEqual("ahoihfohfo\r\noishfoihg\r\n\r\nehufhffh \r\n\r\nbbb", result);
		}

		[Test]
		public void LeaveNormalLineBreaks()
		{
			var service = GetService();
			var result = service.CleanForumCode("first\r\n\r\nsecond");
			Assert.AreEqual("first\r\n\r\nsecond", result);
		}

		[Test]
		public void ConvertLonelyCarriageReturn()
		{
			var service = GetService();
			var result = service.CleanForumCode("first\nsecond\r\nthird");
			Assert.AreEqual("first\r\nsecond\r\nthird", result);
		}

		[Test]
		public void RemoveImageTagIfImagesDisallowed()
		{
			var service = GetService();
			_settings.AllowImages = false;
			var result = service.CleanForumCode("fff[i]blah[/i] f f8whef 98wy 8wyef [image=blah.jpg]asfd affs[i]blah[/i]");
			Assert.AreEqual("fff[i]blah[/i] f f8whef 98wy 8wyef asfd affs[i]blah[/i]", result);
		}

		[Test]
		public void AllowWellFormedImageTag()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCode("fff[i]blah[/i] f f8whef [image=\"blah.jpg\"] 98wy 8wyef [image=blah.jpg]asfd affs[i]blah[/i]");
			Assert.AreEqual("fff[i]blah[/i] f f8whef [image=\"blah.jpg\"] 98wy 8wyef [image=blah.jpg]asfd affs[i]blah[/i]", result);
		}

		[Test]
		public void RemoveMalFormedImageTag()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCode("fff[i]blah[/i] f f8whef [image \"blah.jpg\"] 98wy 8wyef [image=blah.jpg]asfd [image=\"blah.jpg\"]affs[i]blah[/i]");
			Assert.AreEqual("fff[i]blah[/i] f f8whef  98wy 8wyef [image=blah.jpg]asfd [image=\"blah.jpg\"]affs[i]blah[/i]", result);
		}

		[Test]
		public void CloseUnclosedTag()
		{
			var service = GetService();
			var result = service.CleanForumCode("eorifj e oeihf eorhf [b]eoeirf eriojf");
			Assert.AreEqual("eorifj e oeihf eorhf [b]eoeirf eriojf[/b]", result);
		}

		[Test]
		public void CloseMultipleUnclosedTags()
		{
			var service = GetService();
			var result = service.CleanForumCode("eo[ul]rifj [i]e[/i] oeihf eorhf [b]eoeirf [i]eriojf");
			Assert.AreEqual("eo[ul]rifj [i]e[/i] oeihf eorhf [b]eoeirf [i]eriojf[/i][/b][/ul]", result);
		}

		[Test]
		public void CleanUpOverlappingTags()
		{
			var service = GetService();
			var result = service.CleanForumCode("eo[ul]rifj [i]e[/ul]asdfg[/i] oeihf eorhf [b]eoeirf [i]eriojf[/i][/b]");
			Assert.AreEqual("eo[ul]rifj [i]e[/i][/ul][i]asdfg[/i] oeihf eorhf [b]eoeirf [i]eriojf[/i][/b]", result);
		}

		[Test]
		public void CleanUpOverlappingTags2()
		{
			var service = GetService();
			var result = service.CleanForumCode("now is [i]the time to [b]write good[/i] tests [li]and make[/b] sure that [b][i]everything[/li] is awesome[/i][/b] and stuff");
			Assert.AreEqual("now is [i]the time to [b]write good[/b][/i][b] tests [li]and make[/li][/b][li] sure that [b][i]everything[/i][/b][/li][b][i] is awesome[/i][/b] and stuff", result);
		}

		[Test]
		public void ClosingTagWithoutOpener()
		{
			var service = GetService();
			var result = service.CleanForumCode("ohf whfwofhw h whfweohf[b]oihfowihfwf[/b] ihfwhf [/i]");
			Assert.AreEqual("[i]ohf whfwofhw h whfweohf[b]oihfowihfwf[/b] ihfwhf [/i]", result);
		}

		[Test]
		public void ClosingTagWithoutOpener2()
		{
			var service = GetService();
			var result = service.CleanForumCode("ohf whfwofhw h whfweohf[b]oihfowihfwf[/b] ihfwhf [/i][/li]");
			Assert.AreEqual("[li][i]ohf whfwofhw h whfweohf[b]oihfowihfwf[/b] ihfwhf [/i][/li]", result);
		}

		[Test]
		public void UrlTagOk()
		{
			var service = GetService();
			var result = service.CleanForumCode("ohf whfwofhw h whfweohf[url=\"http://popw.com/\"]oihfo[b]wihfwf[/url] ihfwhf[/b]");
			Assert.AreEqual("ohf whfwofhw h whfweohf[url=\"http://popw.com/\"]oihfo[b]wihfwf[/b][/url][b] ihfwhf[/b]", result);
		}

		[Test]
		public void IgnoreInvalidTag()
		{
			var service = GetService();
			var result = service.CleanForumCode("ohf i oih hgoehi[bad]gaeiorw iowh owahfaowhfwohf");
			Assert.AreEqual("ohf i oih hgoehi[bad]gaeiorw iowh owahfaowhfwohf", result);
		}

		[Test]
		public void TagUrlWithProtocol()
		{
			var service = GetService();
			var result = service.CleanForumCode("ohf i oih hgoehi http://popw.com/ owahfaowhfwohf");
			Assert.AreEqual("ohf i oih hgoehi [url=http://popw.com/]http://popw.com/[/url] owahfaowhfwohf", result);
		}

		[Test]
		public void TagLongUrlWithProtocol()
		{
			var service = GetService();
			var result = service.CleanForumCode("ohf i oih hgoehi http://popw.com/1234567890123456789012345678901234567890123456789012345678901234567890 owahfaowhfwohf");
			Assert.AreEqual("ohf i oih hgoehi [url=http://popw.com/1234567890123456789012345678901234567890123456789012345678901234567890]http://popw.com/123456789012345678901234567890123456789012345678901...1234567890[/url] owahfaowhfwohf", result);
		}

		[Test]
		public void TagWwwUrl()
		{
			var service = GetService();
			var result = service.CleanForumCode("ohf i oih hgoehi www.popw.com owahfaowhfwohf");
			Assert.AreEqual("ohf i oih hgoehi [url=http://www.popw.com]www.popw.com[/url] owahfaowhfwohf", result);
		}

		[Test]
		public void TagLongWwwUrl()
		{
			var service = GetService();
			var result = service.CleanForumCode("ohf i oih hgoehi www.popw.com/1234567890123456789012345678901234567890123456789012345678901234567890 owahfaowhfwohf");
			Assert.AreEqual("ohf i oih hgoehi [url=http://www.popw.com/1234567890123456789012345678901234567890123456789012345678901234567890]www.popw.com/123456789012345678901234567890123456789012345678901234...1234567890[/url] owahfaowhfwohf", result);
		}

		[Test]
		public void TagEmailUrl()
		{
			var service = GetService();
			var result = service.CleanForumCode("ohf i oih hgoehi jeff@popw.com owahfaowhfwohf");
			Assert.AreEqual("ohf i oih hgoehi [url=mailto:jeff@popw.com]jeff@popw.com[/url] owahfaowhfwohf", result);
		}

		[Test]
		public void EscapeHtml()
		{
			var service = GetService();
			var result = service.CleanForumCode("ohf i oih hgoehi <a href=\"javascript:alert('blah')\">indeed</a> owahfaowhfwohf");
			Assert.AreEqual("ohf i oih hgoehi &lt;a href=\"javascript:alert('blah')\"&gt;indeed&lt;/a&gt; owahfaowhfwohf", result);
		}

		[Test]
		public void DontCreateUrlOpenForOrphanCloser()
		{
			var service = GetService();
			var result = service.CleanForumCode("test [b]test[/url] test[/b]");
			Assert.AreEqual("test [b]test test[/b]", result);
		}

		[Test]
		public void DoubleHttpArchiveUrl()
		{
			var service = GetService();
			var result = service.CleanForumCode("blah http://web.archive.org/web/20001002225219/http://coasterbuzz.com/forums/ blah");
			Assert.AreEqual("blah [url=http://web.archive.org/web/20001002225219/http://coasterbuzz.com/forums/]http://web.archive.org/web/20001002225219/http://coasterbuzz.com/forums/[/url] blah", result);
		}

		[Test]
		public void YouTubeHttpOnYouTubeDomain()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCode("blah http://youtube.com/watch?v=12345 blah");
			Assert.AreEqual(result, "blah [youtube=http://youtube.com/watch?v=12345] blah");
		}

		[Test]
		public void YouTubeHttpOnWwwYouTubeDomain()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCode("blah http://www.youtube.com/watch?v=12345 blah");
			Assert.AreEqual(result, "blah [youtube=http://www.youtube.com/watch?v=12345] blah");
		}

		[Test]
		public void YouTubeHttpsOnYouTubeDomain()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCode("blah https://youtube.com/watch?v=12345 blah");
			Assert.AreEqual(result, "blah [youtube=https://youtube.com/watch?v=12345] blah");
		}

		[Test]
		public void YouTubeHttpsOnWwwYouTubeDomain()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCode("blah https://www.youtube.com/watch?v=12345 blah");
			Assert.AreEqual(result, "blah [youtube=https://www.youtube.com/watch?v=12345] blah");
		}

		[Test]
		public void YouTubeHttpOnShortYouTubeDomain()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCode("blah http://youtu.be/12345 blah");
			Assert.AreEqual(result, "blah [youtube=http://youtu.be/12345] blah");
		}

		[Test]
		public void YouTubeHttpsOnShortYouTubeDomain()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCode("blah https://youtu.be/12345 blah");
			Assert.AreEqual(result, "blah [youtube=https://youtu.be/12345] blah");
		}

		[Test]
		public void YouTubeLinkParsedToLinkWithImagesOff()
		{
			var service = GetService();
			_settings.AllowImages = false;
			var result = service.CleanForumCode("blah https://youtu.be/12345 blah");
			Assert.AreEqual(result, "blah [url=https://youtu.be/12345]https://youtu.be/12345[/url] blah");
		}

		[Test]
		public void YouTubeLinkInUrlTagNotParsed()
		{
			var service = GetService();
			_settings.AllowImages = true;
			var result = service.CleanForumCode("blah [url=https://youtu.be/12345]test[/url] blah");
			Assert.AreEqual(result, "blah [url=https://youtu.be/12345]test[/url] blah");
		}
	}
}