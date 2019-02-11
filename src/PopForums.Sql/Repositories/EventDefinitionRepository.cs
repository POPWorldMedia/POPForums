using System.Collections.Generic;
using System.Linq;
using Dapper;
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
				eventDef = connection.QuerySingle<EventDefinition>("SELECT EventDefinitionID, Description, PointValue, IsPublishedToFeed FROM pf_EventDefinition WHERE EventDefinitionID = @EventDefinitionID", new { EventDefinitionID = eventDefinitionID }));
			return eventDef;
		}

		public List<EventDefinition> GetAll()
		{
			var list = new List<EventDefinition>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<EventDefinition>("SELECT EventDefinitionID, Description, PointValue, IsPublishedToFeed FROM pf_EventDefinition ORDER BY EventDefinitionID").ToList());
			return list;
		}

		public void Create(EventDefinition eventDefinition)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_EventDefinition (EventDefinitionID, Description, PointValue, IsPublishedToFeed) VALUES (@EventDefinitionID, @Description, @PointValue, @IsPublishedToFeed)", new { eventDefinition.EventDefinitionID, Description = eventDefinition.Description.NullToEmpty(), eventDefinition.PointValue, eventDefinition.IsPublishedToFeed }));
		}

		public void Delete(string eventDefinitionID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_EventDefinition WHERE EventDefinitionID = @EventDefinitionID", new { EventDefinitionID = eventDefinitionID }));
		}
	}
}
