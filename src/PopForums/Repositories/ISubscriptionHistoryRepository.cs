namespace PopForums.Repositories;

public interface ISubscriptionHistoryRepository
{
	Task Create(SubscriptionHistory subscriptionHistory);
	Task<List<SubscriptionHistory>> GetByUserID(int userID);
}
