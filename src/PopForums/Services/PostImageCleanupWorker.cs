namespace PopForums.Services;

public class PostImageCleanupWorker
{
	private static readonly object SyncRoot = new object();
	private static PostImageCleanupWorker _instance;

	private PostImageCleanupWorker()
	{
	}

	public void DeleteOldPostImages(IPostImageService postImageService, IErrorLog errorLog)
	{
		if (!Monitor.TryEnter(SyncRoot))
		{
			return;
		}

		try
		{
			postImageService.DeleteOldPostImages();
		}
		catch (Exception exc)
		{
			errorLog.Log(exc, ErrorSeverity.Error);
		}
		finally
		{
			Monitor.Exit(SyncRoot);
		}
	}

	public static PostImageCleanupWorker Instance
	{
		get
		{
			_instance ??= new PostImageCleanupWorker();
			return _instance;
		}
	}
}