namespace PopForums.Services.Interfaces;

public interface ILastReadService
{
    Task MarkForumRead(User user, Forum forum);

    Task MarkAllForumsRead(User user);

    Task MarkTopicRead(User user, Topic topic);

    Task GetForumReadStatus(User user, CategorizedForumContainer container);

    Task GetTopicReadStatus(User user, PagedTopicContainer container);

    Task<Post> GetFirstUnreadPost(User user, Topic topic);

    Task<DateTime> GetTopicReadStatus(User user, Topic topic);

    Task<DateTime> GetForumReadStatus(User user, Forum forum);

    Task<DateTime> GetLastReadTime(User user, Topic topic);
}
