using PopForums.Repositories.Subscriptions;

namespace PopForums.Services.Subscriptions;

public interface IManualSubscriptionService
{
	Task<BasicServiceResponse<SubscriptionHistory>> Apply(int userID, string skuID, DateOnly expiration);
}

public class ManualSubscriptionService(IUserRepository userRepository, IProfileRepository profileRepository, ISkuRepository skuRepository, ISubscriptionHistoryRepository subscriptionHistoryRepository) : IManualSubscriptionService
{
	public async Task<BasicServiceResponse<SubscriptionHistory>> Apply(int userID, string skuID, DateOnly expiration)
	{
		var sku = await skuRepository.Get(skuID);
		if (sku == null)
			return BasicServiceResponse<SubscriptionHistory>.Failed($"SKU {skuID} not found.");

		var user = await userRepository.GetUser(userID);
		if (user == null)
			return BasicServiceResponse<SubscriptionHistory>.Failed($"UserID {userID} not found.");

		var profile = await profileRepository.GetProfile(userID);
		var oldSkuID = profile.SkuID;
		var oldExpiration = user.SubscriptionExpiration;

		profile.SkuID = sku.SkuID;
		await profileRepository.Update(profile);
		await userRepository.UpdateSubscriptionExpiration(userID, expiration);

		var history = new SubscriptionHistory
		{
			UserID = userID,
			TimeStamp = DateTime.UtcNow,
			SkuID = sku.SkuID,
			Message = $"Manual: SKU {oldSkuID ?? "(none)"} -> {sku.SkuID}, expiration {Format(oldExpiration)} -> {expiration}"
		};
		await subscriptionHistoryRepository.Create(history);

		return BasicServiceResponse<SubscriptionHistory>.Success(history);
	}

	private static string Format(DateOnly? date) => date.HasValue ? date.Value.ToString() : "(none)";
}
