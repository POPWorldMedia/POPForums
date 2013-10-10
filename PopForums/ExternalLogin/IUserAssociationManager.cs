using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.ExternalLogin
{
	public interface IUserAssociationManager
	{
		ExternalUserAssociationMatchResult ExternalUserAssociationCheck(ExternalAuthenticationResult externalAuthenticationResult);
		void Associate(User user, ExternalAuthenticationResult externalAuthenticationResult);
		List<ExternalUserAssociation> GetExternalUserAssociations(User user);
		void RemoveAssociation(User user, int externalUserAssociationID);
	}
}