using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IFavoriteTopicsRepository
	{
		List<Topic> GetFavoriteTopics(int userID, int startRow, int pageSize);
		int GetFavoriteTopicCount(int userID);
		bool IsTopicFavorite(int userID, int topicID);
		void AddFavoriteTopic(int userID, int topicID);
		void RemoveFavoriteTopic(int userID, int topicID);
	}
}
