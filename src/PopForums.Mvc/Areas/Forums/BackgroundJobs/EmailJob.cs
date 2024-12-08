using System.Threading;
using Microsoft.Extensions.Hosting;

namespace PopForums.Mvc.Areas.Forums.BackgroundJobs;

public class EmailJob(ISettingsManager settingsManager, IServiceHeartbeatService serviceHeartbeatService, IEmailWorker emailWorker, IServiceProvider serviceProvider) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		PeriodicTimer timer;
		try
		{
			timer = new(TimeSpan.FromMilliseconds(settingsManager.Current.MailSendingInverval));
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				emailWorker.Execute();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				await serviceHeartbeatService.RecordHeartbeat(GetType().FullName, Environment.MachineName);
				var newTimeSpan = TimeSpan.FromMilliseconds(settingsManager.Current.MailSendingInverval);
				timer.Period = newTimeSpan;
			}
			catch (Exception ex)
			{
				var logger = await GetLogger();
				logger.LogError(ex, $"Error while executing {GetType().FullName} background job.");
			}
			await timer.WaitForNextTickAsync(stoppingToken);
		}
	}

	private async Task<ILogger<EmailJob>> GetLogger()
	{
		await using var scope = serviceProvider.CreateAsyncScope();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<EmailJob>>();
		return logger;
	}
}