using Microsoft.Azure.Functions.Worker;
using PopForums.Configuration;
using PopForums.Services;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace PopForums.AzureKit.Functions;

public class PostImageCleanupProcessor
{
	private readonly IPostImageService _postImageService;
	private readonly IServiceHeartbeatService _serviceHeartbeatService;
	private readonly IErrorLog _errorLog;

	public PostImageCleanupProcessor(IPostImageService postImageService, IServiceHeartbeatService serviceHeartbeatService, IErrorLog errorLog)
	{
		_postImageService = postImageService;
		_serviceHeartbeatService = serviceHeartbeatService;
		_errorLog = errorLog;
	}

	[Function("PostImageCleanupProcessor")]
	public async Task Run([TimerTrigger("0 5 */1 * * *")] TimerInfo myTimer, FunctionContext executionContext)
	{
		var logger = executionContext.GetLogger("AzureFunction");
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		try
		{
			await _postImageService.DeleteOldPostImages();
		}
		catch (Exception exc)
		{
			_errorLog.Log(exc, ErrorSeverity.Error);
			logger.LogError(exc, $"Exception thrown running {nameof(PostImageCleanupProcessor)}");
		}

		stopwatch.Stop();
		logger.LogInformation($"C# Timer {nameof(PostImageCleanupProcessor)} function executed ({stopwatch.ElapsedMilliseconds}ms) at: {DateTime.UtcNow}");
		await _serviceHeartbeatService.RecordHeartbeat(typeof(PostImageCleanupProcessor).FullName, "AzureFunction");
	}
}