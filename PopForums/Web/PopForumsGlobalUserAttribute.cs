using PopForums.Services;

namespace PopForums.Web
{
	public class PopForumsGlobalUserAttribute : PopForumsUserAttribute
	{
		public PopForumsGlobalUserAttribute(IUserService userService, IUserSessionService userSessionService) : base(userService, userSessionService)
		{
		}

		protected override bool IsGlobalFilter()
		{
			return true;
		}
	}
}