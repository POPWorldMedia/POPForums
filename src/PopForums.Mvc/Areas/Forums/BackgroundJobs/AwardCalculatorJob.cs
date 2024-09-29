using System.Threading;
using Microsoft.Extensions.Hosting;

namespace PopForums.Mvc.Areas.Forums.BackgroundJobs;

public class AwardCalculatorJob(ISettingsManager settingsManager, IServiceHeartbeatService serviceHeartbeatService, IAwardCalculatorWorker awardCalculatorWorker) : BackgroundService
{
	private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(settingsManager.Current.ScoringGameCalculatorInterval));
	
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			awardCalculatorWorker.Execute();
			await serviceHeartbeatService.RecordHeartbeat(GetType().FullName, Environment.MachineName);
			_timer.Period = TimeSpan.FromMilliseconds(settingsManager.Current.ScoringGameCalculatorInterval);
			await _timer.WaitForNextTickAsync(stoppingToken);
		}
	}
}