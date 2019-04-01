using PopForums.Models;

namespace PopForums.Repositories
{
	public interface ISearchIndexQueueRepository
	{
		void Enqueue(SearchIndexPayload payload);
		SearchIndexPayload Dequeue();
	}
}