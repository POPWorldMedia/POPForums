using System.Web.Mvc;

namespace PopForums.Web
{
	public abstract class RoleRestrictionAttribute : AuthorizeAttribute
	{
		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
		{
			var result = new ViewResult { ViewName = "Forbidden" };
			filterContext.Result = result;
		}

		protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
		{
			return httpContext.User != null && httpContext.User.IsInRole(RoleToRestrictTo);
		}

		protected abstract string RoleToRestrictTo { get; }
	}
}
