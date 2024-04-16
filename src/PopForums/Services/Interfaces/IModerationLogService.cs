namespace PopForums.Services.Interfaces;

public interface IModerationLogService
{
    Task LogTopic(User user, ModerationType moderationType, Topic topic, Forum forum);

    Task LogTopic(User user, ModerationType moderationType, Topic topic, Forum forum, string comment);

    Task LogTopic(ModerationType moderationType, int topicID);

    Task LogPost(User user, ModerationType moderationType, Post post, string comment, string oldText);

    Task<List<ModerationLogEntry>> GetLog(DateTime start, DateTime end);

    Task<List<ModerationLogEntry>> GetLog(Topic topic, bool excludePostEntries);

    Task<List<ModerationLogEntry>> GetLog(Post post);
}
