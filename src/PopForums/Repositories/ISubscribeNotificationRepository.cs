namespace PopForums.Repositories;

public interface ISubscribeNotificationRepository
{
	Task Enqueue(SubscribeNotificationPayload payload);
	Task<SubscribeNotificationPayload> Dequeue();
}