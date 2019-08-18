using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PopForums.ExternalLogin;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class ExternalUserAssociationRepository : IExternalUserAssociationRepository
	{
		public ExternalUserAssociationRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public async Task<ExternalUserAssociation> Get(string issuer, string providerKey)
		{
			Task<ExternalUserAssociation> externalUserAssociation = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				externalUserAssociation = connection.QuerySingleOrDefaultAsync<ExternalUserAssociation>("SELECT ExternalUserAssociationID, UserID, Issuer, ProviderKey, Name FROM pf_ExternalUserAssociation WHERE Issuer = @Issuer AND ProviderKey = @ProviderKey", new {  Issuer = issuer, ProviderKey = providerKey }));
			return await externalUserAssociation;
		}

		public async Task<ExternalUserAssociation> Get(int externalUserAssociationID)
		{
			Task<ExternalUserAssociation> externalUserAssociation = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				externalUserAssociation = connection.QuerySingleOrDefaultAsync<ExternalUserAssociation>("SELECT ExternalUserAssociationID, UserID, Issuer, ProviderKey, Name FROM pf_ExternalUserAssociation WHERE ExternalUserAssociationID = @ExternalUserAssociationID", new { ExternalUserAssociationID = externalUserAssociationID }));
			return await externalUserAssociation;
		}

		public async Task<List<ExternalUserAssociation>> GetByUser(int userID)
		{
			Task<IEnumerable<ExternalUserAssociation>> userAssociations = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				userAssociations = connection.QueryAsync<ExternalUserAssociation>("SELECT ExternalUserAssociationID, UserID, Issuer, ProviderKey, Name FROM pf_ExternalUserAssociation WHERE UserID = @UserID", new { UserID = userID }));
			return userAssociations.Result.ToList();
		}

		public async Task Save(int userID, string issuer, string providerKey, string name)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("INSERT INTO pf_ExternalUserAssociation (UserID, Issuer, ProviderKey, Name) VALUES (@UserID, @Issuer, @ProviderKey, @Name)", new { UserID = userID, Issuer = issuer, ProviderKey = providerKey, Name = name }));
		}

		public async Task Delete(int externalUserAssociationID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("DELETE FROM pf_ExternalUserAssociation WHERE ExternalUserAssociationID = @ExternalUserAssociationID", new { ExternalUserAssociationID = externalUserAssociationID }));
		}
	}
}