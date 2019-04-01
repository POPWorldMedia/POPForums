using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface ISearchRepository
	{
		List<string> GetJunkWords();
		void CreateJunkWord(string word);
		void DeleteJunkWord(string word);
		void DeleteAllIndexedWordsForTopic(int topicID);
		void SaveSearchWord(int topicID, string word, int rank);
		Response<List<Topic>> SearchTopics(string searchTerm, List<int> hiddenForums, SearchType searchType, int startRow, int pageSize, out int topicCount);
	}
}