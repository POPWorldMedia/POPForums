using System;
using System.Threading;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Repositories;

namespace PopForums.ScoringGame
{
	public class AwardCalculatorWorker
	{
		private AwardCalculatorWorker()
		{
			// only allow Instance to create a new instance
		}

		public async Task ProcessCalculation(IAwardCalculator calculator, IAwardCalculationQueueRepository awardCalculationQueueRepository, IErrorLog errorLog)
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

		private static AwardCalculatorWorker _instance;
		public static AwardCalculatorWorker Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new AwardCalculatorWorker();
				}
				return _instance;
			}
		}
	}
}