namespace PopForums.Services.Subscriptions;

public interface IRenewalService
{
	Task<IEnumerable<int>> GetUserIDsForRenewal();
	Task<BasicServiceResponse<Transaction>> ChargeAndRecordRenewal(string userID);
}

public class RenewalService(IUserRepository userRepository) : IRenewalService
{
	public async Task<IEnumerable<int>> GetUserIDsForRenewal()
	{
		throw new NotImplementedException();
	}
	
	public async Task<BasicServiceResponse<Transaction>> ChargeAndRecordRenewal(string userID)
	{
		throw new NotImplementedException();
	}
}