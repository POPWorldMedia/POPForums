using System.Threading;
using Microsoft.Extensions.Hosting;

namespace PopForums.Mvc.Areas.Forums.BackgroundJobs;

public class RenewalEnqueueJob(IServiceHeartbeatService serviceHeartbeatService, IRenewalOrchestrationService renewalOrchestrationService, IRenewalEnqueueClaimRepository renewalEnqueueClaimRepository, IServiceProvider serviceProvider) : BackgroundService
{
	private static readonly TimeSpan TargetTimeOfDayUtc = new(12, 1, 0);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await Task.Delay(GetDelayUntilNextRun(), stoppingToken);
				if (stoppingToken.IsCancellationRequested)
					break;
				var today = DateOnly.FromDateTime(DateTime.UtcNow);
				if (await renewalEnqueueClaimRepository.TryClaim(today))
					await renewalOrchestrationService.EnqueueTenantsForRenewal();
				await serviceHeartbeatService.RecordHeartbeat(GetType().FullName, Environment.MachineName);
			}
			catch (Exception ex)
			{
				var logger = await GetLogger();
				logger.LogError(ex, $"Error while executing {GetType().FullName} background job.");
			}
		}
	}

	private static TimeSpan GetDelayUntilNextRun()
	{
		var now = DateTime.UtcNow;
		var next = now.Date + TargetTimeOfDayUtc;
		if (next <= now)
			next = next.AddDays(1);
		return next - now;
	}

	private async Task<ILogger<RenewalEnqueueJob>> GetLogger()
	{
		await using var scope = serviceProvider.CreateAsyncScope();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<RenewalEnqueueJob>>();
		return logger;
	}
}
