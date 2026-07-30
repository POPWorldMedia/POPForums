namespace PopForums.Services.Subscriptions;

public interface IRenewalOrchestrationService
{
	Task EnqueueUsersForRenewal();
	Task ProcessRenewal(int userID);
}

public class RenewalOrchestrationService(IRenewalService renewalService, IRenewalQueueRepository renewalQueueRepository, ITenantService tenantService, IErrorLog errorLog) : IRenewalOrchestrationService
{
	public async Task EnqueueUsersForRenewal()
	{
		var tenantID = tenantService.GetTenant();
		var userIDs = await renewalService.GetUserIDsForRenewal();
		foreach (var userID in userIDs)
		{
			var payload = new RenewalQueuePayload { UserID = userID, TenantID = tenantID };
			await renewalQueueRepository.Enqueue(payload);
		}
	}

	public async Task ProcessRenewal(int userID)
	{
		// this will be called by a worker and azure function
		var result = await renewalService.ChargeAndRecordRenewal(userID);
		if (!result.IsSuccessful)
			errorLog.Log(null, ErrorSeverity.Information, $"Renewal charge failed for user {userID}: {result.Message}");
	}
}