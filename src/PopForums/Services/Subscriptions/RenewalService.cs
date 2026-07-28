namespace PopForums.Services.Subscriptions;

public interface IRenewalService
{
	Task<IEnumerable<int>> GetUserIDsForRenewal();
	Task<TransactionResult> ChargeAndRecordRenewal(string userID);
}

public class RenewalService : IRenewalService
{
	public async Task<IEnumerable<int>> GetUserIDsForRenewal()
	{
		throw new NotImplementedException();
	}
	
	public async Task<TransactionResult> ChargeAndRecordRenewal(string userID)
	{
		throw new NotImplementedException();
	}
}