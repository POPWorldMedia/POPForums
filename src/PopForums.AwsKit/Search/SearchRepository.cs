using System;
using System.Collections.Generic;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Sql;

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

		public override List<string> GetJunkWords()
		{
			return new List<string>();
		}

		public override void CreateJunkWord(string word)
		{
			throw new NotImplementedException();
		}

		public override void DeleteJunkWord(string word)
		{
			throw new NotImplementedException();
		}

		public override void DeleteAllIndexedWordsForTopic(int topicID)
		{
			throw new NotImplementedException();
		}

		public override void SaveSearchWord(int topicID, string word, int rank)
		{
			throw new NotImplementedException();
		}

		public override Response<List<Topic>> SearchTopics(string searchTerm, List<int> hiddenForums, SearchType searchType, int startRow, int pageSize, out int topicCount)
		{
			var response = _elasticSearchClientWrapper.SearchTopicsWithIDs(searchTerm, hiddenForums, searchType, startRow, pageSize, out topicCount);
			Response<List<Topic>> result;
			if (!response.IsValid)
			{
				result = new Response<List<Topic>>(null, false, response.Exception, response.DebugInfo);
				return result;
			}
			var topics = _topicRepository.Get(response.Data);
			result = new Response<List<Topic>>(topics);
			return result;
		}
	}
}