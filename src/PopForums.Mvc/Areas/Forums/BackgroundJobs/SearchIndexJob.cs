using System.Threading;
using Microsoft.Extensions.Hosting;

namespace PopForums.Mvc.Areas.Forums.BackgroundJobs;

public class SearchIndexJob(ISettingsManager settingsManager, IServiceHeartbeatService serviceHeartbeatService, ISearchIndexWorker searchIndexWorker) : BackgroundService
{
	private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(settingsManager.Current.SearchIndexingInterval));
	
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			searchIndexWorker.Execute();
			await serviceHeartbeatService.RecordHeartbeat(GetType().FullName, Environment.MachineName);
			_timer.Period = TimeSpan.FromMilliseconds(settingsManager.Current.SearchIndexingInterval);
			await _timer.WaitForNextTickAsync(stoppingToken);
		}
	}
}