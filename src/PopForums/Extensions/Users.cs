using PopForums.Models;

namespace PopForums.Extensions
{
    public static class Users
	{
		public static bool IsPostEditable(this User user, Post post)
		{
			if (user == null)
				return false;
			return user.IsInRole(PermanentRoles.Moderator) || user.UserID == post.UserID;
		}
	}
}
