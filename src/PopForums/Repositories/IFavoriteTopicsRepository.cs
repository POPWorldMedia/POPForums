using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IFavoriteTopicsRepository
	{
		Task<List<Topic>> GetFavoriteTopics(int userID, int startRow, int pageSize);
		Task<int> GetFavoriteTopicCount(int userID);
		Task<bool> IsTopicFavorite(int userID, int topicID);
		Task AddFavoriteTopic(int userID, int topicID);
		Task RemoveFavoriteTopic(int userID, int topicID);
	}
}
