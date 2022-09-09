using PopForums.Messaging.Models;

namespace PopForums.Test.Messaging;

public class NotificationAdapterTests
{
	protected NotificationAdapter GetAdapter()
	{
		_notificationManager = new Mock<INotificationManager>();
		return new NotificationAdapter(_notificationManager.Object);
	}

	private Mock<INotificationManager> _notificationManager;

	public class Reply : NotificationAdapterTests
	{
		[Fact]
		public async Task ManagerCalledWithCorrectValues()
		{
			var adapter = GetAdapter();
			var name = "Jeff";
			var title = "The Topic";
			var topicID = 123;
			var userID = 456;
			var tenantID = "cb";
			ReplyData replyData = null;
			_notificationManager.Setup(x => x.ProcessNotification(userID, NotificationType.NewReply, topicID, It.IsAny<ReplyData>(), tenantID)).Callback<int, NotificationType, long, dynamic, string>(((i, type, arg3, arg4, arg5) => replyData = arg4));

			await adapter.Reply(name, title, topicID, userID, tenantID);

			Assert.Equal(name, replyData.PostName);
			Assert.Equal(topicID, replyData.TopicID);
			Assert.Equal(title, replyData.Title);
			_notificationManager.Verify(x => x.ProcessNotification(userID, NotificationType.NewReply, topicID, It.IsAny<ReplyData>(), tenantID), Times.Once);
		}
	}

	public class Vote : NotificationAdapterTests
	{
		[Fact]
		public async Task ManagerCalledWithCorrectValues()
		{
			var adapter = GetAdapter();
			var name = "Jeff";
			var title = "The Topic";
			var postID = 123;
			var userID = 456;
			VoteData voteData = null;
			_notificationManager.Setup(x => x.ProcessNotification(userID, NotificationType.VoteUp, postID, It.IsAny<VoteData>())).Callback<int, NotificationType, long, dynamic>(((i, type, arg3, arg4) => voteData = arg4));

			await adapter.Vote(name, title, postID, userID);

			Assert.Equal(name, voteData.VoterName);
			Assert.Equal(title, voteData.Title);
			Assert.Equal(postID, voteData.PostID);
			_notificationManager.Verify(x => x.ProcessNotification(userID, NotificationType.VoteUp, postID, It.IsAny<VoteData>()), Times.Once);
		}
	}

	public class QuestionAnswer : NotificationAdapterTests
	{
		[Fact]
		public async Task ManagerCalledWithCorrectValues()
		{
			var adapter = GetAdapter();
			var askerName = "Jeff";
			var title = "The Topic";
			var postID = 123;
			var userID = 456;
			QuestionData questionData = null;
			_notificationManager.Setup(x => x.ProcessNotification(userID, NotificationType.QuestionAnswered, postID, It.IsAny<QuestionData>())).Callback<int, NotificationType, long, dynamic>(((i, type, arg3, arg4) => questionData = arg4));

			await adapter.QuestionAnswer(askerName, title, postID, userID);

			Assert.Equal(askerName, questionData.AskerName);
			Assert.Equal(title, questionData.Title);
			Assert.Equal(postID, questionData.PostID);
			_notificationManager.Verify(x => x.ProcessNotification(userID, NotificationType.QuestionAnswered, postID, It.IsAny<QuestionData>()), Times.Once);
		}
	}

	public class Award : NotificationAdapterTests
	{
		[Fact]
		public async Task ManagerCalledWithCorrectValues()
		{
			var adapter = GetAdapter();
			var title = "The Award";
			var userID = 456;
			AwardData awardData = null;
			_notificationManager.Setup(x => x.ProcessNotification(userID, NotificationType.Award, It.IsAny<long>(), It.IsAny<AwardData>(), null)).Callback<int, NotificationType, long, dynamic, string>(((i, type, arg3, arg4, arg5) => awardData = arg4));

			await adapter.Award(title, userID);
			
			_notificationManager.Verify(x => x.ProcessNotification(userID, NotificationType.Award, It.IsAny<long>(), It.IsAny<AwardData>(), null), Times.Once);
			Assert.Equal(title, awardData.Title);
		}
	}
}