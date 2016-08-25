using System;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Configuration;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Email
{
	public class EmailApplicationService : ApplicationServiceBase
	{
		public override void Start(IServiceProvider serviceProvider)
		{
			_settingsManager = serviceProvider.GetService<ISettingsManager>();
			_smtpWrapper = serviceProvider.GetService<ISmtpWrapper>();
			_queuedEmailRepository = serviceProvider.GetService<IQueuedEmailMessageRepository>();
			base.Start(serviceProvider);
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