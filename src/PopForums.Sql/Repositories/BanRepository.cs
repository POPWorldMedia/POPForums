using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		public async Task BanIP(string ip)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_IPBan (IPBan) VALUES (@IPBan)", new { IPBan = ip }));
		}

		public async Task RemoveIPBan(string ip)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_IPBan WHERE IPBan = @IPBan", new { IPBan = ip }));
		}

		public async Task<List<string>> GetIPBans()
		{
			Task<IEnumerable<string>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QueryAsync<string>("SELECT IPBan FROM pf_IPBan ORDER BY IPBan"));
			var list = result.Result.ToList();
			return list;
		}

		public async Task<bool> IPIsBanned(string ip)
		{
			Task<IEnumerable<string>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QueryAsync<string>("SELECT * FROM pf_IPBan WHERE CHARINDEX(pf_IPBan.IPBan, @IPBan) > 0", new { IPBan = ip }));
			var isBanned = result.Result.Any();
			return isBanned;
		}

		public async Task BanEmail(string email)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_EmailBan (EmailBan) VALUES (@EmailBan)", new { EmailBan = email }));
		}

		public async Task RemoveEmailBan(string email)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_EmailBan WHERE EmailBan = @EmailBan", new { EmailBan = email }));
		}

		public async Task<List<string>> GetEmailBans()
		{
			Task<IEnumerable<string>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QueryAsync<string>("SELECT EmailBan FROM pf_EmailBan ORDER BY EmailBan"));
			var list = result.Result.ToList();
			return list;
		}

		public async Task<bool> EmailIsBanned(string email)
		{
			Task<IEnumerable<string>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QueryAsync<string>("SELECT * FROM pf_EmailBan WHERE EmailBan = @EmailBan", new { EmailBan = email }));
			var isBanned = result.Result.Any();
			return isBanned;
		}
	}
}
