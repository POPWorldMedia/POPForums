namespace PopForums.Services;

public interface ICloseAgedTopicsWorker
{
	void Execute();
}

public class CloseAgedTopicsWorker(ITopicService topicService, IErrorLog errorLog) : ICloseAgedTopicsWorker
{
	public async void Execute()
	{
		try
		{
			await topicService.CloseAgedTopics();
		}
		catch (Exception exc)
		{
			errorLog.Log(exc, ErrorSeverity.Error);
		}
	}
}