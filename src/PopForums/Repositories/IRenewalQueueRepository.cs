namespace PopForums.Repositories;

public interface IRenewalQueueRepository
{
	Task Enqueue(RenewalQueuePayload payload);
	Task<RenewalQueuePayload> Dequeue();
}
