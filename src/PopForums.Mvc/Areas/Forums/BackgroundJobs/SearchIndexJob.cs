using System.Threading;
using Microsoft.Extensions.Hosting;

namespace PopForums.Mvc.Areas.Forums.BackgroundJobs;

public class SearchIndexJob(ISettingsManager settingsManager, IServiceHeartbeatService serviceHeartbeatService, ISearchIndexWorker searchIndexWorker, IServiceProvider serviceProvider) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		PeriodicTimer timer;
		try
		{
			timer = new(TimeSpan.FromMilliseconds(settingsManager.Current.SearchIndexingInterval));
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
				searchIndexWorker.Execute();
				await serviceHeartbeatService.RecordHeartbeat(GetType().FullName, Environment.MachineName);
				timer.Period = TimeSpan.FromMilliseconds(settingsManager.Current.SearchIndexingInterval);
			}
			catch (Exception ex)
			{
				var logger = await GetLogger();
				logger.LogError(ex, $"Error while executing {GetType().FullName} background job.");
			}
			await timer.WaitForNextTickAsync(stoppingToken);
		}
	}

	private async Task<ILogger<SearchIndexJob>> GetLogger()
	{
		await using var scope = serviceProvider.CreateAsyncScope();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<SearchIndexJob>>();
		return logger;
	}
}