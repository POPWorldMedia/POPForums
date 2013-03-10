using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer.Repositories
{
	public class SearchRepository : ISearchRepository
	{
		public SearchRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public List<string> GetJunkWords()
		{
			var words = new List<string>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT JunkWord FROM pf_JunkWords ORDER BY JunkWord")
					.ExecuteReader()
					.ReadAll(r => words.Add(r.GetString(0))));
			return words;
		}

		public void CreateJunkWord(string word)
		{
			var exists = false;
			_sqlObjectFactory.GetConnection().Using(connection => exists =
				connection.Command("SELECT JunkWord FROM pf_JunkWords WHERE JunkWord LIKE @JunkWord")
					.AddParameter("@JunkWord", word)
					.ExecuteReader().Read());
			if (exists)
				return;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("INSERT INTO pf_JunkWords (JunkWord) VALUES (@JunkWord)")
				.AddParameter("@JunkWord", word)
				.ExecuteNonQuery());
		}

		public void DeleteJunkWord(string word)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_JunkWords WHERE JunkWord = @JunkWord")
				.AddParameter("@JunkWord", word)
				.ExecuteNonQuery());
		}

		public Topic GetNextTopicForIndexing()
		{
			Topic topic = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT TOP 1 " + TopicRepository.TopicFields + " FROM pf_Topic WHERE IsDeleted = 0 AND IsIndexed = 0 ORDER BY LastPostTime")
				.ExecuteReader()
				.ReadOne(r => topic = TopicRepository.GetTopicFromReader(r)));
			return topic;
		}

		public void MarkTopicAsIndexed(int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("UPDATE pf_Topic SET IsIndexed = 1 WHERE TopicID = @TopicID")
				.AddParameter("@TopicID", topicID)
				.ExecuteNonQuery());
		}

		public void DeleteAllIndexedWordsForTopic(int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_TopicSearchWords WHERE TopicID = @TopicID")
				.AddParameter("@TopicID", topicID)
				.ExecuteNonQuery());
		}

		public void SaveSearchWord(int topicID, string word, int rank)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("INSERT INTO pf_TopicSearchWords (SearchWord, TopicID, Rank) VALUES (@SearchWord, @TopicID, @Rank)")
				.AddParameter("@SearchWord", word)
				.AddParameter("@TopicID", topicID)
				.AddParameter("@Rank", rank)
				.ExecuteNonQuery());
		}

		public List<Topic> SearchTopics(string searchTerm, List<int> hiddenForums, SearchType searchType, int startRow, int pageSize, out int topicCount)
		{
			topicCount = 0;
			if (searchTerm.Trim() == String.Empty)
				return new List<Topic>();
			var topics = new List<Topic>();
			var wordArray = searchTerm.Split(new [] { ' ' });
			var wordList = new List<string>();
			var junkWords = GetJunkWords();
			var alphaNum = new Regex(@"[\w']{2,}", RegexOptions.Compiled);
			for (var x = 0; x < wordArray.Length; x++)
			{
				foreach (Match match in alphaNum.Matches(wordArray[x]))
				{
					if (!junkWords.Contains(match.Value))
						wordList.Add(match.Value);
				}
			}
			var words = wordList.ToArray();
			var wordParameters = new Dictionary<string, string>();
			var sb = new StringBuilder();
			sb.Append("WITH FirstEntries AS (SELECT ROW_NUMBER() OVER (PARTITION BY pf_Topic.TopicID ORDER BY pf_Topic.LastPostTime DESC) AS GroupRow, ");
			sb.Append(TopicRepository.TopicFields);
			sb.Append(", ((");
			for (var x = 0; x < words.Length; x++)
			{
				sb.Append("q");
				sb.Append(x.ToString());
				sb.Append(".Rank");
				if (x < words.Length - 1) sb.Append("+");
			}
			sb.Append(")/" + words.Length + ") AS CompositeRank FROM pf_Topic ");
			for (int x = 0; x < words.Length; x++)
			{
				string xstring = x.ToString();
				sb.Append(" JOIN (SELECT TOP 10000 TopicID, pf_TopicSearchWords.Rank FROM pf_TopicSearchWords WHERE SearchWord = ");
				var param = "@w" + x;
				wordParameters.Add(param, words[x]);
				sb.Append(param);
				sb.Append(" ORDER BY pf_TopicSearchWords.Rank DESC) AS q");
				sb.Append(xstring);
				sb.Append(" ON pf_Topic.TopicID = q");
				sb.Append(xstring);
				sb.Append(".TopicID");
			}
			if (hiddenForums.Count > 0)
			{
				sb.Append(" WHERE");
				for (int x = 0; x < hiddenForums.Count; x++)
				{
					if (x > 0) sb.Append(" AND");
					sb.Append(String.Format(" NOT ForumID = {0} ", hiddenForums[x]));
				}
			}

			string orderBy;
			switch (searchType)
			{
				case SearchType.Date:
					orderBy = "LastPostTime DESC";
					break;
				case SearchType.Name:
					orderBy = "StartedByName";
					break;
				case SearchType.Replies:
					orderBy = "ReplyCount DESC";
					break;
				case SearchType.Title:
					orderBy = "Title";
					break;
				default:
					orderBy = "CompositeRank DESC, LastPostTime DESC";
					break;
			}

			sb.Append("),\r\nEntries as (SELECT *,ROW_NUMBER() OVER (ORDER BY ");
			sb.Append(orderBy);
			sb.Append(") AS Row, COUNT(*) OVER () as cnt FROM FirstEntries WHERE GroupRow = 1)\r\nSELECT TopicID, ForumID, Title, ReplyCount, ViewCount, StartedByUserID, StartedByName, LastPostUserID, LastPostName, LastPostTime, IsClosed, IsPinned, IsDeleted, IsIndexed, UrlName, cnt FROM Entries WHERE Row BETWEEN @StartRow AND @StartRow + @PageSize - 1");

			if (words.Length == 0)
				return topics;


			var connection = _sqlObjectFactory.GetConnection();
			var command = connection.Command(sb.ToString());
			command.AddParameter("@StartRow", startRow);
			command.AddParameter("@PageSize", pageSize);
			foreach (var item in wordParameters)
				command.AddParameter(item.Key, item.Value);
			connection.Open();
			var reader = command.ExecuteReader();

			while (reader.Read())
			{
				var topic = TopicRepository.GetTopicFromReader(reader);
				topics.Add(topic);
				topicCount = Convert.ToInt32(reader["cnt"]);
			}
			reader.Close();
			connection.Close();
			return topics;
		}
	}
}