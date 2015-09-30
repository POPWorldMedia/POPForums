using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IQueuedEmailMessageRepository
	{
		void CreateMessage(QueuedEmailMessage message);
		void DeleteMessage(int messageID);
		QueuedEmailMessage GetOldestQueuedEmailMessage();
	}
}
