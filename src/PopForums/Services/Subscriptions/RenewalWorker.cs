namespace PopForums.Services.Subscriptions;

public interface IRenewalWorker
{
	void Execute();
}

public class RenewalWorker(IRenewalOrchestrationService renewalOrchestrationService, IRenewalQueueRepository renewalQueueRepository, IErrorLog errorLog) : IRenewalWorker
{
	public async void Execute()
	{
		try
		{
			var payload = await renewalQueueRepository.Dequeue();
			if (payload == null)
				return;
			await renewalOrchestrationService.ProcessRenewal(payload.UserID);
		}
		catch (Exception exc)
		{
			errorLog.Log(exc, ErrorSeverity.Error);
		}
	}
}
