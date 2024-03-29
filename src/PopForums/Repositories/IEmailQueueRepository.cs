﻿namespace PopForums.Repositories;

public interface IEmailQueueRepository
{
	Task Enqueue(EmailQueuePayload payload);
	Task<EmailQueuePayload> Dequeue();
}