namespace PopForums.Services.Subscriptions;

public interface IRenewalOrchestrationService
{
	Task EnqueueUsersForRenewal();
	Task ProcessRenewal(int userID);
}

public class RenewalOrchestrationService(IRenewalService renewalService, IRenewalQueueRepository renewalQueueRepository, ITenantService tenantService, IErrorLog errorLog, ISettingsManager settingsManager, INotificationTunnel notificationTunnel, IProfileRepository profileRepository, ISkuService skuService) : IRenewalOrchestrationService
{
	public async Task EnqueueUsersForRenewal()
	{
		if (!settingsManager.Current.IsSubscriptionEnabled)
			return;

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
		if (!settingsManager.Current.IsSubscriptionEnabled)
			return;

		// this will be called by a worker and azure function
		var tenantID = tenantService.GetTenant();

		string skuName = null;
		var profile = await profileRepository.GetProfile(userID);
		if (profile != null && !string.IsNullOrEmpty(profile.SkuID))
		{
			var sku = await skuService.Get(profile.SkuID);
			skuName = sku?.Name;
		}

		var result = await renewalService.ChargeAndRecordRenewal(userID);
		if (!result.IsSuccessful)
		{
			errorLog.Log(null, ErrorSeverity.Information, $"Renewal charge failed for user {userID}: {result.Message}");
			notificationTunnel.SendNotificationForSubscriptionRenewalFailed(userID, skuName, tenantID);
		}
		else
		{
			notificationTunnel.SendNotificationForSubscriptionRenewed(userID, skuName, tenantID);
		}
	}
}