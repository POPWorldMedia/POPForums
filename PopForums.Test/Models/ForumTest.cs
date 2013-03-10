using NUnit.Framework;
using PopForums.Models;

namespace PopForums.Test.Models
{
	[TestFixture]
	public class ForumTest
	{
		[Test]
		public void CreateForumObject()
		{
			const int forumID = 123;
			var forum = new Forum(forumID);
			Assert.AreEqual(forumID, forum.ForumID);
		}
	}
}
