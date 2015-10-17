using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using PopForums.Models;

namespace PopForums.Web.Areas.Forums.Services
{
	public interface IUserRetrievalShim
	{
		User GetUser(HttpContext context);
	}

	public class UserRetrievalShim : IUserRetrievalShim
	{
	    public User GetUser(HttpContext context)
	    {
			var user = context.Items["PopForumsUser"] as User;
			return user;
		}
    }
}
