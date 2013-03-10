using PopForums.Models;

namespace PopForums.Web
{
	public class ModeratorAttribute : RoleRestrictionAttribute
	{
		protected override string RoleToRestrictTo
		{
			get { return PermanentRoles.Moderator; }
		}
	}
}
