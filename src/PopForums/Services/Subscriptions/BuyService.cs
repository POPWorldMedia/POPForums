using PopForums.Repositories.Subscriptions;

namespace PopForums.Services.Subscriptions;

public interface IBuyService
{
	Task<BasicServiceResponse<Transaction>> BuyNew(BuyModel buyModel, int userID);
	Task<BasicServiceResponse<string>> UpdatePaymentMethod(int userID, string token);
}

public class BuyService(ISkuRepository skuRepository, IUserRepository userRepository, IBankChargeRepository bankChargeRepository, ITransactionRepository transactionRepository, IProfileRepository profileRepository, ISubscriptionHistoryRepository subscriptionHistoryRepository) : IBuyService
{
	public async Task<BasicServiceResponse<Transaction>> BuyNew(BuyModel buyModel, int userID)
	{
		var errors = string.Empty;
		var now = DateTime.UtcNow;
		
		// check for valid sku
		var sku = await skuRepository.Get(buyModel.SkuID);
		if (sku == null || !sku.IsActive)
			errors += " That plan is no longer available.";
		
		// error check
		if (!string.IsNullOrEmpty(errors))
		{
			var failedValidCheckResult = BasicServiceResponse<Transaction>.Failed(errors);
			return failedValidCheckResult;
		}

		var user = await userRepository.GetUser(userID);

		// create customer and charge it
		var createCustomerResult = await bankChargeRepository.CreateCustomer(buyModel.Token, user.Email, user.UserID);
		if (!createCustomerResult.IsSuccessful)
		{
			var failedCreateCustomerResult = BasicServiceResponse<Transaction>.Failed(createCustomerResult.Message);
			return failedCreateCustomerResult;
		}
		var customerID = createCustomerResult.Data.CustomerID;

		var transactionResult = await bankChargeRepository.ChargeCustomer(customerID, user.UserID, sku.Price, now, sku.SkuID, user.Email);
		if (!transactionResult.IsSuccessful)
		{
			var failedTransactionResult = BasicServiceResponse<Transaction>.Failed(transactionResult.Message);
			return failedTransactionResult;
		}
		
		// create and record transaction
		var transaction = transactionResult.Data;
		await transactionRepository.Create(transaction);

		// update user's last4 and sku on profile, expiration on user
		var profile = await profileRepository.GetProfile(userID);
		profile.Last4 = transaction.Last4;
		profile.CustomerID = customerID;
		profile.SkuID = sku.SkuID;
		await profileRepository.Update(profile);

		// expiration is either sku.Months + now, date only, or if expiration is after now, sku.Months + current expiration
		var today = DateOnly.FromDateTime(now);
		var baseExpiration = user.SubscriptionExpiration.HasValue && user.SubscriptionExpiration.Value > today
			? user.SubscriptionExpiration.Value
			: today;
		var newExpiration = baseExpiration.AddMonths(sku.Months);
		await userRepository.UpdateSubscriptionExpiration(userID, newExpiration);

		// record subscriptionhistory entry
		var subscriptionHistory = new SubscriptionHistory
		{
			UserID = userID,
			TimeStamp = now,
			SkuID = sku.SkuID,
			Message = $"Charge: {sku.Name} - {sku.Price:C} - card: {createCustomerResult.Data.Last4} - exp: {newExpiration}"
		};
		await subscriptionHistoryRepository.Create(subscriptionHistory);

		return transactionResult;
	}

	public async Task<BasicServiceResponse<string>> UpdatePaymentMethod(int userID, string token)
	{
		// get the user
		var user = await userRepository.GetUser(userID);

		// create new customer using token, userid and email
		var createCustomerResult = await bankChargeRepository.CreateCustomer(token, user.Email, user.UserID);

		// if failure, return it with error message
		if (!createCustomerResult.IsSuccessful)
			return BasicServiceResponse<string>.Failed(createCustomerResult.Message);

		var last4 = createCustomerResult.Data.Last4;

		// update last4 on profile
		var profile = await profileRepository.GetProfile(userID);
		profile.Last4 = last4;
		profile.CustomerID = createCustomerResult.Data.CustomerID;
		await profileRepository.Update(profile);

		// add history, with message as: New card: {last4}
		var subscriptionHistory = new SubscriptionHistory
		{
			UserID = userID,
			TimeStamp = DateTime.UtcNow,
			SkuID = profile.SkuID,
			Message = $"New card: {last4}"
		};
		await subscriptionHistoryRepository.Create(subscriptionHistory);

		// return last4
		return BasicServiceResponse<string>.Success(last4);
	}
}