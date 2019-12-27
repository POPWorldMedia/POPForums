using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface ISearchService
	{
		Task<List<string>> GetJunkWords();
		Task CreateJunkWord(string word);
		Task DeleteJunkWord(string word);
		Task<Tuple<Response<List<Topic>>, PagerContext>> GetTopics(string searchTerm, SearchType searchType, User user, bool includeDeleted, int pageIndex);
		Task<SearchIndexPayload> GetNextTopicForIndexing();
		Task DeleteAllIndexedWordsForTopic(int topicID);
		Task SaveSearchWord(SearchWord searchWord);
	}

	public class SearchService : ISearchService
	{
		public SearchService(ISearchRepository searchRepository, ISettingsManager settingsManager, IForumService forumService, ISearchIndexQueueRepository searchIndexQueueRepository, IErrorLog errorLog)
		{
			_searchRepository = searchRepository;
			_settingsManager = settingsManager;
			_forumService = forumService;
			_searchIndexQueueRepository = searchIndexQueueRepository;
			_errorLog = errorLog;
		}

		private readonly ISearchRepository _searchRepository;
		private readonly ISettingsManager _settingsManager;
		private readonly IForumService _forumService;
		private readonly ISearchIndexQueueRepository _searchIndexQueueRepository;
		private readonly IErrorLog _errorLog;

		public static Regex SearchWordPattern = new Regex(@"[\w'\@\#\$\%\^\&\*]{2,}", RegexOptions.None);

		public async Task<Tuple<Response<List<Topic>>, PagerContext>> GetTopics(string searchTerm, SearchType searchType, User user, bool includeDeleted, int pageIndex)
		{
			var nonViewableForumIDs = await _forumService.GetNonViewableForumIDs(user);
			var pageSize = _settingsManager.Current.TopicsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var topicCount = 0;
			Response<List<Topic>> topics;
			PagerContext pagerContext;
			if (string.IsNullOrEmpty(searchTerm))
				topics = new Response<List<Topic>>(new List<Topic>(), true);
			else
			{
				(topics, topicCount) = await _searchRepository.SearchTopics(searchTerm, nonViewableForumIDs, searchType, startRow, pageSize);
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
				var exc = new Exception($"Search service error: {topics.Exception?.Message}");
				_errorLog.Log(exc, ErrorSeverity.Warning, topics.DebugInfo);
			}
			return Tuple.Create(topics, pagerContext);
		}

		public async Task<SearchIndexPayload> GetNextTopicForIndexing()
		{
			var payload = await _searchIndexQueueRepository.Dequeue();
			return payload;
		}

		public async Task<List<string>> GetJunkWords()
		{
			return await _searchRepository.GetJunkWords();
		}

		public async Task CreateJunkWord(string word)
		{
			await _searchRepository.CreateJunkWord(word);
		}

		public async Task DeleteJunkWord(string word)
		{
			await _searchRepository.DeleteJunkWord(word);
		}

		public async Task DeleteAllIndexedWordsForTopic(int topicID)
		{
			await _searchRepository.DeleteAllIndexedWordsForTopic(topicID);
		}

		public async Task SaveSearchWord(SearchWord searchWord)
		{
			await _searchRepository.SaveSearchWord(searchWord.TopicID, searchWord.Word, searchWord.Rank);
		}
	}
}