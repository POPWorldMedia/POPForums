namespace PopForums.Test.Services;

public class LastReadServiceTests
{
	private LastReadService GetService()
	{
		_lastReadRepo = Substitute.For<ILastReadRepository>();
		_postRepo = Substitute.For<IPostRepository>();
		return new LastReadService(_lastReadRepo, _postRepo);
	}

	private ILastReadRepository _lastReadRepo;
	private IPostRepository _postRepo;

	[Fact]
	public async Task MarkForumReadSetsReadTime()
	{
		var service = GetService();
		var forum = new Forum { ForumID = 123 };
		var user = new User { UserID = 456 };
		await service.MarkForumRead(user, forum);
		await _lastReadRepo.Received(1).SetForumRead(user.UserID, forum.ForumID, Arg.Any<DateTime>());
	}

	[Fact]
	public async Task MarkForumReadDeletesOldTopicReadTimes()
	{
		var service = GetService();
		var forum = new Forum { ForumID = 123 };
		var user = new User { UserID = 456 };
		await service.MarkForumRead(user, forum);
		await _lastReadRepo.Received(1).DeleteTopicReadsInForum(user.UserID, forum.ForumID);
	}

	[Fact]
	public async Task MarkTopicReadThrowsWithoutUser()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.MarkTopicRead(null, new Topic { TopicID = 1 }));
	}

	[Fact]
	public async Task MarkTopicReadThrowsWithoutTopic()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.MarkTopicRead(new User(), null));
	}

	[Fact]
	public async Task MarkAllForumReadThrowsWithoutUser()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.MarkAllForumsRead(null));
	}

	[Fact]
	public async Task MarkForumReadThrowsWithoutUser()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.MarkForumRead(null, new Forum { ForumID = 1 }));
	}

	[Fact]
	public async Task MarkForumReadThrowsWithoutForum()
	{
		var service = GetService();
		await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.MarkForumRead(new User(), null));
	}

	[Fact]
	public async Task MarkAllForumReadSetsReadTimes()
	{
		var service = GetService();
		var user = new User { UserID = 456 };
		await service.MarkAllForumsRead(user);
		await _lastReadRepo.Received(1).SetAllForumsRead(user.UserID, Arg.Any<DateTime>());
	}

	[Fact]
	public async Task MarkAllForumReadDeletesAllOldTopicReadTimes()
	{
		var service = GetService();
		var user = new User { UserID = 456 };
		await service.MarkAllForumsRead(user);
		await _lastReadRepo.Received(1).DeleteAllTopicReads(user.UserID);
	}

	[Fact]
	public async Task ForumReadStatusForNoUser()
	{
		var service = GetService();
		var forum1 = new Forum { ForumID = 1 };
		var forum2 = new Forum { ForumID = 2, IsArchived = true };
		var forum3 = new Forum { ForumID = 3 };
		var container = new CategorizedForumContainer(new List<Category>(), new[] { forum1, forum2, forum3 });
		await service.GetForumReadStatus(null, container);
		Assert.Equal(3, container.ReadStatusLookup.Count);
		Assert.Equal(ReadStatus.NoNewPosts, container.ReadStatusLookup[1]);
		Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Closed, container.ReadStatusLookup[2]);
		Assert.Equal(ReadStatus.NoNewPosts, container.ReadStatusLookup[3]);
	}

	[Fact]
	public async Task ForumReadStatusUserNewPosts()
	{
		var service = GetService();
		var forum = new Forum { ForumID = 1, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) };
		var user = new User { UserID = 2 };
		_lastReadRepo.GetLastReadTimesForForums(2).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } }));
		var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
		await service.GetForumReadStatus(user, container);
		Assert.Single(container.ReadStatusLookup);
		Assert.Equal(ReadStatus.NewPosts, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task ForumReadStatusUserNewPostsButNoTopicRecords()
	{
		var service = GetService();
		var forum = new Forum { ForumID = 1, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) };
		var user = new User { UserID = 2 };
		_lastReadRepo.GetLastReadTimesForForums(2).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		_lastReadRepo.GetLastReadTimesForForum(user.UserID, forum.ForumID).Returns(new DateTime(2000, 1, 1, 3, 0, 0));
		var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
		await service.GetForumReadStatus(user, container);
		Assert.Single(container.ReadStatusLookup);
		Assert.Equal(ReadStatus.NewPosts, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task ForumReadStatusUserNewPostsNoLastReadRecords()
	{
		var service = GetService();
		var forum = new Forum { ForumID = 1, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) };
		var user = new User { UserID = 2 };
		_lastReadRepo.GetLastReadTimesForForums(2).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
		await service.GetForumReadStatus(user, container);
		Assert.Single(container.ReadStatusLookup);
		Assert.Equal(ReadStatus.NewPosts, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task ForumReadStatusUserNoNewPosts()
	{
		var service = GetService();
		var forum = new Forum { ForumID = 1, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) };
		var user = new User { UserID = 2 };
		_lastReadRepo.GetLastReadTimesForForums(2).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 7, 0, 0) } }));
		var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
		await service.GetForumReadStatus(user, container);
		Assert.Single(container.ReadStatusLookup);
		Assert.Equal(ReadStatus.NoNewPosts, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task ForumReadStatusUserNewPostsArchived()
	{
		var service = GetService();
		var forum = new Forum { ForumID = 1, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0), IsArchived = true };
		var user = new User { UserID = 2 };
		_lastReadRepo.GetLastReadTimesForForums(2).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } }));
		var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
		await service.GetForumReadStatus(user, container);
		Assert.Single(container.ReadStatusLookup);
		Assert.Equal(ReadStatus.NewPosts | ReadStatus.Closed, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task ForumReadStatusUserNoNewPostsArchived()
	{
		var service = GetService();
		var forum = new Forum { ForumID = 1, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0), IsArchived = true };
		var user = new User { UserID = 2 };
		_lastReadRepo.GetLastReadTimesForForums(2).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 7, 0, 0) } }));
		var container = new CategorizedForumContainer(new List<Category>(), new[] { forum });
		await service.GetForumReadStatus(user, container);
		Assert.Single(container.ReadStatusLookup);
		Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Closed, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusForNoUser()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> {new Topic { TopicID = 1 }, new Topic { TopicID = 2, IsClosed = true}, new Topic { TopicID = 3, IsPinned = true}};
		await service.GetTopicReadStatus(null, container);
		Assert.Equal(3, container.ReadStatusLookup.Count);
		Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
		Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Closed | ReadStatus.NotPinned, container.ReadStatusLookup[2]);
		Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.Pinned, container.ReadStatusLookup[3]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserNewNoForumRecordNoTopicRecord()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserNewNoForumRecordWithTopicRecord()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } }));
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserNewWithForumRecordNoTopicRecord()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 2, 0, 0) } }));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserNewWithForumRecordWithTopicRecord()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 2, 0, 0) } }));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } }));
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserNotNewWithForumRecordNoTopicRecord()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } }));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserNotNewNoForumRecordWithTopicRecord()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 7, 0, 0) } }));
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserNotNewWithForumRecordWithTopicRecordForumNewer()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } }));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 3, 0, 0) } }));
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserNotNewWithForumRecordWithTopicRecordTopicNewer()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } }));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 1, new DateTime(2000, 1, 1, 7, 0, 0) } }));
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserOpenNewPinned()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, IsPinned = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } }));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.Pinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserOpenNewNotPinned()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } }));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserOpenNotNewPinned()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, IsPinned = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } }));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.Pinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserOpenNotNewNotPinned()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } }));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Open | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserClosedNewPinned()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, IsClosed = true, IsPinned = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } }));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(new Dictionary<int, DateTime>());
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NewPosts | ReadStatus.Closed | ReadStatus.Pinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserClosedNewNotPinned()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, IsClosed = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 3, 0, 0) } }));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NewPosts | ReadStatus.Closed | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserClosedNoNewPinned()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, IsClosed = true, IsPinned = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } }));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		
		await service.GetTopicReadStatus(user, container);
		
		Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Closed | ReadStatus.Pinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task TopicReadStatusWithUserClosedNoNewNotPinned()
	{
		var service = GetService();
		var container = new PagedTopicContainer();
		container.Topics = new List<Topic> { new Topic { TopicID = 1, ForumID = 2, IsClosed = true, LastPostTime = new DateTime(2000, 1, 1, 5, 0, 0) } };
		var user = new User { UserID = 123 };
		_lastReadRepo.GetLastReadTimesForForums(user.UserID).Returns(Task.FromResult(new Dictionary<int, DateTime> { { 2, new DateTime(2000, 1, 1, 7, 0, 0) } }));
		_lastReadRepo.GetLastReadTimesForTopics(user.UserID, Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 1 }))).Returns(Task.FromResult(new Dictionary<int, DateTime>()));
		await service.GetTopicReadStatus(user, container);
		Assert.Equal(ReadStatus.NoNewPosts | ReadStatus.Closed | ReadStatus.NotPinned, container.ReadStatusLookup[1]);
	}

	[Fact]
	public async Task MarkTopicReadCallsRepo()
	{
		var service = GetService();
		var user = new User { UserID = 1 };
		var topic = new Topic { TopicID = 2 };
		await service.MarkTopicRead(user, topic);
		await _lastReadRepo.Received(1).SetTopicRead(user.UserID, topic.TopicID, Arg.Any<DateTime>());
	}

	[Fact]
	public async Task GetLastReadTimeReturnsTopicTimeWhenAvailable()
	{
		var service = GetService();
		var user = new User { UserID = 1 };
		var topic = new Topic { TopicID = 2 };
		var lastRead = new DateTime(2010, 1, 1);
		_lastReadRepo.GetLastReadTimeForTopic(user.UserID, topic.TopicID).Returns(lastRead);

		var result = await service.GetLastReadTime(user, topic);

		Assert.Equal(lastRead, result);
		await _lastReadRepo.DidNotReceive().GetLastReadTimesForForum(Arg.Any<int>(), Arg.Any<int>());
	}

	[Fact]
	public async Task GetLastReadTimeReturnsForumTimeWhenNoTopicTimeAvailable()
	{
		var service = GetService();
		var user = new User { UserID = 1 };
		var topic = new Topic { TopicID = 2, ForumID = 3};
		var lastRead = new DateTime(2010, 1, 1);
		_lastReadRepo.GetLastReadTimeForTopic(user.UserID, topic.TopicID).Returns((DateTime?)null);
		_lastReadRepo.GetLastReadTimesForForum(user.UserID, topic.ForumID).Returns(lastRead);

		var result = await service.GetLastReadTime(user, topic);

		Assert.Equal(lastRead, result);
	}
}