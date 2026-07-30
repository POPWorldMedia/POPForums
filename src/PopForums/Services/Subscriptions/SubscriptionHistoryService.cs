namespace PopForums.Services.Subscriptions;

public interface ISubscriptionHistoryService
{
	Task<List<SubscriptionHistory>> GetByUserID(int userID);
}

public class SubscriptionHistoryService(ISubscriptionHistoryRepository subscriptionHistoryRepository) : ISubscriptionHistoryService
{
	public async Task<List<SubscriptionHistory>> GetByUserID(int userID)
	{
		var history = await subscriptionHistoryRepository.GetByUserID(userID);
		return history.OrderByDescending(h => h.TimeStamp).ToList();
	}
}
