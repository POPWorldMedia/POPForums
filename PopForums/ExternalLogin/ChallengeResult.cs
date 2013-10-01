using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

namespace PopForums.ExternalLogin
{
	public class ChallengeResult : HttpUnauthorizedResult
	{
		public ChallengeResult(string provider, string redirectUrl)
		{
			LoginProvider = provider;
			RedirectUrl = redirectUrl;
		}

		public string LoginProvider { get; set; }
		public string RedirectUrl { get; set; }

		public override void ExecuteResult(ControllerContext context)
		{
			context.HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties{ RedirectUrl = RedirectUrl }, LoginProvider);
		}
	}
}
