using PopForums.Messaging.Models;

namespace PopForums.Test.Messaging;

public class NotificationAdapterTests
{
	protected NotificationAdapter GetAdapter()
	{
		_notificationManager = Substitute.For<INotificationManager>();
		return new NotificationAdapter(_notificationManager);
	}

	private INotificationManager _notificationManager;

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
			await _notificationManager.ProcessNotification(userID, NotificationType.NewReply, topicID, Arg.Do<ReplyData>(x => replyData = x), tenantID);

			await adapter.Reply(name, title, topicID, userID, tenantID);

			Assert.Equal(name, replyData.PostName);
			Assert.Equal(topicID, replyData.TopicID);
			Assert.Equal(title, replyData.Title);
			await _notificationManager.Received().ProcessNotification(userID, NotificationType.NewReply, topicID, Arg.Any<ReplyData>(), tenantID);
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
			await _notificationManager.ProcessNotification(userID, NotificationType.VoteUp, postID, Arg.Do<VoteData>(x => voteData = x));

			await adapter.Vote(name, title, postID, userID);

			Assert.Equal(name, voteData.VoterName);
			Assert.Equal(title, voteData.Title);
			Assert.Equal(postID, voteData.PostID);
			await _notificationManager.Received().ProcessNotification(userID, NotificationType.VoteUp, postID, Arg.Any<VoteData>());
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
			await _notificationManager.ProcessNotification(userID, NotificationType.QuestionAnswered, postID, Arg.Do<QuestionData>(x => questionData = x));

			await adapter.QuestionAnswer(askerName, title, postID, userID);

			Assert.Equal(askerName, questionData.AskerName);
			Assert.Equal(title, questionData.Title);
			Assert.Equal(postID, questionData.PostID);
			await _notificationManager.Received().ProcessNotification(userID, NotificationType.QuestionAnswered, postID, Arg.Any<QuestionData>());
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
			await _notificationManager.ProcessNotification(userID, NotificationType.Award, Arg.Any<long>(), Arg.Do<AwardData>(x => awardData = x), null);

			await adapter.Award(title, userID);
			
			await _notificationManager.Received().ProcessNotification(userID, NotificationType.Award, Arg.Any<long>(), Arg.Any<AwardData>(), null);
			Assert.Equal(title, awardData.Title);
		}
	}
}