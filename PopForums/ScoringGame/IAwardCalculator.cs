using PopForums.Models;

namespace PopForums.ScoringGame
{
	public interface IAwardCalculator
	{
		void QueueCalculation(User user, EventDefinition eventDefinition);
		void ProcessOneCalculation();
	}
}
