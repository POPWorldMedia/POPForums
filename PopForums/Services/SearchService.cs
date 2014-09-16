using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public class SearchService : ISearchService
	{
		public SearchService(ISearchRepository searchRepository, ISettingsManager settingsManager, IForumService forumService)
		{
			_searchRepository = searchRepository;
			_settingsManager = settingsManager;
			_forumService = forumService;
		}

		private readonly ISearchRepository _searchRepository;
		private readonly ISettingsManager _settingsManager;
		private readonly IForumService _forumService;

		public static Regex SearchWordPattern = new Regex(@"[\w'\@\#\$\%\^\&\*]{2,}", RegexOptions.Compiled);

		public List<Topic> GetTopics(string searchTerm, SearchType searchType, User user, bool includeDeleted, int pageIndex, out PagerContext pagerContext)
		{
			var nonViewableForumIDs = _forumService.GetNonViewableForumIDs(user);
			var pageSize = _settingsManager.Current.TopicsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var topicCount = 0;
			List<Topic> topics;
			if (String.IsNullOrEmpty(searchTerm))
				topics = new List<Topic>();
			else
				topics = _searchRepository.SearchTopics(searchTerm, nonViewableForumIDs, searchType, startRow, pageSize, out topicCount);
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(topicCount) / Convert.ToDouble(pageSize)));
			pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return topics;
		}

		public Topic GetNextTopicForIndexing()
		{
			return _searchRepository.GetNextTopicForIndexing();
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

		public void MarkTopicAsIndexed(Topic topic)
		{
			_searchRepository.MarkTopicAsIndexed(topic.TopicID);
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