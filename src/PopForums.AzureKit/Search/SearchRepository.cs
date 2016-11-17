using System;
using System.Collections.Generic;
using Microsoft.Azure.Search;
using Microsoft.Rest.Azure;
using PopForums.Configuration;
using PopForums.Data.Sql;
using PopForums.Models;

namespace PopForums.AzureKit.Search
{
    public class SearchRepository : Data.Sql.Repositories.SearchRepository
	{
		private readonly IConfig _config;
		private readonly IErrorLog _errorLog;

		public SearchRepository(ISqlObjectFactory sqlObjectFactory, IConfig config, IErrorLog errorLog) : base(sqlObjectFactory)
		{
			_config = config;
			_errorLog = errorLog;
		}

		public override List<string> GetJunkWords()
	    {
		    throw new NotImplementedException();
	    }

	    public override void CreateJunkWord(string word)
	    {
		    throw new NotImplementedException();
	    }

	    public override void DeleteJunkWord(string word)
	    {
		    throw new NotImplementedException();
	    }

	    // GetNextTopicForIndexing() uses base

	    // MarkTopicAsIndexed(int topicID) uses base

	    public override void DeleteAllIndexedWordsForTopic(int topicID)
	    {
		    throw new NotImplementedException();
	    }

	    public override void SaveSearchWord(int topicID, string word, int rank)
	    {
		    throw new NotImplementedException();
	    }

		public override List<Topic> SearchTopics(string searchTerm, List<int> hiddenForums, SearchType searchType, int startRow, int pageSize, out int topicCount)
		{
			var list = new List<Topic>();

			var serviceIndexClient = new SearchIndexClient(_config.SearchUrl, SearchIndexSubsystem.IndexName, new SearchCredentials(_config.SearchKey));
			try
			{
				var result = serviceIndexClient.Documents.Search<SearchTopic>("test");
			}
			catch (CloudException cloudException)
			{
				_errorLog.Log(cloudException, ErrorSeverity.Error);
				topicCount = 0;
				return new List<Topic>();
			}

			topicCount = 1;
			return list;
		}
	}
}
