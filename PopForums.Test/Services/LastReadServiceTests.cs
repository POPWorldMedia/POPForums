using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
	[TestFixture]
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

		[Test]
		public void MarkForumReadSetsReadTime()
		{
			var service = GetService();
			var forum = new Forum(123);
			var user = new User(456, DateTime.MinValue);
			service.MarkForumRead(user, forum);
			_lastReadRepo.Verify(l => l.SetForumRead(user.UserID, forum.ForumID, It.IsAny<DateTime>()), Times.Exactly(1));
		}

		[Test]
		public void MarkForumReadDeletesOldTopicReadTimes()
		{
			var service = GetService();
			var forum = new Forum(123);
			var user = new User(456, DateTime.MinValue);
			service.MarkForumRead(user, forum);
			_lastReadRepo.Verify(l => l.DeleteTopicReadsInForum(user.UserID, forum.ForumID), Times.Exactly(1));
		}

		[Test]
		public void MarkTopicReadThrowsWithoutUser()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.MarkTopicRead(null, new Topic(1)));
		}

		[Test]
		public void MarkTopicReadThrowsWithoutTopic()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.MarkTopicRead(new User(123, DateTime.MaxValue), null));
		}

		[Test]
		public void MarkAllForumReadThrowsWithoutUser()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.MarkAllForumsRead(null));
		}

		[Test]
		public void MarkForumReadThrowsWithoutUser()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.MarkForumRead(null, new Forum(1)));
		}

		[Test]
		public void MarkForumReadThrowsWithoutForum()
		{
			var service = GetService();
			Assert.Throws<ArgumentNullException>(() => service.MarkForumRead(new User(1, DateTime.MaxValue), null));
		}

		[Test]
		public void MarkAllForumReadSetsReadTimes()
		{
			var service = GetService();
			var user = new User(456, DateTime.MinValue);
			service.MarkAllForumsRead(user);
			_lastReadRepo.Verify(l => l.SetAllForumsRead(user.UserID, It.IsAny<DateTime>()), Times.Exactly(1));
		}

		[Test]
		public void MarkAllForumReadDeletesAllOldTopicReadTimes()
		{
			var service = GetService();
			var user = new User(456, DateTime.MinValue);
			service.MarkAllForumsRead(user);
			_lastReadRepo.Verify(l => l.DeleteAllTopicReads(user.UserID), Times.Exactly(1));
		}

		[Test]
		public void ForumReadStatusForNoUser()
		{
			var service = GetService();
			var forum1 = new Forum(1);
			var forum2 = new Forum(2) { IsArchived = true };
			var forum3 = new Forum(3);
			var container = new CategorizedForumContainer(new List<Category>(), new[] { forum1, forum2, forum3 });
			service.GetForumReadStatus(null, container);
			Assert.AreEqual(3, container.ReadStatusLookup.Count);
			Assert.AreEqual(ReadStatus.NoNewPosts, container.ReadStatusLookup[1]);
			Assert.AreEqual(ReadStatus.NoNewPosts | ReadStatus.Closed, container.ReadStatusLookup[2]);
			Assert.AreEqual(ReadStatus.NoNewPosts, container.ReadStatusLookup[3]);
		}

		[Test]
		public void ForumReadStatusUserNewPosts()
		{
			var service = GetService();
			var forum = new Forum(1) { LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) };
			var user = new User(2, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(2)).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } });
			var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
			service.GetForumReadStatus(user, container);
			Assert.AreEqual(1, container.ReadStatusLookup.Count);
			Assert.AreEqual(ReadStatus.NewPosts, container.ReadStatusLookup[1]);
		}

		[Test]
		public void ForumReadStatusUserNewPostsButNoTopicRecords()
		{
			var service = GetService();
			var forum = new Forum(1) { LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) };
			var user = new User(2, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(2)).Returns(new Dictionary<int, DateTime>());
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForum(user.UserID, forum.ForumID)).Returns(new DateTime(2000, 1, 1, 3, 0, 0));
			var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
			service.GetForumReadStatus(user, container);
			Assert.AreEqual(1, container.ReadStatusLookup.Count);
			Assert.AreEqual(ReadStatus.NewPosts, container.ReadStatusLookup[1]);
		}

		[Test]
		public void ForumReadStatusUserNewPostsNoLastReadRecords()
		{
			var service = GetService();
			var forum = new Forum(1) { LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) };
			var user = new User(2, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(2)).Returns(new Dictionary<int, DateTime>());
			var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
			service.GetForumReadStatus(user, container);
			Assert.AreEqual(1, container.ReadStatusLookup.Count);
			Assert.AreEqual(ReadStatus.NewPosts, container.ReadStatusLookup[1]);
		}

		[Test]
		public void ForumReadStatusUserNoNewPosts()
		{
			var service = GetService();
			var forum = new Forum(1) { LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) };
			var user = new User(2, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(2)).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 7, 0, 0) } });
			var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
			service.GetForumReadStatus(user, container);
			Assert.AreEqual(1, container.ReadStatusLookup.Count);
			Assert.AreEqual(ReadStatus.NoNewPosts, container.ReadStatusLookup[1]);
		}

		[Test]
		public void ForumReadStatusUserNewPostsArchived()
		{
			var service = GetService();
			var forum = new Forum(1) { LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0), IsArchived = true };
			var user = new User(2, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(2)).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } });
			var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
			service.GetForumReadStatus(user, container);
			Assert.AreEqual(1, container.ReadStatusLookup.Count);
			Assert.AreEqual(ReadStatus.NewPosts | ReadStatus.Closed, container.ReadStatusLookup[1]);
		}

		[Test]
		public void ForumReadStatusUserNoNewPostsArchived()
		{
			var service = GetService();
			var forum = new Forum(1) { LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0), IsArchived = true };
			var user = new User(2, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(2)).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 7, 0, 0) } });
			var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
			service.GetForumReadStatus(user, container);
			Assert.AreEqual(1, container.ReadStatusLookup.Count);
			Assert.AreEqual(ReadStatus.NoNewPosts | ReadStatus.Closed, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusForNoUser()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> {new Topic(1), new Topic(2) {IsClosed = true}, new Topic(3) {IsPinned = true}};
			service.GetTopicReadStatus(null, container);
			Assert.AreEqual(3, container.ReadStatusLookup.Count);
			Assert.AreEqual(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
			Assert.AreEqual(ReadStatus.NoNewPosts | ReadStatus.Closed | ReadStatus.NotPinned, container.ReadStatusLookup[2]);
			Assert.AreEqual(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.Pinned, container.ReadStatusLookup[3]);
		}

		[Test]
		public void TopicReadStatusWithUserNewNoForumRecordNoTopicRecord()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime>());
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserNewNoForumRecordWithTopicRecord()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime>());
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } });
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserNewWithForumRecordNoTopicRecord()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 2, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserNewWithForumRecordWithTopicRecord()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 2, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } });
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserNotNewWithForumRecordNoTopicRecord()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserNotNewNoForumRecordWithTopicRecord()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime>());
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 7, 0, 0) } });
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserNotNewWithForumRecordWithTopicRecordForumNewer()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } });
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserNotNewWithForumRecordWithTopicRecordTopicNewer()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 7, 0, 0) } });
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserOpenNewPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, IsPinned = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new [] {1})).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.Pinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserOpenNewNotPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserOpenNotNewPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, IsPinned = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.Pinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserOpenNotNewNotPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserClosedNewPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, IsClosed = true, IsPinned = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NewPosts | ReadStatus.Closed | ReadStatus.Pinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserClosedNewNotPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, IsClosed = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NewPosts | ReadStatus.Closed | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserClosedNoNewPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, IsClosed = true, IsPinned = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NoNewPosts | ReadStatus.Closed | ReadStatus.Pinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void TopicReadStatusWithUserClosedNoNewNotPinned()
		{
			var service = GetService();
			var container = new PagedTopicContainer();
			container.Topics = new List<Topic> { new Topic(1) { ForumID = 2, IsClosed = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
			var user = new User(123, DateTime.MinValue);
			_lastReadRepo.Setup(l => l.GetLastReadTimesForForums(user.UserID)).Returns(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } });
			_lastReadRepo.Setup(l => l.GetLastReadTimesForTopics(user.UserID, new[] { 1 })).Returns(new Dictionary<int, DateTime>());
			service.GetTopicReadStatus(user, container);
			Assert.AreEqual(ReadStatus.NoNewPosts | ReadStatus.Closed | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		}

		[Test]
		public void MarkTopicReadCallsRepo()
		{
			var service = GetService();
			var user = new User(1, DateTime.MinValue);
			var topic = new Topic(2);
			service.MarkTopicRead(user, topic);
			_lastReadRepo.Verify(l => l.SetTopicRead(user.UserID, topic.TopicID, It.IsAny<DateTime>()), Times.Exactly(1));
		}
	}
}