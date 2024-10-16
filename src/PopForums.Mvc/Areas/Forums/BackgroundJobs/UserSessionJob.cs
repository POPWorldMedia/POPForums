using System.Threading;
using Microsoft.Extensions.Hosting;

namespace PopForums.Mvc.Areas.Forums.BackgroundJobs;

public class UserSessionJob(IServiceHeartbeatService serviceHeartbeatService, IUserSessionWorker userSessionWorker) : BackgroundService
{
	private const double IntervalValue = 10000;
	private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(IntervalValue));
	
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			userSessionWorker.Execute();
			await serviceHeartbeatService.RecordHeartbeat(GetType().FullName, Environment.MachineName);
			await _timer.WaitForNextTickAsync(stoppingToken);
		}
	}
}