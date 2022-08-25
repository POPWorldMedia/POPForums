using System.Text.Json;
using Org.BouncyCastle.Bcpg;

namespace PopForums.Test.Messaging;

public class NotificationManagerTests
{
	protected NotificationManager GetManager()
	{
		_notificationRepository = new Mock<INotificationRepository>();
		_broker = new Mock<IBroker>();
		return new NotificationManager(_notificationRepository.Object, _broker.Object);
	}

	private Mock<INotificationRepository> _notificationRepository;
	private Mock<IBroker> _broker;

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
			Notification result = null;
			_notificationRepository.Setup(x => x.UpdateNotification(It.IsAny<Notification>())).ReturnsAsync(1).Callback<Notification>(n => result = n);

			await manager.ProcessNotification(userID, notificationType, contextID, data);

			Assert.Equal(userID, result.UserID);
			Assert.False(result.IsRead);
			Assert.Equal(notificationType, result.NotificationType);
			Assert.Equal(contextID, result.ContextID);
			var serializedData = JsonSerializer.SerializeToElement(data);
			Assert.Equal(serializedData.ToString(), result.Data.ToString());
		}

		[Fact]
		public async Task CreateNotCalledOnSuccessfulUpdate()
		{
			var manager = GetManager();
			_notificationRepository.Setup(x => x.UpdateNotification(It.IsAny<Notification>())).ReturnsAsync(1);

			await manager.ProcessNotification(1, NotificationType.NewReply, 2, new {});
			
			_notificationRepository.Verify(x => x.CreateNotification(It.IsAny<Notification>()), Times.Never);
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
			_notificationRepository.Setup(x => x.UpdateNotification(It.IsAny<Notification>())).ReturnsAsync(0);
			_notificationRepository.Setup(x => x.CreateNotification(It.IsAny<Notification>())).Callback<Notification>(n => result = n);

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
			_notificationRepository.Setup(x => x.UpdateNotification(It.IsAny<Notification>())).ReturnsAsync(1);
			_broker.Setup(x => x.NotifyUser(It.IsAny<Notification>())).Callback<Notification>(n => result = n);

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
			_notificationRepository.Setup(x => x.GetUnreadNotificationCount(userID)).ReturnsAsync(101);

			var result = await manager.GetUnreadNotificationCount(userID);

			Assert.Equal(100, result);
		}

		[Fact]
		public async Task Under100ReturnsRepoValue()
		{
			var manager = GetManager();
			const int userID = 123;
			_notificationRepository.Setup(x => x.GetUnreadNotificationCount(userID)).ReturnsAsync(99);

			var result = await manager.GetUnreadNotificationCount(userID);

			Assert.Equal(99, result);
		}

		[Fact]
		public async Task The100Returns100()
		{
			var manager = GetManager();
			const int userID = 123;
			_notificationRepository.Setup(x => x.GetUnreadNotificationCount(userID)).ReturnsAsync(100);

			var result = await manager.GetUnreadNotificationCount(userID);

			Assert.Equal(100, result);
		}
	}
}