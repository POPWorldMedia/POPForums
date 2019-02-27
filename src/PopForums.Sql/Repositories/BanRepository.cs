using System.Collections.Generic;
using System.Linq;
using Dapper;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
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
				connection.Execute("INSERT INTO pf_IPBan (IPBan) VALUES (@IPBan)", new { IPBan = ip }));
		}

		public void RemoveIPBan(string ip)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_IPBan WHERE IPBan = @IPBan", new { IPBan = ip }));
		}

		public List<string> GetIPBans()
		{
			var list = new List<string>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<string>("SELECT IPBan FROM pf_IPBan ORDER BY IPBan").ToList());
			return list;
		}

		public bool IPIsBanned(string ip)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection =>
				result = connection.Query<string>("SELECT * FROM pf_IPBan WHERE CHARINDEX(pf_IPBan.IPBan, @IPBan) > 0", new { IPBan = ip }).Any());
			return result;
		}

		public void BanEmail(string email)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_EmailBan (EmailBan) VALUES (@EmailBan)", new { EmailBan = email }));
		}

		public void RemoveEmailBan(string email)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_EmailBan WHERE EmailBan = @EmailBan", new { EmailBan = email }));
		}

		public List<string> GetEmailBans()
		{
			var list = new List<string>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<string>("SELECT EmailBan FROM pf_EmailBan ORDER BY EmailBan").ToList());
			return list;
		}

		public bool EmailIsBanned(string email)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection =>
				result = connection.Query<string>("SELECT * FROM pf_EmailBan WHERE EmailBan = @EmailBan", new { EmailBan = email }).Any());
			return result;
		}
	}
}
