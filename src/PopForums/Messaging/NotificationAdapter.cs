using PopForums.Messaging.Models;

namespace PopForums.Messaging;

public interface INotificationAdapter
{
	Task Reply(string postName, string title, int topicID, int userID);
	Task Vote(string voterName, string title, int postID, int userID);
	Task QuestionAnswer(string askerName, string title, int postID, int userID);
	Task Award(string title, int userID);
	Task Award(string title, int userID, string tenantID);
}

public class NotificationAdapter : INotificationAdapter
{
	private readonly INotificationManager _notificationManager;

	public NotificationAdapter(INotificationManager notificationManager)
	{
		_notificationManager = notificationManager;
	}

	public async Task Reply(string postName, string title, int topicID, int userID)
	{
		var replyData = new ReplyData
		{
			PostName = postName,
			Title = title,
			TopicID = topicID
		};
		await _notificationManager.ProcessNotification(userID, NotificationType.NewReply, replyData.TopicID, replyData);
	}

	public async Task Vote(string voterName, string title, int postID, int userID)
	{
		var voteData = new VoteData
		{
			VoterName = voterName,
			Title = title,
			PostID = postID
		};
		await _notificationManager.ProcessNotification(userID, NotificationType.VoteUp, postID, voteData);
	}

	public async Task QuestionAnswer(string askerName, string title, int postID, int userID)
	{
		var questionData = new QuestionData
		{
			AskerName = askerName,
			Title = title,
			PostID = postID
		};
		await _notificationManager.ProcessNotification(userID, NotificationType.QuestionAnswered, postID, questionData);
	}

	public async Task Award(string title, int userID)
	{
		await Award(title, userID, null);
	}

	public async Task Award(string title, int userID, string tenantID)
	{
		var awardData = new AwardData
		{
			Title = title
		};
		var sequentialContext = DateTime.UtcNow.Ticks;
		await _notificationManager.ProcessNotification(userID, NotificationType.Award, sequentialContext, awardData, tenantID);
	}
}