﻿namespace PopForums.Services;

public class CloseAgedTopicsApplicationService : ApplicationServiceBase
{
	private ITopicService _topicService;
	private IErrorLog _errorLog;

	public override void Start(IServiceProvider serviceProvider)
	{
		_topicService = serviceProvider.GetService<ITopicService>();
		_errorLog = serviceProvider.GetService<IErrorLog>();
		base.Start(serviceProvider);
	}

	protected override void ServiceAction()
	{
		CloseAgedTopicsWorker.Instance.CloseOldTopics(_topicService, _errorLog);
	}

	protected override int GetInterval()
	{
		return 43200; // 12 hours
	}
}