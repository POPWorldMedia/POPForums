using System;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Configuration;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.ScoringGame
{
	public class AwardCalculatorApplicationService : ApplicationServiceBase
	{
		public override void Start(IServiceProvider container)
		{
			_settingsManager = container.GetService<ISettingsManager>();
			_awardCalculator = container.GetService<IAwardCalculator>();
			_awardCalcQueueRepo = container.GetService<IAwardCalculationQueueRepository>();
			base.Start(container);
		}

		private ISettingsManager _settingsManager;
		private IAwardCalculator _awardCalculator;
		private IAwardCalculationQueueRepository _awardCalcQueueRepo;

		protected override async void ServiceAction()
		{
			await AwardCalculatorWorker.Instance.ProcessCalculation(_awardCalculator, _awardCalcQueueRepo, ErrorLog);
		}

		protected override int GetInterval()
		{
			return _settingsManager.Current.ScoringGameCalculatorInterval;
		}
	}
}