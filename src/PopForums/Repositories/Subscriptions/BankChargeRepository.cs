namespace PopForums.Repositories.Subscriptions;

public interface IBankChargeRepository
{
	Task<BasicServiceResponse<CreateCustomerResult>> CreateCustomer(string token, string email, int userID);

	Task<BasicServiceResponse<Transaction>> ChargeCustomer(string customerID, int userID, decimal amount, DateTime timeStamp, string skuID,
		string email);
}

public class BankChargeRepository : IBankChargeRepository
{
	public async Task<BasicServiceResponse<CreateCustomerResult>> CreateCustomer(string token, string email, int userID)
	{
		throw new NotImplementedException();
	}

	public async Task<BasicServiceResponse<Transaction>> ChargeCustomer(string customerID, int userID, decimal amount, DateTime timeStamp, string skuID,
		string email)
	{
		throw new NotImplementedException();
	}
}