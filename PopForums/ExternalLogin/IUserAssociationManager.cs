using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.ExternalLogin
{
	public interface IUserAssociationManager
	{
		ExternalUserAssociationMatchResult ExternalUserAssociationCheck(ExternalAuthenticationResult externalAuthenticationResult, string ip);
		void Associate(User user, ExternalAuthenticationResult externalAuthenticationResult, string ip);
		List<ExternalUserAssociation> GetExternalUserAssociations(User user);
		void RemoveAssociation(User user, int externalUserAssociationID, string ip);
	}
}