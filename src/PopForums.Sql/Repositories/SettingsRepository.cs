using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class SettingsRepository : ISettingsRepository
	{
		public SettingsRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public Dictionary<string, string> Get()
		{
			var dictionary = new Dictionary<string, string>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				dictionary = connection.Query("SELECT Setting, [Value] FROM pf_Setting").ToDictionary(r => (string)r.Setting, r => (string)r.Value));
			return dictionary;
		}

		public void Save(Dictionary<string, object> dictionary)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
			{
				connection.Execute("DELETE FROM pf_Setting");
				foreach (var key in dictionary)
					connection.Execute("INSERT INTO pf_Setting (Setting, [Value]) VALUES (@Setting, @Value)", new { Setting = key.Key, Value = key.Value == null ? string.Empty : key.Value.ToString()});
				});
		}

		public bool IsStale(DateTime lastLoad)
		{
			return false;
		}
	}
}
