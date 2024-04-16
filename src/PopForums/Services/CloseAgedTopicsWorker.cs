namespace PopForums.Services;

public class CloseAgedTopicsWorker
{
	private static readonly object _syncRoot = new object();
	private static CloseAgedTopicsWorker _instance;

	private CloseAgedTopicsWorker()
	{
	}

	public void CloseOldTopics(ITopicService topicService, IErrorLog errorLog)
	{
		if (!Monitor.TryEnter(_syncRoot))
        {
            return;
        }

        try
		{
			topicService.CloseAgedTopics();
		}
		catch (Exception exc)
		{
			errorLog.Log(exc, ErrorSeverity.Error);
		}
		finally
		{
			Monitor.Exit(_syncRoot);
		}
	}

	public static CloseAgedTopicsWorker Instance
	{
		get
		{
			_instance ??= new CloseAgedTopicsWorker();
			return _instance;
		}
	}
}