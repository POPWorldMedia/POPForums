using System.Collections.Generic;
using System.Linq;
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

		public ExternalUserAssociation Get(string issuer, string providerKey)
		{
			ExternalUserAssociation externalUserAssociation = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				externalUserAssociation = connection.QuerySingleOrDefault<ExternalUserAssociation>("SELECT ExternalUserAssociationID, UserID, Issuer, ProviderKey, Name FROM pf_ExternalUserAssociation WHERE Issuer = @Issuer AND ProviderKey = @ProviderKey", new {  Issuer = issuer, ProviderKey = providerKey }));
			return externalUserAssociation;
		}

		public ExternalUserAssociation Get(int externalUserAssociationID)
		{
			ExternalUserAssociation externalUserAssociation = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				externalUserAssociation = connection.QuerySingleOrDefault<ExternalUserAssociation>("SELECT ExternalUserAssociationID, UserID, Issuer, ProviderKey, Name FROM pf_ExternalUserAssociation WHERE ExternalUserAssociationID = @ExternalUserAssociationID", new { ExternalUserAssociationID = externalUserAssociationID }));
			return externalUserAssociation;
		}

		public List<ExternalUserAssociation> GetByUser(int userID)
		{
			var userAssociations = new List<ExternalUserAssociation>();
			_sqlObjectFactory.GetConnection().Using(connection => 
				userAssociations = connection.Query<ExternalUserAssociation>("SELECT ExternalUserAssociationID, UserID, Issuer, ProviderKey, Name FROM pf_ExternalUserAssociation WHERE UserID = @UserID", new { UserID = userID }).ToList());
			return userAssociations;
		}

		public void Save(int userID, string issuer, string providerKey, string name)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("INSERT INTO pf_ExternalUserAssociation (UserID, Issuer, ProviderKey, Name) VALUES (@UserID, @Issuer, @ProviderKey, @Name)", new { UserID = userID, Issuer = issuer, ProviderKey = providerKey, Name = name }));
		}

		public void Delete(int externalUserAssociationID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("DELETE FROM pf_ExternalUserAssociation WHERE ExternalUserAssociationID = @ExternalUserAssociationID", new { ExternalUserAssociationID = externalUserAssociationID }));
		}
	}
}