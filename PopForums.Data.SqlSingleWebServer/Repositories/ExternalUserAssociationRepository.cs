using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.ExternalLogin;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer.Repositories
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
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command("SELECT ExternalUserAssociationID, UserID, Issuer, ProviderKey, Name FROM pf_ExternalUserAssociation WHERE Issuer = @Issuer AND ProviderKey = @ProviderKey")
					.AddParameter("@Issuer", issuer)
					.AddParameter("@ProviderKey", providerKey)
					.ExecuteReader()
					.ReadOne(r => externalUserAssociation = new ExternalUserAssociation
					                                        {
						                                        ExternalUserAssociationID = r.GetInt32(0),
																UserID = r.GetInt32(1),
																Issuer = r.GetString(2),
																ProviderKey = r.GetString(3),
																Name = r.GetString(4)
					                                        }));
			return externalUserAssociation;
		}

		public ExternalUserAssociation Get(int externalUserAssociationID)
		{
			ExternalUserAssociation externalUserAssociation = null;
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command("SELECT ExternalUserAssociationID, UserID, Issuer, ProviderKey, Name FROM pf_ExternalUserAssociation WHERE ExternalUserAssociationID = @ExternalUserAssociationID")
					.AddParameter("@ExternalUserAssociationID", externalUserAssociationID)
					.ExecuteReader()
					.ReadOne(r => externalUserAssociation = new ExternalUserAssociation
					{
						ExternalUserAssociationID = r.GetInt32(0),
						UserID = r.GetInt32(1),
						Issuer = r.GetString(2),
						ProviderKey = r.GetString(3),
						Name = r.GetString(4)
					}));
			return externalUserAssociation;
		}

		public List<ExternalUserAssociation> GetByUser(int userID)
		{
			var userAssociations = new List<ExternalUserAssociation>();
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command("SELECT ExternalUserAssociationID, UserID, Issuer, ProviderKey, Name FROM pf_ExternalUserAssociation WHERE UserID = @UserID")
					.AddParameter("@UserID", userID)
					.ExecuteReader()
					.ReadAll(r => userAssociations.Add(new ExternalUserAssociation
					{
						ExternalUserAssociationID = r.GetInt32(0),
						UserID = r.GetInt32(1),
						Issuer = r.GetString(2),
						ProviderKey = r.GetString(3),
						Name = r.GetString(4)
					})));
			return userAssociations;
		}

		public void Save(int userID, string issuer, string providerKey, string name)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command("INSERT INTO pf_ExternalUserAssociation (UserID, Issuer, ProviderKey, Name) VALUES (@UserID, @Issuer, @ProviderKey, @Name)")
				.AddParameter("@UserID", userID)
				.AddParameter("@Issuer", issuer)
				.AddParameter("@ProviderKey", providerKey)
				.AddParameter("@Name", name)
				.ExecuteNonQuery());
		}

		public void Delete(int externalUserAssociationID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command("DELETE FROM pf_ExternalUserAssociation WHERE ExternalUserAssociationID = @ExternalUserAssociationID")
				.AddParameter("@ExternalUserAssociationID", externalUserAssociationID)
				.ExecuteNonQuery());
		}
	}
}