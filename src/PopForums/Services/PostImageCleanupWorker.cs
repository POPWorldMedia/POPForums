namespace PopForums.Services;

public interface IPostImageCleanupWorker
{
	void Execute();
}

public class PostImageCleanupWorker(IPostImageService postImageService, IErrorLog errorLog) : IPostImageCleanupWorker
{
	public async void Execute()
	{
		try
		{
			await postImageService.DeleteOldPostImages();
		}
		catch (Exception exc)
		{
			errorLog.Log(exc, ErrorSeverity.Error);
		}
	}
}