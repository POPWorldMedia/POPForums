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
			ReplyData replyData = null;
			_notificationManager.Setup(x => x.ProcessNotification(userID, NotificationType.NewReply, topicID, It.IsAny<ReplyData>())).Callback<int, NotificationType, int, dynamic>(((i, type, arg3, arg4) => replyData = arg4));

			await adapter.Reply(name, title, topicID, userID);

			Assert.Equal(name, replyData.PostName);
			Assert.Equal(topicID, replyData.TopicID);
			Assert.Equal(title, replyData.Title);
			_notificationManager.Verify(x => x.ProcessNotification(userID, NotificationType.NewReply, topicID, It.IsAny<ReplyData>()), Times.Once);
		}
	}
}