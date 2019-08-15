using System.Collections.Generic;
using PopForums.Extensions;
using Xunit;

namespace PopForums.Test.Extensions
{
	public class StringTests
	{
		[Fact]
		public void GetSHA256HashString()
		{
			var output = "fred".GetSHA256Hash();
			Assert.Equal("0M/C5TGbgs3HGjOHPoJsk9fuETY/iskcT6Oiz80ihuU=", output);
		}

		[Fact]
		public void GetMD5HashString()
		{
			var output = "fred".GetMD5Hash();
			Assert.Equal("VwqQv7+MfqtdxdTiaDLVsQ==", output);
		}

		[Fact]
		public void IsEmailTest()
		{
			Assert.True("a@b.com".IsEmailAddress());
			Assert.True("scottgu@microsoft.com".IsEmailAddress());
			Assert.True("obama@whitehouse.gov".IsEmailAddress());
			Assert.True("a_b@c.net".IsEmailAddress());
			Assert.True("a.b@site.co.uk".IsEmailAddress());
		}

		[Fact]
		public void IsNoteEmailTest()
		{
			Assert.False("a@c".IsEmailAddress());
			Assert.False("a a@c.com".IsEmailAddress());
			Assert.False("aa@c a.com".IsEmailAddress());
			Assert.False("a!a@c.com".IsEmailAddress());
			Assert.False("aa@coishd!iwe.com".IsEmailAddress());
		}

		[Fact]
		public void UrlNameTest()
		{
			Assert.Equal("abc-def-ghi", "abc def ghi".ToUrlName());
			Assert.Equal("abcdef-ghi", "abcdef-ghi".ToUrlName());
			Assert.Equal("abcdef-ghi", "abc.def-ghi".ToUrlName());
			Assert.Equal("abcdefghi", "abc#def/ghi".ToUrlName());
			Assert.Equal("abc----defghi", "abc    def*ghi".ToUrlName());
		}

		[Fact]
		public void UrlNameUniqueTest()
		{
			var list = new List<string> { "forum-title", "forum-title-but-not", "forum-title-2" };
			const string title = "forum-title";
			var result = title.ToUniqueUrlName(list);
			Assert.Equal("forum-title-3", result);
		}

		[Fact]
		public void UrlNameUniqueTestForPlantedDupe()
		{
			var list = new List<string> { "forum-title", "forum-title-2" };
			const string title = "forum-title";
			var result = title.ToUniqueUrlName(list);
			Assert.Equal("forum-title-3", result);
		}

		[Fact]
		public void UrlNameUniqueTestForDoubleDigits()
		{
			var list = new List<string> { "forum-title", "forum-title-1", "forum-title-2", "forum-title-3", "forum-title-4", "forum-title-5", "forum-title-6", "forum-title-7", "forum-title-8", "forum-title-9", "forum-title-10", "forum-title-11" };
			const string title = "forum-title";
			var result = title.ToUniqueUrlName(list);
			Assert.Equal("forum-title-12", result);
		}

		[Fact]
		public void TrimmerTest()
		{
			var result = "123456789012345678901234567890".Trimmer(22);
			Assert.Equal("123456789...1234567890", result);
		}
	}
}
