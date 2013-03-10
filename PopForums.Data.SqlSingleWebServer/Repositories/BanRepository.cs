using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer.Repositories
{
	public class BanRepository : IBanRepository
	{
		public BanRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public void BanIP(string ip)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("INSERT INTO pf_IPBan (IPBan) VALUES (@IPBan)")
					.AddParameter("@IPBan", ip)
					.ExecuteNonQuery());
		}

		public void RemoveIPBan(string ip)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_IPBan WHERE IPBan = @IPBan")
					.AddParameter("@IPBan", ip)
					.ExecuteNonQuery());
		}

		public List<string> GetIPBans()
		{
			var list = new List<string>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT IPBan FROM pf_IPBan ORDER BY IPBan")
					.ExecuteReader()
					.ReadAll(r => list.Add(r.GetString(0))));
			return list;
		}

		public bool IPIsBanned(string ip)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection =>
				result = connection.Command("SELECT * FROM pf_IPBan WHERE IPBan = @IPBan")
					.AddParameter("@IPBan", ip)
					.ExecuteReader()
					.Read());
			return result;
		}

		public void BanEmail(string email)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("INSERT INTO pf_EmailBan (EmailBan) VALUES (@EmailBan)")
					.AddParameter("@EmailBan", email)
					.ExecuteNonQuery());
		}

		public void RemoveEmailBan(string email)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_EmailBan WHERE EmailBan = @EmailBan")
					.AddParameter("@EmailBan", email)
					.ExecuteNonQuery());
		}

		public List<string> GetEmailBans()
		{
			var list = new List<string>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT EmailBan FROM pf_EmailBan ORDER BY EmailBan")
					.ExecuteReader()
					.ReadAll(r => list.Add(r.GetString(0))));
			return list;
		}

		public bool EmailIsBanned(string email)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection =>
				result = connection.Command("SELECT * FROM pf_EmailBan WHERE EmailBan = @EmailBan")
					.AddParameter("@EmailBan", email)
					.ExecuteReader()
					.Read());
			return result;
		}
	}
}
