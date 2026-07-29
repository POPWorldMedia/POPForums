using PopForums.Repositories.Subscriptions;

namespace PopForums.Services.Subscriptions;

public interface IRenewalService
{
	Task<IEnumerable<int>> GetUserIDsForRenewal();
	Task<BasicServiceResponse<Transaction>> ChargeAndRecordRenewal(int userID);
}

public class RenewalService(IUserRepository userRepository, ISkuRepository skuRepository, IBankChargeRepository bankChargeRepository, ITransactionRepository transactionRepository, IProfileRepository profileRepository, ISubscriptionHistoryRepository subscriptionHistoryRepository) : IRenewalService
{
	public async Task<IEnumerable<int>> GetUserIDsForRenewal()
	{
		var today = DateOnly.FromDateTime(DateTime.UtcNow);
		return await userRepository.GetUserIDsBySubscriptionExpiration(today);
	}

	public async Task<BasicServiceResponse<Transaction>> ChargeAndRecordRenewal(int userID)
	{
		var now = DateTime.UtcNow;

		var user = await userRepository.GetUser(userID);
		var profile = await profileRepository.GetProfile(userID);

		// a renewal can use an inactive sku, but the sku must exist
		var sku = await skuRepository.Get(profile.SkuID);
		if (sku == null)
			throw new Exception($"SKU {profile.SkuID} not found for user {userID} during renewal.");

		// charge existing customer
		var transactionResult = await bankChargeRepository.ChargeCustomer(profile.CustomerID, userID, sku.Price, now, sku.SkuID, user.Email);
		if (!transactionResult.IsSuccessful)
		{
			var failedTransactionResult = BasicServiceResponse<Transaction>.Failed(transactionResult.Message);
			return failedTransactionResult;
		}

		// create and record transaction
		var transaction = transactionResult.Data;
		await transactionRepository.Create(transaction);

		// renewal happens on the expiration date, so there's no time left to preserve; base off the current expiration, or today if none is set
		var baseExpiration = user.SubscriptionExpiration ?? DateOnly.FromDateTime(now);
		var newExpiration = baseExpiration.AddMonths(sku.Months);
		await userRepository.UpdateSubscriptionExpiration(userID, newExpiration);

		// record subscriptionhistory entry
		var subscriptionHistory = new SubscriptionHistory
		{
			UserID = userID,
			TimeStamp = now,
			SkuID = sku.SkuID,
			Message = $"Renewal charge: {sku.Name} - {sku.Price:C} - card: {transaction.Last4} - exp: {newExpiration}"
		};
		await subscriptionHistoryRepository.Create(subscriptionHistory);

		return transactionResult;
	}
}