using PopForums.Models;

namespace PopForums.ExternalLogin
{
	public interface IUserAssociationManager
	{
		ExternalUserAssociationMatchResult ExternalUserAssociationCheck(ExternalAuthenticationResult externalAuthenticationResult);
		void Associate(User user, ExternalAuthenticationResult externalAuthenticationResult);
	}
}