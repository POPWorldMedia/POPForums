using System.Web;
using PopForums.Models;

namespace PopForums.Web
{
	public interface IFormsAuthenticationWrapper
	{
		void SignOut();
		void SetAuthCookie(HttpContextBase context, User user, bool createPersistentCookie);
	}
}