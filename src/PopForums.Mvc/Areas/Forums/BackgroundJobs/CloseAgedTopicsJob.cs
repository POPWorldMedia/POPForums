using System.Threading;
using Microsoft.Extensions.Hosting;

namespace PopForums.Mvc.Areas.Forums.BackgroundJobs;

public class CloseAgedTopicsJob(IServiceHeartbeatService serviceHeartbeatService, ICloseAgedTopicsWorker closeAgedTopicsWorker, IServiceProvider serviceProvider) : BackgroundService
{
	private const int IntervalValue = 12;
	private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(IntervalValue));
	
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				closeAgedTopicsWorker.Execute();
				await serviceHeartbeatService.RecordHeartbeat(GetType().FullName, Environment.MachineName);
			}
			catch (Exception ex)
			{
				var logger = await GetLogger();
				logger.LogError(ex, $"Error while executing {GetType().FullName} background job.");
			}
			await _timer.WaitForNextTickAsync(stoppingToken);
		}
	}

	private async Task<ILogger<CloseAgedTopicsJob>> GetLogger()
	{
		await using var scope = serviceProvider.CreateAsyncScope();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<CloseAgedTopicsJob>>();
		return logger;
	}
}