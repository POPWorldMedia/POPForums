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
			_emailQueueRepository = serviceProvider.GetService<IEmailQueueRepository>();
			base.Start(serviceProvider);
		}

		private ISettingsManager _settingsManager;
		private ISmtpWrapper _smtpWrapper;
		private IQueuedEmailMessageRepository _queuedEmailRepository;
		private IEmailQueueRepository _emailQueueRepository;

		protected override void ServiceAction()
		{
#pragma warning disable 4014
			MailWorker.Instance.SendQueuedMessages(_settingsManager, _smtpWrapper, _queuedEmailRepository, _emailQueueRepository, ErrorLog);
#pragma warning restore 4014
		}

		protected override int GetInterval()
		{
			return _settingsManager.Current.MailSendingInverval;
		}
	}
}