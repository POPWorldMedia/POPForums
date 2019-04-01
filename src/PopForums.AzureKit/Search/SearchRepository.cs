using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Rest.Azure;
using PopForums.Configuration;
using PopForums.Sql;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.AzureKit.Search
{
    public class SearchRepository : Sql.Repositories.SearchRepository
	{
		private readonly IConfig _config;
		private readonly IErrorLog _errorLog;
		private readonly ITopicRepository _topicRepository;

		public SearchRepository(ISqlObjectFactory sqlObjectFactory, IConfig config, IErrorLog errorLog, ITopicRepository topicRepository) : base(sqlObjectFactory)
		{
			_config = config;
			_errorLog = errorLog;
			_topicRepository = topicRepository;
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
			var serviceIndexClient = new SearchIndexClient(_config.SearchUrl, SearchIndexSubsystem.IndexName, new SearchCredentials(_config.SearchKey));
			try
			{
				var parameters = new SearchParameters();
				switch (searchType)
				{
					case SearchType.Date:
						parameters.OrderBy = new List<string> { "lastPostTime desc" };
						break;
					case SearchType.Name:
						parameters.OrderBy = new List<string> { "startedByName" };
						break;
					case SearchType.Replies:
						parameters.OrderBy = new List<string> { "replies desc" };
						break;
					case SearchType.Title:
						parameters.OrderBy = new List<string> { "title" };
						break;
					default:
						break;
				}
				if (startRow > 1)
					parameters.Skip = startRow - 1;
				parameters.Top = pageSize;
				if (hiddenForums != null && hiddenForums.Any())
				{
					var neConditions = hiddenForums.Select(x => "forumID ne " + x);
					parameters.Filter = string.Join(" and ", neConditions);
				}
				parameters.IncludeTotalResultCount = true;
				parameters.Select = new [] {"topicID"};
				var result = serviceIndexClient.Documents.Search<SearchTopic>(searchTerm, parameters);
				var topicIDs = result.Results.Select(x => Convert.ToInt32(x.Document.TopicID));
				var topics = _topicRepository.Get(topicIDs);
				topicCount = Convert.ToInt32(result.Count);
				return new Response<List<Topic>>(topics);
			}
			catch (CloudException cloudException)
			{
				_errorLog.Log(cloudException, ErrorSeverity.Error);
				topicCount = 0;
				return new Response<List<Topic>>(null, false, cloudException);
			}
		}
	}
}
