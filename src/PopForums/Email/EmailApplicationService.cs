//using PopForums.Configuration;
//using PopForums.Repositories;
//using StructureMap;

//namespace PopForums.Email
//{
//	public class EmailApplicationService : ApplicationServiceBase
//	{
//		public override void Start(IContainer container)
//		{
//			_settingsManager = container.GetInstance<ISettingsManager>();
//			_smtpWrapper = container.GetInstance<ISmtpWrapper>();
//			_queuedEmailRepository = container.GetInstance<IQueuedEmailMessageRepository>();
//			base.Start(container);
//		}

//		private ISettingsManager _settingsManager;
//		private ISmtpWrapper _smtpWrapper;
//		private IQueuedEmailMessageRepository _queuedEmailRepository;

//		protected override void ServiceAction()
//		{
//			MailWorker.Instance.SendQueuedMessages(_settingsManager, _smtpWrapper, _queuedEmailRepository, ErrorLog);
//		}

//		protected override int GetInterval()
//		{
//			return _settingsManager.Current.MailSendingInverval;
//		}
//	}
//}