using System.Collections.Generic;
using PopForums.ExternalLogin;
using PopForums.Repositories;

namespace PopForums.Data.Sql.Repositories
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
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "SELECT ExternalUserAssociationID, UserID, Issuer, ProviderKey, Name FROM pf_ExternalUserAssociation WHERE Issuer = @Issuer AND ProviderKey = @ProviderKey")
					.AddParameter(_sqlObjectFactory, "@Issuer", issuer)
					.AddParameter(_sqlObjectFactory, "@ProviderKey", providerKey)
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
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "SELECT ExternalUserAssociationID, UserID, Issuer, ProviderKey, Name FROM pf_ExternalUserAssociation WHERE ExternalUserAssociationID = @ExternalUserAssociationID")
					.AddParameter(_sqlObjectFactory, "@ExternalUserAssociationID", externalUserAssociationID)
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
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "SELECT ExternalUserAssociationID, UserID, Issuer, ProviderKey, Name FROM pf_ExternalUserAssociation WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@UserID", userID)
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
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "INSERT INTO pf_ExternalUserAssociation (UserID, Issuer, ProviderKey, Name) VALUES (@UserID, @Issuer, @ProviderKey, @Name)")
				.AddParameter(_sqlObjectFactory, "@UserID", userID)
				.AddParameter(_sqlObjectFactory, "@Issuer", issuer)
				.AddParameter(_sqlObjectFactory, "@ProviderKey", providerKey)
				.AddParameter(_sqlObjectFactory, "@Name", name)
				.ExecuteNonQuery());
		}

		public void Delete(int externalUserAssociationID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "DELETE FROM pf_ExternalUserAssociation WHERE ExternalUserAssociationID = @ExternalUserAssociationID")
				.AddParameter(_sqlObjectFactory, "@ExternalUserAssociationID", externalUserAssociationID)
				.ExecuteNonQuery());
		}
	}
}