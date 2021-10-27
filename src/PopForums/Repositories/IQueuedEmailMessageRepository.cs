namespace PopForums.Repositories;

public interface IQueuedEmailMessageRepository
{
	Task<int> CreateMessage(QueuedEmailMessage message);
	Task DeleteMessage(int messageID);
	Task<QueuedEmailMessage> GetMessage(int messageID);
}