using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Search.Documents;
using PopForums.Configuration;
using PopForums.Sql;
using PopForums.Models;
using PopForums.Repositories;
using Azure;

#pragma warning disable 1998

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

		public override async Task<Tuple<PopForums.Models.Response<List<Topic>>, int>> SearchTopics(string searchTerm, List<int> hiddenForums, SearchType searchType, int startRow, int pageSize)
		{
			int topicCount;
			var searchClient = new SearchClient(new Uri(_config.SearchUrl), SearchIndexSubsystem.IndexName, new AzureKeyCredential(_config.SearchKey));
			try
			{
				var options = new SearchOptions();
				switch (searchType)
				{
					case SearchType.Date:
						options.OrderBy.Add("LastPostTime desc");
						break;
					case SearchType.Name:
						options.OrderBy.Add("StartedByName");
						break;
					case SearchType.Replies:
						options.OrderBy.Add("Replies desc");
						break;
					case SearchType.Title:
						options.OrderBy.Add("Title");
						break;
					default:
						break;
				}
				if (startRow > 1)
					options.Skip = startRow - 1;
				options.Size = pageSize;
				if (hiddenForums != null && hiddenForums.Any())
				{
					var neConditions = hiddenForums.Select(x => "ForumID ne " + x);
					options.Filter = string.Join(" and ", neConditions);
				}
				options.IncludeTotalCount = true;
				options.Select.Add("TopicID");
				var result = searchClient.Search<SearchTopic>(searchTerm, options);
				var resultModels = result.Value.GetResults();
				var topicIDs = resultModels.Select(x => Convert.ToInt32(x.Document.TopicID));
				var topics = await _topicRepository.Get(topicIDs);
				topicCount = Convert.ToInt32(result.Value.TotalCount);
				return Tuple.Create(new PopForums.Models.Response<List<Topic>>(topics), topicCount);
			}
			catch (Exception exception)
			{
				_errorLog.Log(exception, ErrorSeverity.Error);
				topicCount = 0;
				return Tuple.Create(new PopForums.Models.Response<List<Topic>>(null, false, exception), topicCount);
			}
		}
	}
}
