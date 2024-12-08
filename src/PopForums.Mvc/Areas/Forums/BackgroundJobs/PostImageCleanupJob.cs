using System.Threading;
using Microsoft.Extensions.Hosting;

namespace PopForums.Mvc.Areas.Forums.BackgroundJobs;

public class PostImageCleanupJob(IServiceHeartbeatService serviceHeartbeatService, IPostImageCleanupWorker postImageCleanupWorker) : BackgroundService
{
	private const double IntervalValue = 12;
	private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(IntervalValue));
	
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			postImageCleanupWorker.Execute();
			await serviceHeartbeatService.RecordHeartbeat(GetType().FullName, Environment.MachineName);
			await _timer.WaitForNextTickAsync(stoppingToken);
		}
	}
}