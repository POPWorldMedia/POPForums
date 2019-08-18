using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PopForums.Repositories;

namespace PopForums.ScoringGame
{
	public interface IEventDefinitionService
	{
		Task<EventDefinition> GetEventDefinition(string eventDefinitionID);
		Task<List<EventDefinition>> GetAll();
		Task Create(EventDefinition eventDefinition);
		Task Delete(string eventDefinitionID);
	}

	public class EventDefinitionService : IEventDefinitionService
	{
		public static Dictionary<string, EventDefinition> StaticEvents = new Dictionary<string, EventDefinition>
		{
		    {StaticEventIDs.PostVote, new EventDefinition {EventDefinitionID = StaticEventIDs.PostVote, Description = "Post vote", PointValue = 1, IsPublishedToFeed = true}},
		    {StaticEventIDs.NewPost, new EventDefinition {EventDefinitionID = StaticEventIDs.NewPost, Description = "New post", PointValue = 0, IsPublishedToFeed = true}},
		    {StaticEventIDs.NewTopic, new EventDefinition {EventDefinitionID = StaticEventIDs.NewTopic, Description = "New topic", PointValue = 0, IsPublishedToFeed = true}},
		    {StaticEventIDs.QuestionAnswered, new EventDefinition {EventDefinitionID = StaticEventIDs.QuestionAnswered, Description = "Question answered", PointValue = 10, IsPublishedToFeed = true}}
		};

		public static class StaticEventIDs
		{
			public static string PostVote = "PostVote";
			public static string NewPost = "NewPost";
			public static string NewTopic = "NewTopic";
			public static string QuestionAnswered = "QuestionAnswered";
		}

		public EventDefinitionService(IEventDefinitionRepository eventDefinitionRepository, IAwardConditionRepository awardConditionRepository)
		{
			_eventDefinitionRepository = eventDefinitionRepository;
			_awardConditionRepository = awardConditionRepository;
		}

		private readonly IEventDefinitionRepository _eventDefinitionRepository;
		private readonly IAwardConditionRepository _awardConditionRepository;

		public async Task<EventDefinition> GetEventDefinition(string eventDefinitionID)
		{
			if (StaticEvents.ContainsKey(eventDefinitionID))
				return StaticEvents[eventDefinitionID];
			return await _eventDefinitionRepository.Get(eventDefinitionID);
		}

		public async Task<List<EventDefinition>> GetAll()
		{
			var merged = StaticEvents.Select(x => x.Value).ToList();
			merged.AddRange(await _eventDefinitionRepository.GetAll());
			return merged.OrderBy(x => x.EventDefinitionID).ToList();
		}

		public async Task Create(EventDefinition eventDefinition)
		{
			await _eventDefinitionRepository.Create(eventDefinition);
		}

		public async Task Delete(string eventDefinitionID)
		{
			await _awardConditionRepository.DeleteConditionsByEventDefinitionID(eventDefinitionID);
			_eventDefinitionRepository.Delete(eventDefinitionID);
		}
	}
}
