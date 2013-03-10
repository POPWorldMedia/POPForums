using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Services
{
	public interface ISearchService
	{
		List<string> GetJunkWords();
		void CreateJunkWord(string word);
		void DeleteJunkWord(string word);
		List<Topic> GetTopics(string searchTerm, SearchType searchType, User user, bool includeDeleted, int pageIndex, out PagerContext pagerContext);
		Topic GetNextTopicForIndexing();
		void MarkTopicAsIndexed(Topic topic);
		void DeleteAllIndexedWordsForTopic(Topic topic);
		void SaveSearchWord(SearchWord searchWord);
	}
}