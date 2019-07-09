using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IAwardCalculationQueueRepository
	{
		Task Enqueue(AwardCalculationPayload payload);
		Task<KeyValuePair<string, int>> Dequeue();
	}
}
