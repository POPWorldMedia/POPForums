using System.Threading;
using Microsoft.Extensions.Hosting;

namespace PopForums.Mvc.Areas.Forums.BackgroundJobs;

public class RenewalJob(ISettingsManager settingsManager, IServiceHeartbeatService serviceHeartbeatService, IRenewalWorker renewalWorker, IServiceProvider serviceProvider) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		PeriodicTimer timer;
		try
		{
			timer = new(TimeSpan.FromMilliseconds(settingsManager.Current.RenewalWorkerInterval));
		}
		catch(Exception ex)
		{
			var logger = await GetLogger();
			logger.LogError(ex, $"Error while executing {GetType().FullName} background job. This job will not restart without restarting the app.");
			return;
		}

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				renewalWorker.Execute();
				await serviceHeartbeatService.RecordHeartbeat(GetType().FullName, Environment.MachineName);
				timer.Period = TimeSpan.FromMilliseconds(settingsManager.Current.RenewalWorkerInterval);
			}
			catch (Exception ex)
			{
				var logger = await GetLogger();
				logger.LogError(ex, $"Error while executing {GetType().FullName} background job.");
			}
			await timer.WaitForNextTickAsync(stoppingToken);
		}
	}

	private async Task<ILogger<RenewalJob>> GetLogger()
	{
		await using var scope = serviceProvider.CreateAsyncScope();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<RenewalJob>>();
		return logger;
	}
}
