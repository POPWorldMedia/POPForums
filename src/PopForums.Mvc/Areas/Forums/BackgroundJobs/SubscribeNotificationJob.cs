using System.Threading;
using Microsoft.Extensions.Hosting;

namespace PopForums.Mvc.Areas.Forums.BackgroundJobs;

public class SubscribeNotificationJob(IServiceHeartbeatService serviceHeartbeatService, ISubscribeNotificationWorker subscribeNotificationWorker) : BackgroundService
{
	private const double IntervalValue = 15;
	private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(IntervalValue));
	
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			subscribeNotificationWorker.Execute();
			await serviceHeartbeatService.RecordHeartbeat(GetType().FullName, Environment.MachineName);
			await _timer.WaitForNextTickAsync(stoppingToken);
		}
	}
}