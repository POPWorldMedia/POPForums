namespace PopForums.Services.Interfaces;

public interface ISearchIndexSubsystem
{
    void DoIndex(int topicID, string tenantID, bool isForRemoval);
    void RemoveIndex(int topicID, string tenantID);
}
