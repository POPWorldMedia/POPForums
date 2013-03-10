using Ninject;
using PopForums.Services;

namespace PopForums.Configuration
{
	public class UserSessionApplicationService : ApplicationServiceBase
	{
		public override void Start(IKernel kernel)
		{
			_userSessionService = kernel.Get<IUserSessionService>();
			base.Start(kernel);
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