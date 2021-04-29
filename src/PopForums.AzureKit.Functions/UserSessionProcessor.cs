using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.AzureKit.Functions
{
    public class UserSessionProcessor
    {
	    private readonly IUserSessionService _userSessionService;
	    private readonly IServiceHeartbeatService _serviceHeartbeatService;
	    private readonly IErrorLog _errorLog;

	    public UserSessionProcessor(IUserSessionService userSessionService, IServiceHeartbeatService serviceHeartbeatService, IErrorLog errorLog)
	    {
		    _userSessionService = userSessionService;
		    _serviceHeartbeatService = serviceHeartbeatService;
		    _errorLog = errorLog;
	    }

	    [Function("UserSessionProcessor")]
        public async Task RunAsync([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, FunctionContext executionContext)
        {
	        var logger = executionContext.GetLogger("AzureFunction");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			try
			{
				await _userSessionService.CleanUpExpiredSessions();
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
				logger.LogError(exc, $"Exception thrown running {nameof(UserSessionProcessor)}");
			}

			stopwatch.Stop();
			logger.LogInformation($"C# Timer {nameof(UserSessionProcessor)} function executed ({stopwatch.ElapsedMilliseconds}ms) at: {DateTime.UtcNow}");
            await _serviceHeartbeatService.RecordHeartbeat(typeof(UserSessionProcessor).FullName, "AzureFunction");
		}
    }
}
