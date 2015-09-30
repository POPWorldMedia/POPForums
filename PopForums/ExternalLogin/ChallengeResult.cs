//using System.Web;
//using System.Web.Mvc;
//using Microsoft.Owin.Security;

//namespace PopForums.ExternalLogin
//{
//	public class ChallengeResult : HttpUnauthorizedResult
//	{
//		public ChallengeResult(string provider, string redirectUri)
//		{
//			LoginProvider = provider;
//			RedirectUri = redirectUri;
//		}

//		public string LoginProvider { get; set; }
//		public string RedirectUri { get; set; }

//		public override void ExecuteResult(ControllerContext context)
//		{
//			context.RequestContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
//			context.HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties{ RedirectUri = RedirectUri }, LoginProvider);
//		}
//	}
//}
