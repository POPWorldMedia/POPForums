using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
	public class LastReadServiceTests
	{
		private LastReadService GetService()
		{
			_lastReadRepo = new Mock<ILastReadRepository>();
			_postRepo = new Mock<IPostRepository>();
			return new LastReadService(_lastReadRepo.Object, _postRepo.Object);
		}

		private Mock<ILastReadRepository> _lastReadRepo;
		private Mock<IPostRepository> _postRepo;

		[Fact]
		public void MarkForumReadSetsReadTime()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 123 };
			var user = new User { UserID = 456 };
			service.MarkForumRead(user, forum);
			_lastReadRepo.Verify(l => l.SetForumRead(user.UserID, forum.ForumID, It.IsAny<DateTime>()), Times.Exactly(1));
		}

		[Fact]
		public void MarkForumReadDeletesOldTopicReadTimes()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 123 };
			var user = new User { UserID = 456 };
			service.MarkForumRead(user, forum);
			_lastReadRepo.Verify(l => l.DeleteTopicReadsInForum(user.UserID, forum.ForumID), Times.Exactly(1));
		}

		[Fact]
		public void MarkTopicReadThrowsWithoutUser()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.MarkTopicRead(null, new Topic { TopicID = 1 }));
		}

		[Fact]
		public void MarkTopicReadThrowsWithoutTopic()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.MarkTopicRead(new User(), null));
		}

		[Fact]
		public void MarkAllForumReadThrowsWithoutUser()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.MarkAllForumsRead(null));
		}

		[Fact]
		public void MarkForumReadThrowsWithoutUser()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.MarkForumRead(null, new Forum { ForumID = 1 }));
		}

		[Fact]
		public void MarkForumReadThrowsWithoutForum()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.MarkForumRead(new User(), null));
		}

		[Fact]
		public void MarkAllForumReadSetsReadTimes()
		{
			var service = GetService();
			var user = new User { UserID = 456 };
			service.MarkAllForumsRead(user);
			_lastReadRepo.Verify(l => l.SetAllForumsRead(user.UserID, It.IsAny<DateTime>()), Times.Exactly(1));
		}

		[Fact]
		public void MarkAllForumReadDeletesAllOldTopicReadTimes()
		{
			var service = GetService();
			var user = new User { UserID = 456 };
			service.MarkAllForumsRead(user);
			_lastReadRepo.Verify(l => l.DeleteAllTopicReads(user.UserID), Times.Exactly(1));
		}

		[Fact]
		public void ForumReadStatusForNoUser()
		{
			var service = GetService();
			var forum1 = new Forum { ForumID = 1 };
			var forum2 = new Forum { ForumID = 2, IsArchived = true };
			var forum3 = new Forum { ForumID = 3 };
			var container = new CategorizedForumContainer(new List<Category>(), new[] { forum1, forum2, forum3 });
			service.GetForumReadStatus(null, container);
			Assert.Equal(3, container.ReadStatusLookup.Count);
			Assert.Equal(ReadStatus.NoNewPosts, container.ReadStatusLookup[1]);
			Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Closed, container.ReadStatusLookup[2]);
			Assert.Equal(ReadStatus.NoNewPosts, container.ReadStatusLookup[3]);
		}

		[Fact]
		public void ForumReadStatusUserNewPosts()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) };
			var user = new User { UserID = 2 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(2)).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } });
			var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
			service.GetForumReadStatus(user, container);
			Assert.Single(container.ReadStatusLookup);
			Assert.Equal(ReadStatus.NewPosts, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void ForumReadStatusUserNewPostsButNoTopicRecords()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) };
			var user = new User { UserID = 2 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(2)).Returns(new Dictionary<int, DateTime>());
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForum(user.UserID, forum.ForumID)).Returns(new DateTime(2000, 1, 1, 3, 0, 0));
			var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
			service.GetForumReadStatus(user, container);
			Assert.Single(container.ReadStatusLookup);
			Assert.Equal(ReadStatus.NewPosts, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void ForumReadStatusUserNewPostsNoLastReadRecords()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) };
			var user = new User { UserID = 2 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(2)).Returns(new Dictionary<int, DateTime>());
			var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
			service.GetForumReadStatus(user, container);
			Assert.Single(container.ReadStatusLookup);
			Assert.Equal(ReadStatus.NewPosts, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void ForumReadStatusUserNoNewPosts()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) };
			var user = new User { UserID = 2 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(2)).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 7, 0, 0) } });
			var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
			service.GetForumReadStatus(user, container);
			Assert.Single(container.ReadStatusLookup);
			Assert.Equal(ReadStatus.NoNewPosts, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void ForumReadStatusUserNewPostsArchived()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0), IsArchived = true };
			var user = new User { UserID = 2 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(2)).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } });
			var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
			service.GetForumReadStatus(user, container);
			Assert.Single(container.ReadStatusLookup);
			Assert.Equal(ReadStatus.NewPosts | ReadStatus.Closed, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void ForumReadStatusUserNoNewPostsArchived()
		{
			var service = GetService();
			var forum = new Forum { ForumID = 1, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0), IsArchived = true };
			var user = new User { UserID = 2 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(2)).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 7, 0, 0) } });
			var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
			service.GetForumReadStatus(user, container);
			Assert.Single(container.ReadStatusLookup);
			Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Closed, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusForNoUser()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> {new Topic { TopicID = 1 }, new Topic { TopicID = 2, IsClosed = true}, new Topic { TopicID = 3, IsPinned = true}};
			service.GetTopicReadStatus(null, container);
			Assert.Equal(3, container.ReadStatusLookup.Count);
			Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
			Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Closed | ReadStatus.NotPinned, container.ReadStatusLookup[2]);
			Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.Pinned, container.ReadStatusLookup[3]);
		}

		[Fact]
		public void TopicReadStatusWithUserNewNoForumRecordNoTopicRecord()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime>());
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserNewNoForumRecordWithTopicRecord()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime>());
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } });
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserNewWithForumRecordNoTopicRecord()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 2, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserNewWithForumRecordWithTopicRecord()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 2, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } });
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserNotNewWithForumRecordNoTopicRecord()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserNotNewNoForumRecordWithTopicRecord()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime>());
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 7, 0, 0) } });
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserNotNewWithForumRecordWithTopicRecordForumNewer()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } });
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserNotNewWithForumRecordWithTopicRecordTopicNewer()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 7, 0, 0) } });
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserOpenNewPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, IsPinned = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new [] {1})).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.Pinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserOpenNewNotPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserOpenNotNewPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, IsPinned = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.Pinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserOpenNotNewNotPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserClosedNewPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, IsClosed = true, IsPinned = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NewPosts | ReadStatus.Closed | ReadStatus.Pinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserClosedNewNotPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, IsClosed = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NewPosts | ReadStatus.Closed | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserClosedNoNewPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, IsClosed = true, IsPinned = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Closed | ReadStatus.Pinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void TopicReadStatusWithUserClosedNoNewNotPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, IsClosed = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User { UserID = 123 };
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Closed | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Fact]
		public void MarkTopicReadCallsRepo()
		{
			var service = GetService();
			var user = new User { UserID = 1 };
			var topic = new Topic { TopicID = 2 };
			service.MarkTopicRead(user, topic);
			_lastReadRepo.Verify(l => l.SetTopicRead(user.UserID, topic.TopicID, It.IsAny<DateTime>()), Times.Exactly(1));
		}

		[Fact]
		public void GetLastReadTimeReturnsTopicTimeWhenAvailable()
		{
			var service = GetService();
			var user = new User { UserID = 1 };
			var topic = new Topic { TopicID = 2 };
			var lastRead = new DateTime(2010, 1, 1);
			_lastReadRepo.Setup(x => x.GetLastReadTimeForTopic(user.UserID, topic.TopicID)).Returns(lastRead);

			var result = service.GetLastReadTime(user, topic);

			Assert.Equal(lastRead, result);
			_lastReadRepo.Verify(x => x.GetLastReadTimesForForum(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
		}

		[Fact]
		public void GetLastReadTimeReturnsForumTimeWhenNoTopicTimeAvailable()
		{
			var service = GetService();
			var user = new User { UserID = 1 };
			var topic = new Topic { TopicID = 2, ForumID = 3};
			var lastRead = new DateTime(2010, 1, 1);
			_lastReadRepo.Setup(x => x.GetLastReadTimeForTopic(user.UserID, topic.TopicID)).Returns((DateTime?)null);
			_lastReadRepo.Setup(x => x.GetLastReadTimesForForum(user.UserID, topic.ForumID)).Returns(lastRead);

			var result = service.GetLastReadTime(user, topic);

			Assert.Equal(lastRead, result);
		}
	}
}