namespace PopForums.Services;

public class PostImageCleanupService : ApplicationServiceBase
{
	private IPostImageService _postImageService;
	private IErrorLog _errorLog;

	public override void Start(IServiceProvider serviceProvider)
	{
		_postImageService = serviceProvider.GetService<IPostImageService>();
		_errorLog = serviceProvider.GetService<IErrorLog>();
		base.Start(serviceProvider);
	}

	protected override void ServiceAction()
	{
		PostImageCleanupWorker.Instance.DeleteOldPostImages(_postImageService, _errorLog);
	}

	protected override int GetInterval()
	{
		return 3600; // hourly
	}
}