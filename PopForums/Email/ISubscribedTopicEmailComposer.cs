using PopForums.Models;

namespace PopForums.Email
{
	public interface ISubscribedTopicEmailComposer
	{
		void ComposeAndQueue(Topic topic, User user, string topicLink, string unsubscribeLink);
	}
}