using PopForums.Services.Interfaces;

namespace PopForums.Services;

public class TopicViewLogService : ITopicViewLogService
{
	private readonly IConfig _config;
	private readonly ITopicViewLogRepository _topicViewLogRepository;

	public TopicViewLogService(IConfig config, ITopicViewLogRepository topicViewLogRepository)
	{
		_config = config;
		_topicViewLogRepository = topicViewLogRepository;
	}

	public async Task LogView(int? userID, int topicID)
	{
		if (!_config.LogTopicViews)
        {
            return;
        }

        var timeStamp = DateTime.UtcNow;

		await _topicViewLogRepository.Log(userID, topicID, timeStamp);
	}
}