using System;
using System.Threading;
using PopForums.Configuration;
using PopForums.Repositories;

namespace PopForums.ScoringGame
{
	public class AwardCalculatorWorker
	{
		private static readonly object _syncRoot = new Object();

		private AwardCalculatorWorker()
		{
			// only allow Instance to create a new instance
		}

		public void ProcessCalculation(IAwardCalculator calculator, IAwardCalculationQueueRepository awardCalculationQueueRepository, IErrorLog errorLog)
		{
			if (!Monitor.TryEnter(_syncRoot)) return;
			try
			{
				var nextItem = awardCalculationQueueRepository.Dequeue();
				if (string.IsNullOrEmpty(nextItem.Key))
					return;
				calculator.ProcessCalculation(nextItem.Key, nextItem.Value);
			}
			catch (Exception exc)
			{
				errorLog.Log(exc, ErrorSeverity.Error);
			}
			finally
			{
				Monitor.Exit(_syncRoot);
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