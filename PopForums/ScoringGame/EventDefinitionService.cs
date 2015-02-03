using System.Collections.Generic;
using System.Linq;
using PopForums.Repositories;

namespace PopForums.ScoringGame
{
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

		public EventDefinition GetEventDefinition(string eventDefinitionID)
		{
			if (StaticEvents.ContainsKey(eventDefinitionID))
				return StaticEvents[eventDefinitionID];
			return _eventDefinitionRepository.Get(eventDefinitionID);
		}

		public List<EventDefinition> GetAll()
		{
			var merged = StaticEvents.Select(x => x.Value).ToList();
			merged.AddRange(_eventDefinitionRepository.GetAll());
			return merged.OrderBy(x => x.EventDefinitionID).ToList();
		}

		public void Create(EventDefinition eventDefinition)
		{
			_eventDefinitionRepository.Create(eventDefinition);
		}

		public void Delete(string eventDefinitionID)
		{
			_awardConditionRepository.DeleteConditionsByEventDefinitionID(eventDefinitionID);
			_eventDefinitionRepository.Delete(eventDefinitionID);
		}
	}
}
