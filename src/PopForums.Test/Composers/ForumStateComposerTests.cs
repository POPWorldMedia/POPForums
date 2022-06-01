using PopForums.Composers;

namespace PopForums.Test.Composers;

public class ForumStateComposerTests
{
	protected ForumStateComposer GetComposer()
	{
		return new ForumStateComposer();
	}

	public class GetState : ForumStateComposerTests
	{
		[Fact]
		public void MapsCorrectly()
		{
			var composer = GetComposer();
			var pagerContext = new PagerContext {PageCount = 1, PageIndex = 2, PageSize = 3};
			var forum = new Forum {ForumID = 4};

			var result = composer.GetState(forum, pagerContext);

			Assert.Equal(forum.ForumID, result.ForumID);
			Assert.Equal(pagerContext.PageIndex, result.PageIndex);
			Assert.Equal(pagerContext.PageSize, result.PageSize);
		}
	}
}