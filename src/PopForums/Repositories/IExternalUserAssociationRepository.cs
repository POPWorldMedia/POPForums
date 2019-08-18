using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.ExternalLogin;

namespace PopForums.Repositories
{
	public interface IExternalUserAssociationRepository
	{
		Task<ExternalUserAssociation> Get(string issuer, string providerKey);
		Task<ExternalUserAssociation> Get(int externalUserAssociationID);
		Task<List<ExternalUserAssociation>> GetByUser(int userID);
		Task Save(int userID, string issuer, string providerKey, string name);
		Task Delete(int externalUserAssociationID);
	}
}