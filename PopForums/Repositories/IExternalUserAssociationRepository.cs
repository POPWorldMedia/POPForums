using System.Collections.Generic;
using PopForums.ExternalLogin;

namespace PopForums.Repositories
{
	public interface IExternalUserAssociationRepository
	{
		ExternalUserAssociation Get(string issuer, string providerKey);
		ExternalUserAssociation Get(int externalUserAssociatinoID);
		List<ExternalUserAssociation> GetByUser(int userID);
		void Save(int userID, string issuer, string providerKey, string name);
		void Delete(int externalUserAssociationID);
	}
}