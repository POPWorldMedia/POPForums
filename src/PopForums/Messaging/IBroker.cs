namespace PopForums.Messaging;

public interface IBroker
{
	void NotifyNewPosts(Topic topic, int lasPostID);
	void NotifyForumUpdate(Forum forum);
	void NotifyTopicUpdate(Topic topic, Forum forum, string topicLink);
	void NotifyNewPost(Topic topic, int postID);
	void NotifyPMCount(int userID, int pmCount);
	void NotifyUser(Notification notification);
	void NotifyUser(Notification notification, string tenantID);
	void SendPMMessage(PrivateMessagePost post);
}