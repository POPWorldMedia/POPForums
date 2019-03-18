using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IQueuedEmailMessageRepository
	{
		int CreateMessage(QueuedEmailMessage message);
		void DeleteMessage(int messageID);
		QueuedEmailMessage GetMessage(int messageID);
	}
}
