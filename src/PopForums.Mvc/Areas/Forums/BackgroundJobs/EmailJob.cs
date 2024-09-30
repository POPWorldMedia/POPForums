using System.Threading;
using Microsoft.Extensions.Hosting;

namespace PopForums.Mvc.Areas.Forums.BackgroundJobs;

public class EmailJob(ISettingsManager settingsManager, IServiceHeartbeatService serviceHeartbeatService, IEmailWorker emailWorker) : BackgroundService
{
	private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(settingsManager.Current.MailSendingInverval));
	
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			emailWorker.Execute();
			await serviceHeartbeatService.RecordHeartbeat(GetType().FullName, Environment.MachineName);
			var newTimeSpan = TimeSpan.FromMilliseconds(settingsManager.Current.MailSendingInverval);
			_timer.Period = newTimeSpan;
			await _timer.WaitForNextTickAsync(stoppingToken);
		}
	}
}