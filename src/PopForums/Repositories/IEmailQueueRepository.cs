using PopForums.Email;

namespace PopForums.Repositories
{
	public interface IEmailQueueRepository
	{
		void Enqueue(EmailQueuePayload payload);
		EmailQueuePayload Dequeue();
	}
}