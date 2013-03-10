using Ninject;
using PopForums.Configuration;
using PopForums.Repositories;

namespace PopForums.Email
{
	public class EmailApplicationService : ApplicationServiceBase
	{
		public override void Start(IKernel kernel)
		{
			_settingsManager = kernel.Get<ISettingsManager>();
			_smtpWrapper = kernel.Get<ISmtpWrapper>();
			_queuedEmailRepository = kernel.Get<IQueuedEmailMessageRepository>();
			base.Start(kernel);
		}

		private ISettingsManager _settingsManager;
		private ISmtpWrapper _smtpWrapper;
		private IQueuedEmailMessageRepository _queuedEmailRepository;

		protected override void ServiceAction()
		{
			MailWorker.Instance.SendQueuedMessages(_settingsManager, _smtpWrapper, _queuedEmailRepository, ErrorLog);
		}

		protected override int GetInterval()
		{
			return _settingsManager.Current.MailSendingInverval;
		}
	}
}