namespace PopForums.Messaging;

public interface IBroker
{
	void NotifyNewPosts(Topic topic, int lasPostID);
	void NotifyFeed(string message);
	void NotifyForumUpdate(Forum forum);
	void NotifyTopicUpdate(Topic topic, Forum forum, string topicLink);
	void NotifyNewPost(Topic topic, int postID);
}