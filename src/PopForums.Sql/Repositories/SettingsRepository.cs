using System;
using System.Collections.Generic;
using PopForums.Data.Sql;
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
				connection.Command(_sqlObjectFactory, "SELECT Setting, [Value] FROM pf_Setting")
					.ExecuteReader()
					.ReadAll(r => dictionary.Add(r.GetString(0), r.GetString(1))));
			return dictionary;
		}

		public void Save(Dictionary<string, object> dictionary)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				{
					connection.Command(_sqlObjectFactory, "DELETE FROM pf_Setting")
						.ExecuteNonQuery();
					foreach (var key in dictionary)
						connection.Command(_sqlObjectFactory, "INSERT INTO pf_Setting (Setting, [Value]) VALUES (@Setting, @Value)")
							.AddParameter(_sqlObjectFactory, "@Setting", key.Key)
							.AddParameter(_sqlObjectFactory, "@Value", key.Value == null ? String.Empty : key.Value.ToString())
							.ExecuteNonQuery();
				});
		}

		public bool IsStale(DateTime lastLoad)
		{
			return false;
		}
	}
}
