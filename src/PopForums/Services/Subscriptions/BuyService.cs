namespace PopForums.Services.Subscriptions;

public interface IBuyService
{
	Task<TransactionResult> BuyNew(BuyModel buyModel, int userID);
	Task<TransactionResult> UpdatePaymentMethod(int userID, string token);
}

public class BuyService : IBuyService
{
	public async Task<TransactionResult> BuyNew(BuyModel buyModel, int userID)
	{
		throw new NotImplementedException();
	}

	public async Task<TransactionResult> UpdatePaymentMethod(int userID, string token)
	{
		throw new NotImplementedException();
	}
}