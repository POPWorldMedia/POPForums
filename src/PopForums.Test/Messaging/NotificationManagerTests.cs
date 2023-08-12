using System.Text.Json;

namespace PopForums.Test.Messaging;

public class NotificationManagerTests
{
	protected NotificationManager GetManager()
	{
		_notificationRepository = Substitute.For<INotificationRepository>();
		_broker = Substitute.For<IBroker>();
		return new NotificationManager(_notificationRepository, _broker);
	}

	private INotificationRepository _notificationRepository;
	private IBroker _broker;

	public class ProcessNotification : NotificationManagerTests
	{
		[Fact]
		public async Task FieldsMapToUpdate()
		{
			var manager = GetManager();
			var userID = 1;
			var contextID = 2;
			var notificationType = NotificationType.NewReply;
			var data = new {a = 123, b = "xyz"};
			var unreadCount = 42;
			Notification result = null;
			_notificationRepository.UpdateNotification(Arg.Do<Notification>(x => result = x)).Returns(Task.FromResult(1));
			_notificationRepository.GetUnreadNotificationCount(userID).Returns(Task.FromResult(unreadCount));

			await manager.ProcessNotification(userID, notificationType, contextID, data);

			Assert.Equal(userID, result.UserID);
			Assert.False(result.IsRead);
			Assert.Equal(notificationType, result.NotificationType);
			Assert.Equal(contextID, result.ContextID);
			var serializedData = JsonSerializer.SerializeToElement(data);
			Assert.Equal(serializedData.ToString(), result.Data.ToString());
			Assert.Equal(unreadCount, result.UnreadCount);
		}

		[Fact]
		public async Task CreateNotCalledOnSuccessfulUpdate()
		{
			var manager = GetManager();
			_notificationRepository.UpdateNotification(Arg.Any<Notification>()).Returns(Task.FromResult(1));

			await manager.ProcessNotification(1, NotificationType.NewReply, 2, new {});
			
			await _notificationRepository.DidNotReceive().CreateNotification(Arg.Any<Notification>());
		}

		[Fact]
		public async Task FieldsMapToCreate()
		{
			var manager = GetManager();
			var userID = 1;
			var contextID = 2;
			var notificationType = NotificationType.NewReply;
			var data = new { a = 123, b = "xyz" };
			Notification result = null;
			_notificationRepository.UpdateNotification(Arg.Any<Notification>()).Returns(Task.FromResult(0));
			await _notificationRepository.CreateNotification(Arg.Do<Notification>(x => result = x));

			await manager.ProcessNotification(userID, notificationType, contextID, data);

			Assert.Equal(userID, result.UserID);
			Assert.False(result.IsRead);
			Assert.Equal(notificationType, result.NotificationType);
			Assert.Equal(contextID, result.ContextID);
			var serializedData = JsonSerializer.SerializeToElement(data);
			Assert.Equal(serializedData.ToString(), result.Data.ToString());
		}

		[Fact]
		public async Task FieldsMapToNotifyUser()
		{
			var manager = GetManager();
			var userID = 1;
			var contextID = 2;
			var notificationType = NotificationType.NewReply;
			var data = new { a = 123, b = "xyz" };
			Notification result = null;
			_notificationRepository.UpdateNotification(Arg.Any<Notification>()).Returns(Task.FromResult(1));
			_broker.NotifyUser(Arg.Do<Notification>(x => result = x));

			await manager.ProcessNotification(userID, notificationType, contextID, data);

			Assert.Equal(userID, result.UserID);
			Assert.False(result.IsRead);
			Assert.Equal(notificationType, result.NotificationType);
			Assert.Equal(contextID, result.ContextID);
			var serializedData = JsonSerializer.SerializeToElement(data);
			Assert.Equal(serializedData.ToString(), result.Data.ToString());
		}
	}

	public class GetUnreadNotificationCount : NotificationManagerTests
	{
		[Fact]
		public async Task Over100Returns100()
		{
			var manager = GetManager();
			const int userID = 123;
			_notificationRepository.GetUnreadNotificationCount(userID).Returns(Task.FromResult(101));

			var result = await manager.GetUnreadNotificationCount(userID);

			Assert.Equal(100, result);
		}

		[Fact]
		public async Task Under100ReturnsRepoValue()
		{
			var manager = GetManager();
			const int userID = 123;
			_notificationRepository.GetUnreadNotificationCount(userID).Returns(Task.FromResult(99));

			var result = await manager.GetUnreadNotificationCount(userID);

			Assert.Equal(99, result);
		}

		[Fact]
		public async Task The100Returns100()
		{
			var manager = GetManager();
			const int userID = 123;
			_notificationRepository.GetUnreadNotificationCount(userID).Returns(Task.FromResult(100));

			var result = await manager.GetUnreadNotificationCount(userID);

			Assert.Equal(100, result);
		}
	}
}