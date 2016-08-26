using PopForums.Models;
using Xunit;

namespace PopForums.Test.Models
{
	public class ForumTest
	{
		[Fact]
		public void CreateForumObject()
		{
			const int forumID = 123;
			var forum = new Forum(forumID);
			Assert.Equal(forumID, forum.ForumID);
		}
	}
}
