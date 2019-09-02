using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface ISearchRepository
	{
		Task<List<string>> GetJunkWords();
		Task CreateJunkWord(string word);
		Task DeleteJunkWord(string word);
		Task DeleteAllIndexedWordsForTopic(int topicID);
		Task SaveSearchWord(int topicID, string word, int rank);
		Task<Tuple<Response<List<Topic>>, int>> SearchTopics(string searchTerm, List<int> hiddenForums, SearchType searchType, int startRow, int pageSize);
	}
}