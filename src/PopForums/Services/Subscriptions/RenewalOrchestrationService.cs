namespace PopForums.Services.Subscriptions;

public interface IRenewalOrchestrationService
{
	Task EnqueueTenantsForRenewal();
	Task ProcessRenewal(int userID);
}

public class RenewalOrchestrationService : IRenewalOrchestrationService
{
	public async Task EnqueueTenantsForRenewal()
	{
		throw new NotImplementedException();
	}

	public async Task ProcessRenewal(int userID)
	{
		throw new NotImplementedException();
	}
}