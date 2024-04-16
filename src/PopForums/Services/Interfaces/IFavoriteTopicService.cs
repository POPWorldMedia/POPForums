namespace PopForums.Services.Interfaces;

public interface IFavoriteTopicService
{
    Task<Tuple<List<Topic>, PagerContext>> GetTopics(User user, int pageIndex);

    Task<bool> IsTopicFavorite(int userID, int topicID);

    Task AddFavoriteTopic(User user, Topic topic);

    Task RemoveFavoriteTopic(User user, Topic topic);
}
