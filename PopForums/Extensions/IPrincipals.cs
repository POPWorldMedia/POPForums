using System.Security.Principal;
using PopForums.Models;

namespace PopForums.Extensions
{
	public static class IPrincipals
	{
		public static bool IsPostEditable(this IPrincipal user, Post post)
		{
			if (user == null)
				return false;
			var castUser = user as User;
			if (castUser == null)
				return false;
			return user.IsInRole(PermanentRoles.Moderator) || castUser.UserID == post.UserID;
		}
	}
}
