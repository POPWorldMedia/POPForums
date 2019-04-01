using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface ISearchService
	{
		List<string> GetJunkWords();
		void CreateJunkWord(string word);
		void DeleteJunkWord(string word);
		Response<List<Topic>> GetTopics(string searchTerm, SearchType searchType, User user, bool includeDeleted, int pageIndex, out PagerContext pagerContext);
		int GetNextTopicIDForIndexing();
		void DeleteAllIndexedWordsForTopic(Topic topic);
		void SaveSearchWord(SearchWord searchWord);
	}

	public class SearchService : ISearchService
	{
		public SearchService(ISearchRepository searchRepository, ISettingsManager settingsManager, IForumService forumService, ISearchIndexQueueRepository searchIndexQueueRepository)
		{
			_searchRepository = searchRepository;
			_settingsManager = settingsManager;
			_forumService = forumService;
			_searchIndexQueueRepository = searchIndexQueueRepository;
		}

		private readonly ISearchRepository _searchRepository;
		private readonly ISettingsManager _settingsManager;
		private readonly IForumService _forumService;
		private readonly ISearchIndexQueueRepository _searchIndexQueueRepository;

		public static Regex SearchWordPattern = new Regex(@"[\w'\@\#\$\%\^\&\*]{2,}", RegexOptions.None);

		public Response<List<Topic>> GetTopics(string searchTerm, SearchType searchType, User user, bool includeDeleted, int pageIndex, out PagerContext pagerContext)
		{
			var nonViewableForumIDs = _forumService.GetNonViewableForumIDs(user);
			var pageSize = _settingsManager.Current.TopicsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var topicCount = 0;
			Response<List<Topic>> topics;
			if (String.IsNullOrEmpty(searchTerm))
				topics = new Response<List<Topic>>(new List<Topic>(), true);
			else
			{
				topics = _searchRepository.SearchTopics(searchTerm, nonViewableForumIDs, searchType, startRow, pageSize, out topicCount);
			}
			if (topics.IsValid)
			{
				var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(topicCount) / Convert.ToDouble(pageSize)));
				pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			}
			else
			{
				topics = new Response<List<Topic>>(new List<Topic>(), false);
				pagerContext = new PagerContext {PageCount = 1, PageIndex = 1, PageSize = 1};
			}
			return topics;
		}

		public int GetNextTopicIDForIndexing()
		{
			var payload = _searchIndexQueueRepository.Dequeue();
			return payload.TopicID;
		}

		public List<string> GetJunkWords()
		{
			return _searchRepository.GetJunkWords();
		}

		public void CreateJunkWord(string word)
		{
			_searchRepository.CreateJunkWord(word);
		}

		public void DeleteJunkWord(string word)
		{
			_searchRepository.DeleteJunkWord(word);
		}

		public void DeleteAllIndexedWordsForTopic(Topic topic)
		{
			_searchRepository.DeleteAllIndexedWordsForTopic(topic.TopicID);
		}

		public void SaveSearchWord(SearchWord searchWord)
		{
			_searchRepository.SaveSearchWord(searchWord.TopicID, searchWord.Word, searchWord.Rank);
		}
	}
}