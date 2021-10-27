namespace PopForums.Repositories;

public interface IAwardCalculationQueueRepository
{
	Task Enqueue(AwardCalculationPayload payload);
	Task<KeyValuePair<string, int>> Dequeue();
}