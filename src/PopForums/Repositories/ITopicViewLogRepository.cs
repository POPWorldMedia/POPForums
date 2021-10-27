﻿namespace PopForums.Repositories;

public interface ITopicViewLogRepository
{
	Task Log(int? userID, int topicID, DateTime timeStamp);
}