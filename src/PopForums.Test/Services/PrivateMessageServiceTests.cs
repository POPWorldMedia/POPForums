using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
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

		[Fact]
		public void CreateNullSubjectThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Create(null, "oiahfoih", new User(), new List<User> {new User()}));
		}

		[Fact]
		public void CreateEmptySubjectThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Create(String.Empty, "oiahfoih", new User(), new List<User> { new User() }));
		}

		[Fact]
		public void CreateNullTextThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Create("wfwe", null, new User(), new List<User> { new User() }));
		}

		[Fact]
		public void CreateEmptyTextThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Create("wfwe", String.Empty, new User(), new List<User> { new User() }));
		}

		[Fact]
		public void CreateNullUserThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Create("wfwe", "oho h", null, new List<User> { new User() }));
		}

		[Fact]
		public void CreateNullToUsersThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentException>(() => service.Create("wfwe", "oho h", new User(), null));
		}

		[Fact]
		public void CreateZeroToUsersThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentException>(() => service.Create("wfwe", "oho h", new User(), new List<User>()));
		}

		[Fact]
		public void CreateAggregateUserNameSingle()
		{
			var service = GetService();
			var pm = service.Create("ohqefwwf", "oihefio", new User { UserID = 12, Name = "jeff"}, new List<User> {new User { UserID = 45, Name = "diana"}});
			Assert.Equal("jeff, diana", pm.UserNames);
		}

		[Fact]
		public void CreateAggregateUserNameMultiple()
		{
			var service = GetService();
			var pm = service.Create("ohqefwwf", "oihefio", new User { UserID = 12, Name = "jeff" }, new List<User> { new User { UserID = 45, Name = "diana" }, new User { UserID = 67, Name = "simon"} });
			Assert.Equal("jeff, diana, simon", pm.UserNames);
		}

		[Fact]
		public void CreateSubject()
		{
			var service = GetService();
			_mockTextParse.Setup(t => t.EscapeHtmlAndCensor("ohqefwwf")).Returns("ohqefwwf");
			var pm = service.Create("ohqefwwf", "oihefio", new User { UserID = 12 }, new List<User> { new User { UserID = 45 } });
			Assert.Equal("ohqefwwf", pm.Subject);
		}

		[Fact]
		public void CreatePMPersistedIDReturned()
		{
			var service = GetService();
			var persist = new PrivateMessage();
			_mockPMRepo.Setup(p => p.CreatePrivateMessage(It.IsAny<PrivateMessage>())).Returns(69).Callback<PrivateMessage>(p => persist = p);
			_mockTextParse.Setup(t => t.EscapeHtmlAndCensor("ohqefwwf")).Returns("ohqefwwf");
			var pm = service.Create("ohqefwwf", "oihefio", new User { UserID = 12, Name = "jeff" }, new List<User> { new User { UserID = 45, Name = "diana" }, new User { UserID = 67, Name = "simon"} });
			Assert.Equal(69, pm.PMID);
			Assert.Equal("ohqefwwf", persist.Subject);
			Assert.Equal("jeff, diana, simon", persist.UserNames);
		}

		[Fact]
		public void CreateAllUsersPresisted()
		{
			var user = new User { UserID = 12 };
			var to1 = new User { UserID = 45 };
			var to2 = new User { UserID = 67 };
			var service = GetService();
			var users = new List<int>();
			var originalUser = new List<int>();
			_mockPMRepo.Setup(p => p.CreatePrivateMessage(It.IsAny<PrivateMessage>())).Returns(69);
			_mockPMRepo.Setup(p => p.AddUsers(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<DateTime>(), false)).Callback<int, List<int>, DateTime, bool>((pm, u, now, isa) => users = u);
			_mockPMRepo.Setup(p => p.AddUsers(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<DateTime>(), true)).Callback<int, List<int>, DateTime, bool>((pm, u, now, isa) => originalUser = u);
			service.Create("ohqefwwf", "oihefio", user, new List<User> { to1, to2 });
			Assert.Equal(2, users.Count);
			Assert.Equal(to1.UserID, users[0]);
			Assert.Equal(to2.UserID, users[1]);
			Assert.Equal(user.UserID, originalUser[0]);
		}

		[Fact]
		public void CreatePostPersist()
		{
			var user = new User { UserID = 12, Name = "jeff" };
			var to1 = new User { UserID = 45 };
			var to2 = new User { UserID = 67 };
			var service = GetService();
			_mockPMRepo.Setup(p => p.CreatePrivateMessage(It.IsAny<PrivateMessage>())).Returns(69);
			var post = new PrivateMessagePost();
			_mockPMRepo.Setup(p => p.AddPost(It.IsAny<PrivateMessagePost>())).Callback<PrivateMessagePost>(p => post = p);
			_mockTextParse.Setup(t => t.ForumCodeToHtml("oihefio")).Returns("oihefio");
			service.Create("ohqefwwf", "oihefio", user, new List<User> { to1, to2 });
			Assert.Equal("oihefio", post.FullText);
			Assert.Equal("jeff", post.Name);
			Assert.Equal(69, post.PMID);
			Assert.Equal(user.UserID, post.UserID);
		}

		[Fact]
		public void ReplyNullPMThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentException>(() => service.Reply(null, "ohifwefhi", new User()));
		}

		[Fact]
		public void ReplyNoIdPMThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentException>(() => service.Reply(new PrivateMessage(), "ohifwefhi", new User()));
		}

		[Fact]
		public void ReplyNullTextThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Reply(new PrivateMessage{ PMID = 2 }, null, new User()));
		}

		[Fact]
		public void ReplyEmptyTextThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Reply(new PrivateMessage { PMID = 2 }, String.Empty, new User()));
		}

		[Fact]
		public void ReplyNullUserThrows()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.Reply(new PrivateMessage { PMID = 2 }, "wfwgrg", null));
		}

		[Fact]
		public void ReplyMapsAndPresistsPost()
		{
			var service = GetService();
			var post = new PrivateMessagePost();
			_mockPMRepo.Setup(p => p.AddPost(It.IsAny<PrivateMessagePost>())).Callback<PrivateMessagePost>(p => post = p);
			var user = new User { UserID = 1, Name = "jeff"};
			var pm = new PrivateMessage {PMID = 2};
			var text = "mah message";
			_mockTextParse.Setup(t => t.ForumCodeToHtml(text)).Returns(text);
			_mockPMRepo.Setup(p => p.GetUsers(pm.PMID)).Returns(new List<PrivateMessageUser> {new PrivateMessageUser {UserID = user.UserID}});
			service.Reply(pm, text, user);
			Assert.Equal(text, post.FullText);
			Assert.Equal(user.Name, post.Name);
			Assert.Equal(user.UserID, post.UserID);
			Assert.Equal(pm.PMID, post.PMID);
		}

		[Fact]
		public void ReplyThrowsIfUserIsntOnPM()
		{
			var service = GetService();
			var user = new User { UserID = 1 };
			_mockPMRepo.Setup(p => p.GetUsers(It.IsAny<int>())).Returns(new List<PrivateMessageUser> { new PrivateMessageUser { UserID = 456 } });
			Assert.Throws<Exception>(() => service.Reply(new PrivateMessage { PMID = 2 }, "wohfwo", user));
		}

		[Fact]
		public void IsUserInPMTrue()
		{
			var service = GetService();
			var user = new User { UserID = 1 };
			var pm = new PrivateMessage { PMID = 2 };
			_mockPMRepo.Setup(p => p.GetUsers(pm.PMID)).Returns(new List<PrivateMessageUser> { new PrivateMessageUser { UserID = user.UserID } });
			Assert.True(service.IsUserInPM(user, pm));
		}

		[Fact]
		public void IsUserInPMFalse()
		{
			var service = GetService();
			var user = new User { UserID = 1 };
			var pm = new PrivateMessage { PMID = 2 };
			_mockPMRepo.Setup(p => p.GetUsers(pm.PMID)).Returns(new List<PrivateMessageUser> { new PrivateMessageUser { UserID = 765 } });
			Assert.False(service.IsUserInPM(user, pm));
		}
	}
}
