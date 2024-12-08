using System.Threading;
using Microsoft.Extensions.Hosting;

namespace PopForums.Mvc.Areas.Forums.BackgroundJobs;

public class PostImageCleanupJob(IServiceHeartbeatService serviceHeartbeatService, IPostImageCleanupWorker postImageCleanupWorker, IServiceProvider serviceProvider) : BackgroundService
{
	private const double IntervalValue = 12;
	private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(IntervalValue));
	
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				postImageCleanupWorker.Execute();
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

	private async Task<ILogger<PostImageCleanupJob>> GetLogger()
	{
		await using var scope = serviceProvider.CreateAsyncScope();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<PostImageCleanupJob>>();
		return logger;
	}
}