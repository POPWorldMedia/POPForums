using Ninject;
using PopForums.Configuration;

namespace PopForums.ScoringGame
{
	public class AwardCalculatorApplicationService : ApplicationServiceBase
	{
		public override void Start(IKernel kernel)
		{
			_settingsManager = kernel.Get<ISettingsManager>();
			_awardCalculator = kernel.Get<IAwardCalculator>();
			base.Start(kernel);
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