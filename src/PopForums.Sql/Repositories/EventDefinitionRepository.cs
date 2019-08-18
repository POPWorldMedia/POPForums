using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		public async Task<EventDefinition> Get(string eventDefinitionID)
		{
			Task<EventDefinition> eventDef = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				eventDef = connection.QuerySingleAsync<EventDefinition>("SELECT EventDefinitionID, Description, PointValue, IsPublishedToFeed FROM pf_EventDefinition WHERE EventDefinitionID = @EventDefinitionID", new { EventDefinitionID = eventDefinitionID }));
			return await eventDef;
		}

		public async Task<List<EventDefinition>> GetAll()
		{
			Task<IEnumerable<EventDefinition>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<EventDefinition>("SELECT EventDefinitionID, Description, PointValue, IsPublishedToFeed FROM pf_EventDefinition ORDER BY EventDefinitionID"));
			return list.Result.ToList();
		}

		public async Task Create(EventDefinition eventDefinition)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_EventDefinition (EventDefinitionID, Description, PointValue, IsPublishedToFeed) VALUES (@EventDefinitionID, @Description, @PointValue, @IsPublishedToFeed)", new { eventDefinition.EventDefinitionID, Description = eventDefinition.Description.NullToEmpty(), eventDefinition.PointValue, eventDefinition.IsPublishedToFeed }));
		}

		public void Delete(string eventDefinitionID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_EventDefinition WHERE EventDefinitionID = @EventDefinitionID", new { EventDefinitionID = eventDefinitionID }));
		}
	}
}
