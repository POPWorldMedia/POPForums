namespace PopForums.Services;

public class PostImageCleanupService : ApplicationServiceBase
{
	public override void Start(IServiceProvider serviceProvider)
	{
		_postImageService = serviceProvider.GetService<IPostImageService>();
		_errorLog = serviceProvider.GetService<IErrorLog>();
		base.Start(serviceProvider);
	}

	private IPostImageService _postImageService;
	private IErrorLog _errorLog;

	protected override void ServiceAction()
	{
		PostImageCleanupWorker.Instance.DeleteOldPostImages(_postImageService, _errorLog);
	}

	protected override int GetInterval()
	{
		return 3600; // hourly
	}
}