using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
	[TestFixture]
	public class PrivateMessageServiceTests
	{
		private PrivateMessageService GetService()
		{
			_mockPMRepo = new Mock<IPrivateMessageRepository>();
			_mockSettings = new Mock<ISettingsManager>();
			_mockTextParse = new Mock<ITextParsingService>();
			var service = new PrivateMessageService(_mockPMRepo.Object, _mockSettings.Object, _mockTextParse.Object);
			return service;
		}

		private Mock<IPrivateMessageRepository> _mockPMRepo;
		private Mock<ISettingsManager> _mockSettings;
		private Mock<ITextParsingService> _mockTextParse;

		[Test]
		public void CreateNullSubjectThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Create(null, "oiahfoih", new User(12, DateTime.MinValue), new List<User> {new User(45, DateTime.MaxValue)}));
		}

		[Test]
		public void CreateEmptySubjectThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Create(String.Empty, "oiahfoih", new User(12, DateTime.MinValue), new List<User> { new User(45, DateTime.MaxValue) }));
		}

		[Test]
		public void CreateNullTextThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Create("wfwe", null, new User(12, DateTime.MinValue), new List<User> { new User(45, DateTime.MaxValue) }));
		}

		[Test]
		public void CreateEmptyTextThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Create("wfwe", String.Empty, new User(12, DateTime.MinValue), new List<User> { new User(45, DateTime.MaxValue) }));
		}

		[Test]
		public void CreateNullUserThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Create("wfwe", "oho h", null, new List<User> { new User(45, DateTime.MaxValue) }));
		}

		[Test]
		public void CreateNullToUsersThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentException>(() => service.Create("wfwe", "oho h", new User(12, DateTime.MinValue), null));
		}

		[Test]
		public void CreateZeroToUsersThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentException>(() => service.Create("wfwe", "oho h", new User(12, DateTime.MinValue), new List<User>()));
		}

		[Test]
		public void CreateAggregateUserNameSingle()
		{
			var service = GetService();
			var pm = service.Create("ohqefwwf", "oihefio", new User(12, DateTime.MinValue) {Name = "jeff"}, new List<User> {new User(45, DateTime.MinValue) {Name = "diana"}});
			Assert.AreEqual("jeff, diana", pm.UserNames);
		}

		[Test]
		public void CreateAggregateUserNameMultiple()
		{
			var service = GetService();
			var pm = service.Create("ohqefwwf", "oihefio", new User(12, DateTime.MinValue) { Name = "jeff" }, new List<User> { new User(45, DateTime.MinValue) { Name = "diana" }, new User(67, DateTime.MinValue) { Name = "simon"} });
			Assert.AreEqual("jeff, diana, simon", pm.UserNames);
		}

		[Test]
		public void CreateSubject()
		{
			var service = GetService();
			_mockTextParse.Setup(t => t.EscapeHtmlAndCensor("ohqefwwf")).Returns("ohqefwwf");
			var pm = service.Create("ohqefwwf", "oihefio", new User(12, DateTime.MinValue), new List<User> { new User(45, DateTime.MinValue) });
			Assert.AreEqual("ohqefwwf", pm.Subject);
		}

		[Test]
		public void CreatePMPersistedIDReturned()
		{
			var service = GetService();
			var persist = new PrivateMessage();
			_mockPMRepo.Setup(p => p.CreatePrivateMessage(It.IsAny<PrivateMessage>())).Returns(69).Callback<PrivateMessage>(p => persist = p);
			_mockTextParse.Setup(t => t.EscapeHtmlAndCensor("ohqefwwf")).Returns("ohqefwwf");
			var pm = service.Create("ohqefwwf", "oihefio", new User(12, DateTime.MinValue) { Name = "jeff" }, new List<User> { new User(45, DateTime.MinValue) { Name = "diana" }, new User(67, DateTime.MinValue) { Name = "simon"} });
			Assert.AreEqual(69, pm.PMID);
			Assert.AreEqual("ohqefwwf", persist.Subject);
			Assert.AreEqual("jeff, diana, simon", persist.UserNames);
		}

		[Test]
		public void CreateAllUsersPresisted()
		{
			var user = new User(12, DateTime.MinValue);
			var to1 = new User(45, DateTime.MinValue);
			var to2 = new User(67, DateTime.MinValue);
			var service = GetService();
			var users = new List<int>();
			var originalUser = new List<int>();
			_mockPMRepo.Setup(p => p.CreatePrivateMessage(It.IsAny<PrivateMessage>())).Returns(69);
			_mockPMRepo.Setup(p => p.AddUsers(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<DateTime>(), false)).Callback<int, List<int>, DateTime, bool>((pm, u, now, isa) => users = u);
			_mockPMRepo.Setup(p => p.AddUsers(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<DateTime>(), true)).Callback<int, List<int>, DateTime, bool>((pm, u, now, isa) => originalUser = u);
			service.Create("ohqefwwf", "oihefio", user, new List<User> { to1, to2 });
			Assert.AreEqual(2, users.Count);
			Assert.AreEqual(to1.UserID, users[0]);
			Assert.AreEqual(to2.UserID, users[1]);
			Assert.AreEqual(user.UserID, originalUser[0]);
		}

		[Test]
		public void CreatePostPersist()
		{
			var user = new User(12, DateTime.MinValue) { Name = "jeff" };
			var to1 = new User(45, DateTime.MinValue);
			var to2 = new User(67, DateTime.MinValue);
			var service = GetService();
			_mockPMRepo.Setup(p => p.CreatePrivateMessage(It.IsAny<PrivateMessage>())).Returns(69);
			var post = new PrivateMessagePost();
			_mockPMRepo.Setup(p => p.AddPost(It.IsAny<PrivateMessagePost>())).Callback<PrivateMessagePost>(p => post = p);
			_mockTextParse.Setup(t => t.ForumCodeToHtml("oihefio")).Returns("oihefio");
			service.Create("ohqefwwf", "oihefio", user, new List<User> { to1, to2 });
			Assert.AreEqual("oihefio", post.FullText);
			Assert.AreEqual("jeff", post.Name);
			Assert.AreEqual(69, post.PMID);
			Assert.AreEqual(user.UserID, post.UserID);
		}

		[Test]
		public void ReplyNullPMThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentException>(() => service.Reply(null, "ohifwefhi", new User(1, DateTime.MinValue)));
		}

		[Test]
		public void ReplyNoIdPMThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentException>(() => service.Reply(new PrivateMessage(), "ohifwefhi", new User(1, DateTime.MinValue)));
		}

		[Test]
		public void ReplyNullTextThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Reply(new PrivateMessage{ PMID = 2 }, null, new User(1, DateTime.MinValue)));
		}

		[Test]
		public void ReplyEmptyTextThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Reply(new PrivateMessage { PMID = 2 }, String.Empty, new User(1, DateTime.MinValue)));
		}

		[Test]
		public void ReplyNullUserThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Reply(new PrivateMessage { PMID = 2 }, "wfwgrg", null));
		}

		[Test]
		public void ReplyMapsAndPresistsPost()
		{
			var service = GetService();
			var post = new PrivateMessagePost();
			_mockPMRepo.Setup(p => p.AddPost(It.IsAny<PrivateMessagePost>())).Callback<PrivateMessagePost>(p => post = p);
			var user = new User(1, DateTime.MinValue) {Name = "jeff"};
			var pm = new PrivateMessage {PMID = 2};
			var text = "mah message";
			_mockTextParse.Setup(t => t.ForumCodeToHtml(text)).Returns(text);
			_mockPMRepo.Setup(p => p.GetUsers(pm.PMID)).Returns(new List<PrivateMessageUser> {new PrivateMessageUser {UserID = user.UserID}});
			service.Reply(pm, text, user);
			Assert.AreEqual(text, post.FullText);
			Assert.AreEqual(user.Name, post.Name);
			Assert.AreEqual(user.UserID, post.UserID);
			Assert.AreEqual(pm.PMID, post.PMID);
		}

		[Test]
		public void ReplyThrowsIfUserIsntOnPM()
		{
			var service = GetService();
			var user = new User(1, DateTime.MinValue);
			_mockPMRepo.Setup(p => p.GetUsers(It.IsAny<int>())).Returns(new List<PrivateMessageUser> { new PrivateMessageUser { UserID = 456 } });
			Assert.Throws<Exception>(() => service.Reply(new PrivateMessage { PMID = 2 }, "wohfwo", user));
		}

		[Test]
		public void IsUserInPMTrue()
		{
			var service = GetService();
			var user = new User(1, DateTime.MinValue);
			var pm = new PrivateMessage { PMID = 2 };
			_mockPMRepo.Setup(p => p.GetUsers(pm.PMID)).Returns(new List<PrivateMessageUser> { new PrivateMessageUser { UserID = user.UserID } });
			Assert.True(service.IsUserInPM(user, pm));
		}

		[Test]
		public void IsUserInPMFalse()
		{
			var service = GetService();
			var user = new User(1, DateTime.MinValue);
			var pm = new PrivateMessage { PMID = 2 };
			_mockPMRepo.Setup(p => p.GetUsers(pm.PMID)).Returns(new List<PrivateMessageUser> { new PrivateMessageUser { UserID = 765 } });
			Assert.False(service.IsUserInPM(user, pm));
		}
	}
}
