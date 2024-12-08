using System.Threading;
using Microsoft.Extensions.Hosting;

namespace PopForums.Mvc.Areas.Forums.BackgroundJobs;

public class CloseAgedTopicsJob(IServiceHeartbeatService serviceHeartbeatService, ICloseAgedTopicsWorker closeAgedTopicsWorker) : BackgroundService
{
	private const int IntervalValue = 12;
	private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(IntervalValue));
	
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			closeAgedTopicsWorker.Execute();
			await serviceHeartbeatService.RecordHeartbeat(GetType().FullName, Environment.MachineName);
			await _timer.WaitForNextTickAsync(stoppingToken);
		}
	}
}