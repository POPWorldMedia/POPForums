using System.Collections.Generic;
using PopForums.Data.Sql;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Sql.Repositories
{
	public class EventDefinitionRepository : IEventDefinitionRepository
	{
		public EventDefinitionRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public EventDefinition Get(string eventDefinitionID)
		{
			EventDefinition eventDef = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "SELECT EventDefinitionID, Description, PointValue, IsPublishedToFeed FROM pf_EventDefinition WHERE EventDefinitionID = @EventDefinitionID")
				.AddParameter(_sqlObjectFactory, "@EventDefinitionID", eventDefinitionID)
				.ExecuteReader()
				.ReadOne(r => eventDef = new EventDefinition { EventDefinitionID = r.GetString(0), Description = r.GetString(1), PointValue = r.GetInt32(2), IsPublishedToFeed = r.GetBoolean(3) }));
			return eventDef;
		}

		public List<EventDefinition> GetAll()
		{
			var list = new List<EventDefinition>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "SELECT EventDefinitionID, Description, PointValue, IsPublishedToFeed FROM pf_EventDefinition ORDER BY EventDefinitionID")
				.ExecuteReader()
				.ReadAll(r => list.Add(new EventDefinition { EventDefinitionID = r.GetString(0), Description = r.GetString(1), PointValue = r.GetInt32(2), IsPublishedToFeed = r.GetBoolean(3) })));
			return list;
		}

		public void Create(EventDefinition eventDefinition)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "INSERT INTO pf_EventDefinition (EventDefinitionID, Description, PointValue, IsPublishedToFeed) VALUES (@EventDefinitionID, @Description, @PointValue, @IsPublishedToFeed)")
				.AddParameter(_sqlObjectFactory, "@EventDefinitionID", eventDefinition.EventDefinitionID)
				.AddParameter(_sqlObjectFactory, "@Description", eventDefinition.Description.NullToEmpty())
				.AddParameter(_sqlObjectFactory, "@PointValue", eventDefinition.PointValue)
				.AddParameter(_sqlObjectFactory, "@IsPublishedToFeed", eventDefinition.IsPublishedToFeed)
				.ExecuteNonQuery());
		}

		public void Delete(string eventDefinitionID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "DELETE FROM pf_EventDefinition WHERE EventDefinitionID = @EventDefinitionID")
				.AddParameter(_sqlObjectFactory, "@EventDefinitionID", eventDefinitionID)
				.ExecuteNonQuery());
		}
	}
}
