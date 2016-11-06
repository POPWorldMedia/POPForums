using System.Collections.Generic;
using PopForums.Repositories;

namespace PopForums.Data.Sql.Repositories
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
				connection.Command(_sqlObjectFactory, "INSERT INTO pf_IPBan (IPBan) VALUES (@IPBan)")
					.AddParameter(_sqlObjectFactory, "@IPBan", ip)
					.ExecuteNonQuery());
		}

		public void RemoveIPBan(string ip)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_IPBan WHERE IPBan = @IPBan")
					.AddParameter(_sqlObjectFactory, "@IPBan", ip)
					.ExecuteNonQuery());
		}

		public List<string> GetIPBans()
		{
			var list = new List<string>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "SELECT IPBan FROM pf_IPBan ORDER BY IPBan")
					.ExecuteReader()
					.ReadAll(r => list.Add(r.GetString(0))));
			return list;
		}

		public bool IPIsBanned(string ip)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection =>
				result = connection.Command(_sqlObjectFactory, "SELECT * FROM pf_IPBan WHERE CHARINDEX(pf_IPBan.IPBan, @IPBan) > 0")
					.AddParameter(_sqlObjectFactory, "@IPBan", ip)
					.ExecuteReader()
					.Read());
			return result;
		}

		public void BanEmail(string email)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "INSERT INTO pf_EmailBan (EmailBan) VALUES (@EmailBan)")
					.AddParameter(_sqlObjectFactory, "@EmailBan", email)
					.ExecuteNonQuery());
		}

		public void RemoveEmailBan(string email)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_EmailBan WHERE EmailBan = @EmailBan")
					.AddParameter(_sqlObjectFactory, "@EmailBan", email)
					.ExecuteNonQuery());
		}

		public List<string> GetEmailBans()
		{
			var list = new List<string>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "SELECT EmailBan FROM pf_EmailBan ORDER BY EmailBan")
					.ExecuteReader()
					.ReadAll(r => list.Add(r.GetString(0))));
			return list;
		}

		public bool EmailIsBanned(string email)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection =>
				result = connection.Command(_sqlObjectFactory, "SELECT * FROM pf_EmailBan WHERE EmailBan = @EmailBan")
					.AddParameter(_sqlObjectFactory, "@EmailBan", email)
					.ExecuteReader()
					.Read());
			return result;
		}
	}
}
