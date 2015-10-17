using Microsoft.AspNet.Http;
using PopForums.Models;

namespace PopForums.Web.Areas.Forums.Services
{
	public interface IUserRetrievalShim
	{
		User GetUser(HttpContext context);
		Profile GetProfile(HttpContext context);
	}

	public class UserRetrievalShim : IUserRetrievalShim
	{
	    public User GetUser(HttpContext context)
	    {
			var user = context.Items["PopForumsUser"] as User;
			return user;
		}

		public Profile GetProfile(HttpContext context)
		{
			var profile = context.Items["PopForumsProfile"] as Profile;
			return profile;
		}
    }
}
