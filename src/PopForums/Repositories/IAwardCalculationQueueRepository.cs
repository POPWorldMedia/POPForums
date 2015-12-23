using System.Collections.Generic;

namespace PopForums.Repositories
{
	public interface IAwardCalculationQueueRepository
	{
		void Enqueue(string eventDefinitionID, int userID);
		KeyValuePair<string, int> Dequeue();
	}
}
