﻿namespace PopForums.Repositories;

public interface ISearchIndexQueueRepository
{
	Task Enqueue(SearchIndexPayload payload);
	Task<SearchIndexPayload> Dequeue();
}