using System.Threading;
using Microsoft.Extensions.Hosting;

namespace PopForums.Mvc.Areas.Forums.BackgroundJobs;

public class UserSessionJob(IServiceHeartbeatService serviceHeartbeatService, IUserSessionWorker userSessionWorker, IServiceProvider serviceProvider) : BackgroundService
{
	private const double IntervalValue = 1;
	private readonly PeriodicTimer _timer = new(TimeSpan.FromMinutes(IntervalValue));
	
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				userSessionWorker.Execute();
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

	private async Task<ILogger<UserSessionJob>> GetLogger()
	{
		await using var scope = serviceProvider.CreateAsyncScope();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<UserSessionJob>>();
		return logger;
	}
}