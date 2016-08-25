using System;
using Microsoft.Extensions.DependencyInjection;

namespace PopForums.Services
{
	public class UserSessionApplicationService : ApplicationServiceBase
	{
		public override void Start(IServiceProvider serviceProvider)
		{
			_userSessionService = serviceProvider.GetService<IUserSessionService>();
			base.Start(serviceProvider);
		}

		private IUserSessionService _userSessionService;

		protected override void ServiceAction()
		{
			UserSessionWorker.Instance.CleanUpExpiredSessions(_userSessionService, ErrorLog);
		}

		protected override int GetInterval()
		{
			return 10000;
		}
	}
}