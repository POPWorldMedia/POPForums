using PopForums.Models;

namespace PopForums.Web
{
	public class AdminAttribute : RoleRestrictionAttribute
	{
		protected override string RoleToRestrictTo
		{
			get { return PermanentRoles.Admin; }
		}
	}
}