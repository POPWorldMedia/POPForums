//using PopForums.Configuration;
//using StructureMap;

//namespace PopForums.ScoringGame
//{
//	public class AwardCalculatorApplicationService : ApplicationServiceBase
//	{
//		public override void Start(IContainer container)
//		{
//			_settingsManager = container.GetInstance<ISettingsManager>();
//			_awardCalculator = container.GetInstance<IAwardCalculator>();
//			base.Start(container);
//		}

//		private ISettingsManager _settingsManager;
//		private IAwardCalculator _awardCalculator;

//		protected override void ServiceAction()
//		{
//			AwardCalculatorWorker.Instance.ProcessCalculation(_awardCalculator, ErrorLog);
//		}

//		protected override int GetInterval()
//		{
//			return _settingsManager.Current.ScoringGameCalculatorInterval;
//		}
//	}
//}