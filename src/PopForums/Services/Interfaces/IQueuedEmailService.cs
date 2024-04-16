namespace PopForums.Services.Interfaces;

public interface IQueuedEmailService
{
    Task CreateAndQueueEmail(QueuedEmailMessage queuedEmailMessage);
}
