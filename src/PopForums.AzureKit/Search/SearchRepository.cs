using System;
using System.Collections.Generic;
using Microsoft.Azure.Search;
using PopForums.Configuration;
using PopForums.Data.Sql;
using PopForums.Models;

namespace PopForums.AzureKit.Search
{
    public class SearchRepository : Data.Sql.Repositories.SearchRepository
	{
		private readonly IConfig _config;

		public SearchRepository(ISqlObjectFactory sqlObjectFactory, IConfig config) : base(sqlObjectFactory)
		{
			_config = config;
		}

		public new List<string> GetJunkWords()
	    {
		    throw new NotImplementedException();
	    }

	    public new void CreateJunkWord(string word)
	    {
		    throw new NotImplementedException();
	    }

	    public new void DeleteJunkWord(string word)
	    {
		    throw new NotImplementedException();
	    }

	    public new Topic GetNextTopicForIndexing()
	    {
		    return base.GetNextTopicForIndexing();
	    }

	    public new void MarkTopicAsIndexed(int topicID)
	    {
		    base.MarkTopicAsIndexed(topicID);
	    }

	    public new void DeleteAllIndexedWordsForTopic(int topicID)
	    {
		    throw new NotImplementedException();
	    }

	    public new void SaveSearchWord(int topicID, string word, int rank)
	    {
		    throw new NotImplementedException();
	    }

	    public new List<Topic> SearchTopics(string searchTerm, List<int> hiddenForums, SearchType searchType, int startRow, int pageSize,
		    out int topicCount)
	    {
		    var list = new List<Topic>();

			var serviceIndexClient = new SearchIndexClient(_config.SearchUrl, SearchIndexSubsystem.IndexName, new SearchCredentials(_config.SearchKey));
		    var result = serviceIndexClient.Documents.Search<SearchTopic>("test");

		    topicCount = 1;
			return list;
	    }
    }
}
