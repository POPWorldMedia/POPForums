using System;
using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public class FavoriteTopicService : IFavoriteTopicService
	{
		public FavoriteTopicService(ISettingsManager settingsManager, IFavoriteTopicsRepository favoriteTopicRepository)
		{
			_settingsManager = settingsManager;
			_favoriteTopicRepository = favoriteTopicRepository;
		}

		private readonly ISettingsManager _settingsManager;
		private readonly IFavoriteTopicsRepository _favoriteTopicRepository;

		public List<Topic> GetTopics(User user, int pageIndex, out PagerContext pagerContext)
		{
			var pageSize = _settingsManager.Current.TopicsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var topics = _favoriteTopicRepository.GetFavoriteTopics(user.UserID, startRow, pageSize);
			var topicCount = _favoriteTopicRepository.GetFavoriteTopicCount(user.UserID);
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(topicCount) / Convert.ToDouble(pageSize)));
			pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return topics;
		}

		public bool IsTopicFavorite(User user, Topic topic)
		{
			return _favoriteTopicRepository.IsTopicFavorite(user.UserID, topic.TopicID);
		}

		public void AddFavoriteTopic(User user, Topic topic)
		{
			_favoriteTopicRepository.AddFavoriteTopic(user.UserID, topic.TopicID);
		}

		public void RemoveFavoriteTopic(User user, Topic topic)
		{
			_favoriteTopicRepository.RemoveFavoriteTopic(user.UserID, topic.TopicID);
		}
	}
}
