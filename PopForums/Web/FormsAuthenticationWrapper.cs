using System;
using System.Web;
using System.Web.Security;
using PopForums.Models;

namespace PopForums.Web
{
	public class FormsAuthenticationWrapper : IFormsAuthenticationWrapper
	{
		public void SignOut()
		{
			FormsAuthentication.SignOut();
		}

		public void SetAuthCookie(HttpContextBase context, User user, bool createPersistentCookie)
		{
			var ticket = new FormsAuthenticationTicket(9, user.Name, DateTime.Now, DateTime.Now.AddDays(30), createPersistentCookie, "");
			var encryptedTicket = FormsAuthentication.Encrypt(ticket);
			var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
			if (createPersistentCookie)
				cookie.Expires = DateTime.Now.AddDays(30);
			context.Response.Cookies.Add(cookie);
		}
	}
}