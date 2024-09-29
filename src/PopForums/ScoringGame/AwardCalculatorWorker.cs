namespace PopForums.ScoringGame;

public interface IAwardCalculatorWorker
{
	void Execute();
}

public class AwardCalculatorWorker(IAwardCalculator calculator, IAwardCalculationQueueRepository awardCalculationQueueRepository, IErrorLog errorLog) : IAwardCalculatorWorker
{
	public async void Execute()
	{
		try
		{
			var nextItem = await awardCalculationQueueRepository.Dequeue();
			if (string.IsNullOrEmpty(nextItem.Key))
				return;
			await calculator.ProcessCalculation(nextItem.Key, nextItem.Value);
		}
		catch (Exception exc)
		{
			errorLog.Log(exc, ErrorSeverity.Error);
		}
	}
}