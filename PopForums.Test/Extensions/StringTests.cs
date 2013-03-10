using System.Collections.Generic;
using NUnit.Framework;
using PopForums.Extensions;

namespace PopForums.Test.Extensions
{
	[TestFixture]
	public class StringTests
	{
		[Test]
		public void GetMD5HashString()
		{
			var output = "fred".GetMD5Hash();
			Assert.AreEqual("VwqQv7+MfqtdxdTiaDLVsQ==", output);
		}

		[Test]
		public void IsEmailTest()
		{
			Assert.IsTrue("a@b.com".IsEmailAddress());
			Assert.IsTrue("scottgu@microsoft.com".IsEmailAddress());
			Assert.IsTrue("obama@whitehouse.gov".IsEmailAddress());
			Assert.IsTrue("a_b@c.net".IsEmailAddress());
			Assert.IsTrue("a.b@site.co.uk".IsEmailAddress());
		}

		[Test]
		public void IsNoteEmailTest()
		{
			Assert.IsFalse("a@c".IsEmailAddress());
			Assert.IsFalse("a a@c.com".IsEmailAddress());
			Assert.IsFalse("aa@c a.com".IsEmailAddress());
			Assert.IsFalse("a!a@c.com".IsEmailAddress());
			Assert.IsFalse("aa@coishd!iwe.com".IsEmailAddress());
		}

		[Test]
		public void UrlNameTest()
		{
			Assert.AreEqual("abc-def-ghi", "abc def ghi".ToUrlName());
			Assert.AreEqual("abcdef-ghi", "abcdef-ghi".ToUrlName());
			Assert.AreEqual("abcdef-ghi", "abc.def-ghi".ToUrlName());
			Assert.AreEqual("abcdefghi", "abc#def/ghi".ToUrlName());
			Assert.AreEqual("abc----defghi", "abc    def*ghi".ToUrlName());
		}

		[Test]
		public void UrlNameUniqueTest()
		{
			var list = new List<string> { "forum-title", "forum-title-but-not", "forum-title-2" };
			const string title = "forum-title";
			var result = title.ToUniqueUrlName(list);
			Assert.AreEqual("forum-title-3", result);
		}

		[Test]
		public void UrlNameUniqueTestForPlantedDupe()
		{
			var list = new List<string> { "forum-title", "forum-title-2" };
			const string title = "forum-title";
			var result = title.ToUniqueUrlName(list);
			Assert.AreEqual("forum-title-3", result);
		}

		[Test]
		public void UrlNameUniqueTestForDoubleDigits()
		{
			var list = new List<string> { "forum-title", "forum-title-1", "forum-title-2", "forum-title-3", "forum-title-4", "forum-title-5", "forum-title-6", "forum-title-7", "forum-title-8", "forum-title-9", "forum-title-10", "forum-title-11" };
			const string title = "forum-title";
			var result = title.ToUniqueUrlName(list);
			Assert.AreEqual("forum-title-12", result);
		}

		[Test]
		public void TrimmerTest()
		{
			var result = "123456789012345678901234567890".Trimmer(22);
			Assert.AreEqual("123456789...1234567890", result);
		}
	}
}
