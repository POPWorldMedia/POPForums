using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Sql;
#pragma warning disable 1998

namespace PopForums.AwsKit.Search
{
	public class SearchRepository : Sql.Repositories.SearchRepository
	{
		private readonly ITopicRepository _topicRepository;
		private readonly IElasticSearchClientWrapper _elasticSearchClientWrapper;

		public SearchRepository(ISqlObjectFactory sqlObjectFactory, ITopicRepository topicRepository, IElasticSearchClientWrapper elasticSearchClientWrapper) : base(sqlObjectFactory)
		{
			_topicRepository = topicRepository;
			_elasticSearchClientWrapper = elasticSearchClientWrapper;
		}

		public override async Task<List<string>> GetJunkWords()
		{
			return new List<string>();
		}

		public override async Task CreateJunkWord(string word)
		{
			throw new NotImplementedException();
		}

		public override async Task DeleteJunkWord(string word)
		{
			throw new NotImplementedException();
		}

		public override async Task DeleteAllIndexedWordsForTopic(int topicID)
		{
			throw new NotImplementedException();
		}

		public override async Task SaveSearchWord(int topicID, string word, int rank)
		{
			throw new NotImplementedException();
		}

		public override async Task<Tuple<Response<List<Topic>>, int>> SearchTopics(string searchTerm, List<int> hiddenForums, SearchType searchType, int startRow, int pageSize)
		{
			var response = _elasticSearchClientWrapper.SearchTopicsWithIDs(searchTerm, hiddenForums, searchType, startRow, pageSize, out var topicCount);
			Response<List<Topic>> result;
			if (!response.IsValid)
			{
				result = new Response<List<Topic>>(null, false, response.Exception, response.DebugInfo);
				return Tuple.Create(result, topicCount);
			}
			var topics = await _topicRepository.Get(response.Data);
			result = new Response<List<Topic>>(topics);
			return Tuple.Create(result, topicCount);
		}
	}
}