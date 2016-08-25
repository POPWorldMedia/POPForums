using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using PopForums.Configuration;
using PopForums.Models;

namespace PopForums.Services
{
	public class SearchIndexWorker
	{
		private static readonly object _syncRoot = new Object();

		private SearchIndexWorker()
		{
			// only allow Instance to create a new instance
		}

		public void IndexNextTopic(ISearchService searchService, ISettingsManager settingsManager, IPostService postService, IErrorLog errorLog)
		{
			if (!Monitor.TryEnter(_syncRoot)) return;
			try
			{
				DoIndex(searchService, settingsManager, postService);
			}
			catch (Exception exc)
			{
				errorLog.Log(exc, ErrorSeverity.Error);
			}
			finally
			{
				Monitor.Exit(_syncRoot);
			}
		}

		private static SearchIndexWorker _instance;
		public static SearchIndexWorker Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new SearchIndexWorker();
				}
				return _instance;
			}
		}

		public void DoIndex(ISearchService searchService, ISettingsManager settingsManager, IPostService postService)
		{
			var topic = searchService.GetNextTopicForIndexing();
			if (topic != null)
			{
				searchService.MarkTopicAsIndexed(topic);
				searchService.DeleteAllIndexedWordsForTopic(topic);

				var junkList = searchService.GetJunkWords();
				var wordList = new List<SearchWord>();
				var alphaNum = SearchService.SearchWordPattern;
				var posts = postService.GetPosts(topic, false);

				foreach (var post in posts)
				{
					var firstPostMultiplier = 1;
					if (post.IsFirstInTopic) firstPostMultiplier = 2;
					var postWords = post.FullText.Split(new[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
					if (postWords.Length > 0)
					{
						for (var x = 0; x < postWords.Length; x++)
						{
							foreach (Match match in alphaNum.Matches(postWords[x]))
							{
								TestForIndex(topic, match.Value, 1, firstPostMultiplier, true, wordList, junkList);
							}
						}
					}
					// index the name
					foreach (Match match in alphaNum.Matches(post.Name))
					{
						TestForIndex(topic, match.Value, 2, firstPostMultiplier, false, wordList, junkList);
					}
				}

				// bonus for appearing in title
				foreach (Match match in alphaNum.Matches(topic.Title))
				{
					TestForIndex(topic, match.Value, 20, 1, false, wordList, junkList);
				}


				foreach (var word in wordList)
				{
					searchService.SaveSearchWord(word);
				}
			}
		}

		private void TestForIndex(Topic topic, string testWord, int increment, int multiplier, bool cap, List<SearchWord> wordList, List<String> junkList)
		{
			testWord = testWord.ToLower();
			if (junkList.IndexOf(testWord) < 0)
			{
				var foundWord = wordList.Find(w => w.Word == testWord);
				if (foundWord != null)
				{
					foundWord.Rank += increment * multiplier;
					// cap the word frequency score
					if (cap && foundWord.Rank > 120) foundWord.Rank = 120;
				}
				else wordList.Add(new SearchWord { Rank = 1, TopicID = topic.TopicID, Word = testWord });
			}
		}
	}
}