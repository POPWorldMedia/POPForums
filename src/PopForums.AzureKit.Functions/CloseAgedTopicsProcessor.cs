using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.AzureKit.Functions
{
    public class CloseAgedTopicsProcessor
    {
	    private readonly ITopicService _topicService;
	    private readonly IServiceHeartbeatService _serviceHeartbeatService;
	    private readonly IErrorLog _errorLog;

	    public CloseAgedTopicsProcessor(ITopicService topicService, IServiceHeartbeatService serviceHeartbeatService, IErrorLog errorLog)
	    {
		    _topicService = topicService;
		    _serviceHeartbeatService = serviceHeartbeatService;
		    _errorLog = errorLog;
	    }

	    [Function("CloseAgedTopicsProcessor")]
        public async Task Run([TimerTrigger("0 0 */12 * * *")]TimerInfo myTimer, FunctionContext executionContext)
		{
			var logger = executionContext.GetLogger("AzureFunction");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			try
			{
				await _topicService.CloseAgedTopics();
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
				logger.LogError(exc, $"Exception thrown running {nameof(CloseAgedTopicsProcessor)}");
			}

			stopwatch.Stop();
			logger.LogInformation($"C# Timer {nameof(CloseAgedTopicsProcessor)} function executed ({stopwatch.ElapsedMilliseconds}ms) at: {DateTime.UtcNow}");
			await _serviceHeartbeatService.RecordHeartbeat(typeof(CloseAgedTopicsProcessor).FullName, "AzureFunction");
		}
    }
}
