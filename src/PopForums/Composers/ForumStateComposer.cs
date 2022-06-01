namespace PopForums.Composers;

public interface IForumStateComposer
{
	ForumState GetState(Forum forum, PagerContext pagerContext);
}

public class ForumStateComposer : IForumStateComposer
{
	public ForumState GetState(Forum forum, PagerContext pagerContext)
	{
		var forumState = new ForumState {ForumID = forum?.ForumID, PageSize = pagerContext.PageSize, PageIndex = pagerContext.PageIndex};
		return forumState;
	}
}