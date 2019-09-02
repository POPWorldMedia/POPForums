using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface ISearchIndexQueueRepository
	{
		Task Enqueue(SearchIndexPayload payload);
		Task<SearchIndexPayload> Dequeue();
	}
}