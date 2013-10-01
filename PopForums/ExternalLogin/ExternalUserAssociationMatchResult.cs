using PopForums.Models;

namespace PopForums.ExternalLogin
{
	public class ExternalUserAssociationMatchResult
	{
		public bool Successful { get; set; }
		public ExternalUserAssociation ExternalUserAssociation { get; set; }
		public User User { get; set; }
	}
}