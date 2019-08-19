using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface IFavoriteTopicService
	{
		Task<Tuple<List<Topic>, PagerContext>> GetTopics(User user, int pageIndex);
		Task<bool> IsTopicFavorite(User user, Topic topic);
		Task AddFavoriteTopic(User user, Topic topic);
		Task RemoveFavoriteTopic(User user, Topic topic);
	}

	public class FavoriteTopicService : IFavoriteTopicService
	{
		public FavoriteTopicService(ISettingsManager settingsManager, IFavoriteTopicsRepository favoriteTopicRepository)
		{
			_settingsManager = settingsManager;
			_favoriteTopicRepository = favoriteTopicRepository;
		}

		private readonly ISettingsManager _settingsManager;
		private readonly IFavoriteTopicsRepository _favoriteTopicRepository;

		public async Task<Tuple<List<Topic>, PagerContext>> GetTopics(User user, int pageIndex)
		{
			var pageSize = _settingsManager.Current.TopicsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var topics = await _favoriteTopicRepository.GetFavoriteTopics(user.UserID, startRow, pageSize);
			var topicCount = await _favoriteTopicRepository.GetFavoriteTopicCount(user.UserID);
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(topicCount) / Convert.ToDouble(pageSize)));
			var pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return Tuple.Create(topics, pagerContext);
		}

		public async Task<bool> IsTopicFavorite(User user, Topic topic)
		{
			return await _favoriteTopicRepository.IsTopicFavorite(user.UserID, topic.TopicID);
		}

		public async Task AddFavoriteTopic(User user, Topic topic)
		{
			await _favoriteTopicRepository.AddFavoriteTopic(user.UserID, topic.TopicID);
		}

		public async Task RemoveFavoriteTopic(User user, Topic topic)
		{
			await _favoriteTopicRepository.RemoveFavoriteTopic(user.UserID, topic.TopicID);
		}
	}
}
