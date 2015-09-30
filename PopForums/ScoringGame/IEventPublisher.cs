using PopForums.Models;

namespace PopForums.ScoringGame
{
	public interface IEventPublisher
	{
		void ProcessEvent(string feedMessage, User user, string eventDefinitionID, bool overridePublishToActivityFeed);
		void ProcessManualEvent(string feedMessage, User user, int pointValue);
	}
}