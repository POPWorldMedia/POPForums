using System;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.ScoringGame
{
	public class AwardCalculatorApplicationService : ApplicationServiceBase
	{
		public override void Start(IServiceProvider container)
		{
			_settingsManager = container.GetService<ISettingsManager>();
			_awardCalculator = container.GetService<IAwardCalculator>();
			base.Start(container);
		}

		private ISettingsManager _settingsManager;
		private IAwardCalculator _awardCalculator;

		protected override void ServiceAction()
		{
			AwardCalculatorWorker.Instance.ProcessCalculation(_awardCalculator, ErrorLog);
		}

		protected override int GetInterval()
		{
			return _settingsManager.Current.ScoringGameCalculatorInterval;
		}
	}
}