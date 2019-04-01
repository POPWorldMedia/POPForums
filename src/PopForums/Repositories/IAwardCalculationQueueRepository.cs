using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IAwardCalculationQueueRepository
	{
		void Enqueue(AwardCalculationPayload payload);
		KeyValuePair<string, int> Dequeue();
	}
}
