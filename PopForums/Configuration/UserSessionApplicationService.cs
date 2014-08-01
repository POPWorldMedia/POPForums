using PopForums.Services;
using StructureMap;

namespace PopForums.Configuration
{
	public class UserSessionApplicationService : ApplicationServiceBase
	{
		public override void Start(IContainer container)
		{
			_userSessionService = container.GetInstance<IUserSessionService>();
			base.Start(container);
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