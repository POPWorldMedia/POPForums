namespace PopForums.Test.Services;

public class ForumServiceTests
{
	private IForumRepository _mockForumRepo;
	private ITopicRepository _mockTopicRepo;
	private ICategoryRepository _mockCategoryRepo;
	private ISettingsManager _mockSettingsManager;
	private ILastReadService _mockLastReadService;

	private ForumService GetService()
	{
		_mockCategoryRepo = Substitute.For<ICategoryRepository>();
		_mockForumRepo = Substitute.For<IForumRepository>();
		_mockTopicRepo = Substitute.For<ITopicRepository>();
		_mockSettingsManager = Substitute.For<ISettingsManager>();
		_mockLastReadService = Substitute.For<ILastReadService>();
		return new ForumService(_mockForumRepo, _mockTopicRepo, _mockCategoryRepo, _mockSettingsManager, _mockLastReadService);
	}

	[Fact]
	public async Task Get()
	{
		const int forumID = 123;
		var forumService = GetService();
		_mockForumRepo.Get(forumID).Returns(Task.FromResult(new Forum {ForumID = forumID}));
		var forum = await forumService.Get(forumID);
		Assert.Equal(forumID, forum.ForumID);
		await _mockForumRepo.Received().Get(forumID);
	}

	[Fact]
	public async Task Create()
	{
		var forumService = GetService();
		const int categoryID = 456;
		const string title = "forum title";
		const string desc = "description of forum";
		const bool isVisible = true;
		const bool isArchived = true;
		const int sortOrder = 5;
		const int forumID = 123;
		const string adapter = "Jeff.Adapter";
		const bool isQAForum = true;
		var forum = new Forum {ForumID = forumID, CategoryID = categoryID, Title = title, Description = desc, IsVisible = isVisible, IsArchived = isArchived, SortOrder = sortOrder};
		_mockForumRepo.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, Arg.Any<String>(), adapter, isQAForum).Returns(Task.FromResult(forum));
		_mockForumRepo.GetUrlNamesThatStartWith(Arg.Any<string>()).Returns(Task.FromResult(new List<string>()));
		_mockForumRepo.GetAll().Returns(new List<Forum> { new Forum { ForumID = 1, SortOrder = 9 }, new Forum { ForumID = 2, SortOrder = 6 }, forum});
		var result = await forumService.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, adapter, isQAForum);
		Assert.Equal(forum, result);
		await _mockForumRepo.Received().Create(categoryID, title, desc, isVisible, isArchived, sortOrder, Arg.Any<String>(), adapter, isQAForum);
		await _mockForumRepo.Received().UpdateSortOrder(123, 0);
		await _mockForumRepo.Received().UpdateSortOrder(2, 2);
		await _mockForumRepo.Received().UpdateSortOrder(1, 4);
	}

	[Fact]
	public async Task CreateMakesUrlTitle()
	{
		var forumService = GetService();
		const int categoryID = 456;
		const string title = "forum title";
		const string desc = "description of forum";
		const bool isVisible = true;
		const bool isArchived = true;
		const int sortOrder = 5;
		const int forumID = 123;
		const string adapter = "Jeff.Adapter";
		const bool isQAForum = true;
		var forum = new Forum { ForumID = forumID, CategoryID = categoryID, Title = title, Description = desc, IsVisible = isVisible, IsArchived = isArchived, SortOrder = sortOrder };
		_mockForumRepo.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, Arg.Any<String>(), adapter, isQAForum).Returns(Task.FromResult(forum));
		_mockForumRepo.GetUrlNamesThatStartWith(Arg.Any<string>()).Returns(Task.FromResult(new List<string>()));
		await forumService.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, adapter, isQAForum);
		await _mockForumRepo.Received().Create(categoryID, title, desc, isVisible, isArchived, sortOrder, "forum-title", adapter, isQAForum);
	}

	[Fact]
	public async Task CreateMakesUrlTitleWithAppendage()
	{
		var forumService = GetService();
		const int categoryID = 456;
		const string title = "forum title";
		const string desc = "description of forum";
		const bool isVisible = true;
		const bool isArchived = true;
		const int sortOrder = 5;
		const int forumID = 123;
		const string adapter = "Jeff.Adapter";
		const bool isQAForum = true;
		var forum = new Forum { ForumID = forumID, CategoryID = categoryID, Title = title, Description = desc, IsVisible = isVisible, IsArchived = isArchived, SortOrder = sortOrder };
		_mockForumRepo.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, Arg.Any<String>(), adapter, isQAForum).Returns(Task.FromResult(forum));
		_mockForumRepo.GetUrlNamesThatStartWith(title.ToUrlName()).Returns(Task.FromResult(new List<string> {"forum-title", "forum-title-but-not", "forum-title-2"}));
		await forumService.Create(categoryID, title, desc, isVisible, isArchived, sortOrder, adapter, isQAForum);
		await _mockForumRepo.Received().Create(categoryID, title, desc, isVisible, isArchived, sortOrder, "forum-title-3", adapter, isQAForum);
	}

	[Fact]
	public async Task UpdateLast()
	{
		const int forumID = 123;
		const int topicID = 456;
		var lastTime = new DateTime(2001, 2, 2);
		const string lastName = "Jeff";
		var forum = new Forum { ForumID = forumID };
		var topic = new Topic { TopicID = topicID, LastPostTime = lastTime, LastPostName = lastName };
		var forumService = GetService();
		_mockTopicRepo.GetLastUpdatedTopic(forum.ForumID).Returns(Task.FromResult(topic));
		await forumService.UpdateLast(forum);
		await _mockTopicRepo.Received().GetLastUpdatedTopic(forum.ForumID);
		await _mockForumRepo.Received().UpdateLastTimeAndUser(forum.ForumID, lastTime, lastName);
	}

	[Fact]
	public async Task UpdateLastWithValues()
	{
		var forumService = GetService();
		const int forumID = 123;
		var lastTime = new DateTime(2001, 2, 2);
		const string lastName = "Jeff";
		var forum = new Forum { ForumID = forumID };
		await forumService.UpdateLast(forum, lastTime, lastName);
		await _mockForumRepo.Received().UpdateLastTimeAndUser(forum.ForumID, lastTime, lastName);
	}

	//[Fact]
	//[Ignore] // TODO: gotta account for spawned thread
	//public void UpdateCounts()
	//{
	//    const int topicCount = 456;
	//    const int postCount = 789;
	//    const int forumID = 123;
	//    var forum = new Forum(forumID);
	//    var forumService = GetService();
	//    _mockTopicRepo.GetPostCount(forumID, false).Returns(postCount);
	//    _mockTopicRepo.GetTopicCount(forumID, false).Returns(topicCount);
	//    forumService.UpdateCounts(forum);
	//    _mockTopicRepo.Received().GetPostCount(forumID, false);
	//    _mockTopicRepo.Received().GetTopicCount(forumID, false);
	//    _mockForumRepo.Verify(f => f.UpdateTopicAndPostCounts(forumID, topicCount, postCount));
	//}

	[Fact]
	public async Task GetForumsWithCategories()
	{
		var forums = new List<Forum>();
		var cats = new List<Category>();
		var forumService = GetService();
		_mockForumRepo.GetAll().Returns(forums);
		_mockCategoryRepo.GetAll().Returns(Task.FromResult(cats));
		_mockSettingsManager.Current.ForumTitle.Returns("whatever");
		var container = await forumService.GetCategorizedForumContainer();
		await _mockCategoryRepo.Received().GetAll();
		await _mockForumRepo.Received().GetAll();
		Assert.Equal(container.AllForums, forums);
		Assert.Equal(container.AllCategories, cats);
	}

	[Fact]
	public async Task MoveUp()
	{
		var f1 = new Forum { ForumID = 123, SortOrder = 0, CategoryID = 777 };
		var f2 = new Forum { ForumID = 456, SortOrder = 2, CategoryID = 777 };
		var f3 = new Forum { ForumID = 789, SortOrder = 4, CategoryID = 777 };
		var f4 = new Forum { ForumID = 1000,SortOrder = 6, CategoryID = 777 };
		var forums = new List<Forum> { f1, f2, f3, f4 };
		var service = GetService();
		_mockForumRepo.GetForumsInCategory(777).Returns(Task.FromResult(forums));
		_mockForumRepo.Get(f3.ForumID).Returns(Task.FromResult(f3));
		await service.MoveForumUp(f3.ForumID);
		await _mockForumRepo.Received().GetForumsInCategory(777);
		await _mockForumRepo.Received(4).UpdateSortOrder(Arg.Any<int>(), Arg.Any<int>());
		await _mockForumRepo.Received().UpdateSortOrder(f1.ForumID, f1.SortOrder);
		await _mockForumRepo.Received().UpdateSortOrder(f2.ForumID, f2.SortOrder);
		await _mockForumRepo.Received().UpdateSortOrder(f3.ForumID, f3.SortOrder);
		await _mockForumRepo.Received().UpdateSortOrder(f4.ForumID, f4.SortOrder);
		Assert.Equal(0, f1.SortOrder);
		Assert.Equal(2, f3.SortOrder);
		Assert.Equal(4, f2.SortOrder);
		Assert.Equal(6, f4.SortOrder);
	}

	[Fact]
	public async Task MoveDown()
	{
		var f1 = new Forum { ForumID = 123, SortOrder = 0, CategoryID = 777 };
		var f2 = new Forum { ForumID = 456, SortOrder = 2, CategoryID = 777 };
		var f3 = new Forum { ForumID = 789, SortOrder = 4, CategoryID = 777 };
		var f4 = new Forum { ForumID = 1000, SortOrder = 6, CategoryID = 777 };
		var forums = new List<Forum> { f1, f2, f3, f4 };
		var service = GetService();
		_mockForumRepo.GetForumsInCategory(777).Returns(Task.FromResult(forums));
		_mockForumRepo.Get(f3.ForumID).Returns(Task.FromResult(f3));
		await service.MoveForumDown(f3.ForumID);
		await _mockForumRepo.Received().GetForumsInCategory(777);
		await _mockForumRepo.Received(4).UpdateSortOrder(Arg.Any<int>(), Arg.Any<int>());
		await _mockForumRepo.Received().UpdateSortOrder(f1.ForumID, f1.SortOrder);
		await _mockForumRepo.Received().UpdateSortOrder(f2.ForumID, f2.SortOrder);
		await _mockForumRepo.Received().UpdateSortOrder(f3.ForumID, f3.SortOrder);
		await _mockForumRepo.Received().UpdateSortOrder(f4.ForumID, f4.SortOrder);
		Assert.Equal(0, f1.SortOrder);
		Assert.Equal(2, f2.SortOrder);
		Assert.Equal(4, f4.SortOrder);
		Assert.Equal(6, f3.SortOrder);
	}

	[Fact]
	public async Task MoveForumUpThrowsIfNoForum()
	{
		var service = GetService();
		_mockForumRepo.Get(Arg.Any<int>()).Returns((Forum) null);

		await Assert.ThrowsAsync<Exception>(async () => await service.MoveForumUp(1));
	}

	[Fact]
	public async Task MoveForumDownThrowsIfNoForum()
	{
		var service = GetService();
		_mockForumRepo.Get(Arg.Any<int>()).Returns((Forum)null);

		await Assert.ThrowsAsync<Exception>(async () => await service.MoveForumDown(1));
	}

	[Fact]
	public async Task PostRestrictions()
	{
		var service = GetService();
		var forum = new Forum { ForumID = 1 };
		var roles = new List<string> {"leader", "follower"};
		_mockForumRepo.GetForumPostRoles(forum.ForumID).Returns(Task.FromResult(roles));
		var result = await service.GetForumPostRoles(forum);
		await _mockForumRepo.Received().GetForumPostRoles(forum.ForumID);
		Assert.Same(roles, result);
	}

	[Fact]
	public async Task ViewRestrictions()
	{
		var service = GetService();
		var forum = new Forum { ForumID = 1 };
		var roles = new List<string> { "leader", "follower" };
		_mockForumRepo.GetForumViewRoles(forum.ForumID).Returns(Task.FromResult(roles));
		var result = await service.GetForumViewRoles(forum);
		await _mockForumRepo.Received().GetForumViewRoles(forum.ForumID);
		Assert.Same(roles, result);
	}

	[Fact]
	public async Task GetViewableForumIDsFromViewRestrictedForumsReturnsEmptyDictionaryWithoutUser()
	{
		var graph = new Dictionary<int, List<string>>
		{
			{1, new List<string> {"blah"}},
			{2, new List<string>()},
			{3, new List<string> {"blah"}}
		};
		var service = GetService();
		_mockForumRepo.GetAllVisible().Returns(new List<Forum>
		{
			new Forum { ForumID = 1 }, new Forum { ForumID = 2 }, new Forum { ForumID = 3 }
		});
		_mockForumRepo.GetForumViewRestrictionRoleGraph().Returns(Task.FromResult(graph));
		var result = await service.GetViewableForumIDsFromViewRestrictedForums(null);
		Assert.Single(result);
		Assert.Equal(2, result[0]);
	}

	[Fact]
	public async Task GetViewableForumIDsFromViewRestrictedForumsDoesntIncludeForumsWithNoViewRestrictions()
	{
		var graph = new Dictionary<int, List<string>>();
		graph.Add(1, new List<string> { "blah" });
		graph.Add(2, new List<string>());
		graph.Add(3, new List<string> { "blah" });
		var service = GetService();
		_mockForumRepo.GetForumViewRestrictionRoleGraph().Returns(Task.FromResult(graph));
		_mockForumRepo.GetAllVisible().Returns(new List<Forum>
		{
			new Forum { ForumID = 1 }, new Forum { ForumID = 2 }, new Forum { ForumID = 3 }
		});
		var result = await service.GetViewableForumIDsFromViewRestrictedForums(new User { UserID = 123, Roles = new [] {"blah"}.ToList() });
		Assert.Equal(3, result.Count);
	}

	[Fact]
	public async Task GetViewableForumIDsFromViewRestrictedForumsReturnsIDsWithMatchingUserRoles()
	{
		var graph = new Dictionary<int, List<string>>();
		graph.Add(1, new List<string> { "blah" });
		graph.Add(2, new List<string>());
		graph.Add(3, new List<string> { "blep" });
		graph.Add(4, new List<string> { "burp", "blah" });
		graph.Add(5, new List<string> { "burp" });
		var service = GetService();
		_mockForumRepo.GetForumViewRestrictionRoleGraph().Returns(Task.FromResult(graph));
		_mockForumRepo.GetAllVisible().Returns(new List<Forum>
		{
			new Forum { ForumID = 1 }, new Forum { ForumID = 2 }, new Forum { ForumID = 3 }, new Forum { ForumID = 4 }, new Forum { ForumID = 5 }
		});
		var result = await service.GetViewableForumIDsFromViewRestrictedForums(new User { UserID = 123, Roles = new[] { "blah", "blep" }.ToList() });
		Assert.Equal(4, result.Count);
		Assert.Contains(1, result);
		Assert.Contains(2, result);
		Assert.Contains(3, result);
		Assert.Contains(4, result);
		Assert.DoesNotContain(5, result);
	}

	[Fact]
	public async Task GetNonViewableDoesntIncludeForumsWithNoViewRestrictions()
	{
		var graph = new Dictionary<int, List<string>>();
		graph.Add(1, new List<string> { "blah" });
		graph.Add(2, new List<string>());
		graph.Add(3, new List<string> { "blah" });
		var service = GetService();
		_mockForumRepo.GetForumViewRestrictionRoleGraph().Returns(Task.FromResult(graph));
		var result = await service.GetNonViewableForumIDs(new User { UserID = 123, Roles = new List<string>()});
		Assert.Equal(2, result.Count);
		Assert.DoesNotContain(2, result);
	}

	[Fact]
	public async Task GetNonViewableDoesntIncludeForumsWithRoleMatchingViewRestrictions()
	{
		var graph = new Dictionary<int, List<string>>();
		graph.Add(1, new List<string> { "blah" });
		graph.Add(2, new List<string>());
		graph.Add(3, new List<string> { "OK" });
		var service = GetService();
		_mockForumRepo.GetForumViewRestrictionRoleGraph().Returns(Task.FromResult(graph));
		var result = await service.GetNonViewableForumIDs(new User { UserID = 123, Roles = new List<string> { "OK" } });
		Assert.Single(result);
		Assert.DoesNotContain(3, result);
	}

	[Fact]
	public async Task GetNonViewableIncludesForumsWithNoMatchingViewRestrictions()
	{
		var graph = new Dictionary<int, List<string>>();
		graph.Add(1, new List<string> { "blah" });
		graph.Add(2, new List<string>());
		graph.Add(3, new List<string> { "OK" });
		var service = GetService();
		_mockForumRepo.GetForumViewRestrictionRoleGraph().Returns(Task.FromResult(graph));
		var result = await service.GetNonViewableForumIDs(new User { UserID = 123, Roles = new List<string> { "OK" } });
		Assert.Single(result);
		Assert.Equal(1, result[0]);
	}

	[Fact]
	public async Task GetNonViewableExcludesViewRestrictionsForNoUser()
	{
		var graph = new Dictionary<int, List<string>>();
		graph.Add(1, new List<string> { "blah" });
		graph.Add(2, new List<string>());
		graph.Add(3, new List<string> { "OK" });
		var service = GetService();
		_mockForumRepo.GetForumViewRestrictionRoleGraph().Returns(Task.FromResult(graph));
		var result = await service.GetNonViewableForumIDs(null);
		Assert.Equal(2, result.Count);
		Assert.Equal(1, result[0]);
		Assert.Equal(3, result[1]);
	}

	[Fact]
	public async Task GetCategorizedForUserHasOnlyViewableForums()
	{
		var graph = new Dictionary<int, List<string>>();
		graph.Add(1, new List<string> { "blah" });
		graph.Add(2, new List<string>());
		graph.Add(3, new List<string> { "OK" });
		var allForums = new List<Forum> {new Forum { ForumID = 1 }, new Forum { ForumID = 2 }, new Forum { ForumID = 3 } };
		var service = GetService();
		_mockForumRepo.GetForumViewRestrictionRoleGraph().Returns(Task.FromResult(graph));
		_mockForumRepo.GetAllVisible().Returns(allForums);
		_mockCategoryRepo.GetAll().Returns(Task.FromResult(new List<Category>()));
		_mockSettingsManager.Current.ForumTitle.Returns("whatever");
		var container = await service.GetCategorizedForumContainerFilteredForUser(new User { UserID = 123, Roles = new List<string> { "OK" } });
		Assert.Equal(2, container.UncategorizedForums.Count);
		Assert.Null(container.UncategorizedForums.SingleOrDefault(f => f.ForumID == 1));
	}

	[Fact]
	public async Task GetCategorizedForUserPopulatesReadStatus()
	{
		var service = GetService();
		var user = new User { UserID = 123 };
		_mockCategoryRepo.GetAll().Returns(Task.FromResult(new List<Category>()));
		_mockForumRepo.GetAllVisible().Returns(new List<Forum>());
		_mockForumRepo.GetForumViewRestrictionRoleGraph().Returns(Task.FromResult(new Dictionary<int, List<string>>()));
		_mockSettingsManager.Current.ForumTitle.Returns("");
		await service.GetCategorizedForumContainerFilteredForUser(user);
		await _mockLastReadService.Received(1).GetForumReadStatus(user, Arg.Any<CategorizedForumContainer>());
	}

	[Fact]
	public async Task GetCategoryContainersWithForumsMapsCatsWithUnCatForums()
	{
		var service = GetService();
		var categories = new List<Category>
		{
			new Category {CategoryID = 1, SortOrder = 5},
			new Category {CategoryID = 2, SortOrder = 1},
			new Category {CategoryID = 3, SortOrder = 3}
		};
		_mockCategoryRepo.GetAll().Returns(Task.FromResult(categories));
		var forums = new List<Forum>
		{
			new Forum {ForumID = 1, CategoryID = null},
			new Forum {ForumID = 2, CategoryID = categories[0].CategoryID, SortOrder = 3},
			new Forum {ForumID = 3, CategoryID = categories[0].CategoryID, SortOrder = 1},
			new Forum {ForumID = 4, CategoryID = categories[0].CategoryID, SortOrder = 7},
			new Forum {ForumID = 5, CategoryID = categories[0].CategoryID, SortOrder = 5},
			new Forum {ForumID = 6, CategoryID = categories[2].CategoryID}
		};
		_mockForumRepo.GetAll().Returns(forums);

		var result = await service.GetCategoryContainersWithForums();

		Assert.Equal(0, result[0].Category.CategoryID);
		Assert.Equal(2, result[1].Category.CategoryID);
		Assert.Equal(3, result[2].Category.CategoryID);
		Assert.Equal(1, result[3].Category.CategoryID);
	}

	[Fact]
	public async Task GetCategoryContainersWithForumsMapsCatsWithoutUnCatForums()
	{
		var service = GetService();
		var categories = new List<Category>
		{
			new Category {CategoryID = 1, SortOrder = 5},
			new Category {CategoryID = 2, SortOrder = 1},
			new Category {CategoryID = 3, SortOrder = 3}
		};
		_mockCategoryRepo.GetAll().Returns(Task.FromResult(categories));
		var forums = new List<Forum>
		{
			new Forum {ForumID = 2, CategoryID = categories[0].CategoryID, SortOrder = 3},
			new Forum {ForumID = 3, CategoryID = categories[0].CategoryID, SortOrder = 1},
			new Forum {ForumID = 4, CategoryID = categories[0].CategoryID, SortOrder = 7},
			new Forum {ForumID = 5, CategoryID = categories[0].CategoryID, SortOrder = 5},
			new Forum {ForumID = 6, CategoryID = categories[2].CategoryID}
		};
		_mockForumRepo.GetAll().Returns(forums);

		var result = await service.GetCategoryContainersWithForums();

		Assert.Equal(2, result[0].Category.CategoryID);
		Assert.Equal(3, result[1].Category.CategoryID);
		Assert.Equal(1, result[2].Category.CategoryID);
	}

	[Fact]
	public async Task GetCategoryContainersWithForumsMapsForums()
	{
		var service = GetService();
		var categories = new List<Category>
		{
			new Category {CategoryID = 1, SortOrder = 5},
			new Category {CategoryID = 2, SortOrder = 1},
			new Category {CategoryID = 3, SortOrder = 3}
		};
		_mockCategoryRepo.GetAll().Returns(Task.FromResult(categories));
		var forums = new List<Forum>
		{
			new Forum {ForumID = 1, CategoryID = null, SortOrder = 3},
			new Forum {ForumID = 2, CategoryID = categories[0].CategoryID, SortOrder = 3},
			new Forum {ForumID = 3, CategoryID = categories[0].CategoryID, SortOrder = 1},
			new Forum {ForumID = 4, CategoryID = categories[0].CategoryID, SortOrder = 7},
			new Forum {ForumID = 5, CategoryID = categories[0].CategoryID, SortOrder = 5},
			new Forum {ForumID = 6, CategoryID = categories[2].CategoryID},
			new Forum {ForumID = 7, CategoryID = null, SortOrder = 1},
		};
		_mockForumRepo.GetAll().Returns(forums);

		var result = await service.GetCategoryContainersWithForums();

		Assert.Equal(7, result[0].Forums.ToArray()[0].ForumID);
		Assert.Equal(1, result[0].Forums.ToArray()[1].ForumID);
		Assert.Equal(3, result[3].Forums.ToArray()[0].ForumID);
		Assert.Equal(2, result[3].Forums.ToArray()[1].ForumID);
		Assert.Equal(5, result[3].Forums.ToArray()[2].ForumID);
		Assert.Equal(4, result[3].Forums.ToArray()[3].ForumID);
		Assert.Equal(6, result[2].Forums.ToArray()[0].ForumID);
	}

	[Fact]
	public void MapTopicContainerForQAMapsBaseProperties()
	{
		var topicContainer = new TopicContainer
		{
			Forum = new Forum { ForumID = 1 },
			Topic = new Topic { TopicID = 2 },
			Posts = new List<Post> {new Post { PostID = 123, IsFirstInTopic = true }},
			PagerContext = new PagerContext(),
			PermissionContext = new ForumPermissionContext(),
			Signatures = new Dictionary<int, string>(),
			Avatars = new Dictionary<int, int>(),
			VotedPostIDs = new List<int>(),
			TopicState = new TopicState()
		};
		var service = GetService();
		var result = service.MapTopicContainerForQA(topicContainer);
		Assert.Same(topicContainer.Forum, result.Forum);
		Assert.Same(topicContainer.Topic, result.Topic);
		Assert.Same(topicContainer.Posts, result.Posts);
		Assert.Same(topicContainer.PagerContext, result.PagerContext);
		Assert.Same(topicContainer.PermissionContext, result.PermissionContext);
		Assert.Same(topicContainer.Signatures, result.Signatures);
		Assert.Same(topicContainer.Avatars, result.Avatars);
		Assert.Same(topicContainer.VotedPostIDs, result.VotedPostIDs);
		Assert.Same(topicContainer.TopicState, result.TopicState);
	}

	[Fact]
	public void MapTopicContainerGrabsFirstPostForQuestion()
	{
		var posts = new List<Post>
		{
			new Post {PostID = 1},
			new Post{PostID = 2, IsFirstInTopic = true}
		};
		var topicContainer = new TopicContainer {Posts = posts, Topic = new Topic { TopicID = 123 }};
		var service = GetService();
		var result = service.MapTopicContainerForQA(topicContainer);
		Assert.Equal(2, result.QuestionPostWithComments.Post.PostID);
	}

	[Fact]
	public void MapTopicContainerThrowsWithNoFirstInTopicPost()
	{
		var posts = new List<Post>
		{
			new Post { PostID =  1 },
			new Post { PostID =  2 }
		};
		var topicContainer = new TopicContainer { Posts = posts, Topic = new Topic { TopicID = 123 } };
		var service = GetService();
		Assert.Throws<InvalidOperationException>(() => service.MapTopicContainerForQA(topicContainer));
	}

	[Fact]
	public void MapTopicContainerThrowsWithMoreThanOneFirstInTopicPost()
	{
		var posts = new List<Post>
		{
			new Post { PostID =  1, IsFirstInTopic = true},
			new Post { PostID =  2, IsFirstInTopic = true}
		};
		var topicContainer = new TopicContainer { Posts = posts, Topic = new Topic { TopicID = 123 } };
		var service = GetService();
		Assert.Throws<InvalidOperationException>(() => service.MapTopicContainerForQA(topicContainer));
	}

	[Fact]
	public void MapTopicContainerSetsQuestionsWithNoParentAsAnswers()
	{
		var post1 = new Post { PostID = 1, ParentPostID = 0};
		var post2 = new Post { PostID = 2, IsFirstInTopic = true};
		var post3 = new Post { PostID = 3, ParentPostID = 2};
		var post4 = new Post { PostID = 4, ParentPostID = 1};
		var post5 = new Post { PostID = 5, ParentPostID = 3};
		var posts = new List<Post> {post1, post2, post3, post4, post5};
		var topicContainer = new TopicContainer { Posts = posts, Topic = new Topic { TopicID = 1234 } };
		var service = GetService();
		var result = service.MapTopicContainerForQA(topicContainer);
		Assert.Single(result.AnswersWithComments);
		Assert.Same(post1, result.AnswersWithComments[0].Post);
	}

	[Fact]
	public void MapTopicContainerMapsCommentsToParentQuestionsAndAnswers()
	{
		var post1 = new Post { PostID = 1, ParentPostID = 0 };
		var post2 = new Post { PostID = 2, IsFirstInTopic = true };
		var post3 = new Post { PostID = 3, ParentPostID = 0 };
		var post4 = new Post { PostID = 4, ParentPostID = 1 };
		var post5 = new Post { PostID = 5, ParentPostID = 2 };
		var post6 = new Post { PostID = 6, ParentPostID = 3 };
		var post7 = new Post { PostID = 7, ParentPostID = 3 };
		var posts = new List<Post> { post1, post2, post3, post4, post5, post6, post7 };
		var topicContainer = new TopicContainer { Posts = posts, Topic = new Topic { TopicID = 1234 } };
		var service = GetService();
		var result = service.MapTopicContainerForQA(topicContainer);
		Assert.True(result.AnswersWithComments[0].Children.Count == 1);
		Assert.Contains(post4, result.AnswersWithComments[0].Children);
		Assert.True(result.AnswersWithComments[1].Children.Count == 2);
		Assert.Contains(post6, result.AnswersWithComments[1].Children);
		Assert.Contains(post7, result.AnswersWithComments[1].Children);
	}

	[Fact]
	public void MapTopicContainerMapsCommentsToQuestion()
	{
		var post1 = new Post { PostID = 1, ParentPostID = 0 };
		var post2 = new Post { PostID = 2, IsFirstInTopic = true };
		var post3 = new Post { PostID = 3, ParentPostID = 0 };
		var post4 = new Post { PostID = 4, ParentPostID = 1 };
		var post5 = new Post { PostID = 5, ParentPostID = 2 };
		var post6 = new Post { PostID = 6, ParentPostID = 2 };
		var post7 = new Post { PostID = 7, ParentPostID = 3 };
		var posts = new List<Post> { post1, post2, post3, post4, post5, post6, post7 };
		var topicContainer = new TopicContainer { Posts = posts, Topic = new Topic { TopicID = 1234 } };
		var service = GetService();
		var result = service.MapTopicContainerForQA(topicContainer);
		Assert.True(result.QuestionPostWithComments.Children.Count == 2);
		Assert.Contains(post5, result.QuestionPostWithComments.Children);
		Assert.Contains(post6, result.QuestionPostWithComments.Children);
	}

	[Fact]
	public void MapTopicContainerOrdersAnswersByVoteThenDate()
	{
		var post1 = new Post { PostID = 1, IsFirstInTopic = true };
		var post2 = new Post { PostID = 2, Votes = 7, PostTime = new DateTime(2000, 1, 1) };
		var post3 = new Post { PostID = 3, Votes = 7, PostTime = new DateTime(2000, 2, 1) };
		var post4 = new Post { PostID = 4, Votes = 2 };
		var post5 = new Post { PostID = 5, Votes = 3 };
		var post6 = new Post { PostID = 6, Votes = 8 };
		var post7 = new Post { PostID = 7, Votes = 5 };
		var posts = new List<Post> { post1, post2, post3, post4, post5, post6, post7 };
		var topic = new Topic { TopicID = 123, AnswerPostID = null };
		var topicContainer = new TopicContainer { Posts = posts, Topic = topic };
		var service = GetService();
		var result = service.MapTopicContainerForQA(topicContainer);
		Assert.Same(post6, result.AnswersWithComments[0].Post);
		Assert.Same(post3, result.AnswersWithComments[1].Post);
		Assert.Same(post2, result.AnswersWithComments[2].Post);
		Assert.Same(post7, result.AnswersWithComments[3].Post);
		Assert.Same(post5, result.AnswersWithComments[4].Post);
		Assert.Same(post4, result.AnswersWithComments[5].Post);
	}

	[Fact]
	public void MapTopicContainerOrdersAnswersByAnswerThenVoteThenDate()
	{
		var post1 = new Post { PostID = 1, IsFirstInTopic = true };
		var post2 = new Post { PostID = 2, Votes = 7, PostTime = new DateTime(2000, 1, 1) };
		var post3 = new Post { PostID = 3, Votes = 7, PostTime = new DateTime(2000, 2, 1) };
		var post4 = new Post { PostID = 4, Votes = 2 };
		var post5 = new Post { PostID = 5, Votes = 3 };
		var post6 = new Post { PostID = 6, Votes = 8 };
		var post7 = new Post { PostID = 7, Votes = 5 };
		var posts = new List<Post> { post1, post2, post3, post4, post5, post6, post7 };
		var topic = new Topic { TopicID = 123, AnswerPostID = 5};
		var topicContainer = new TopicContainer { Posts = posts, Topic = topic };
		var service = GetService();
		var result = service.MapTopicContainerForQA(topicContainer);
		Assert.Same(post5, result.AnswersWithComments[0].Post);
		Assert.Same(post6, result.AnswersWithComments[1].Post);
		Assert.Same(post3, result.AnswersWithComments[2].Post);
		Assert.Same(post2, result.AnswersWithComments[3].Post);
		Assert.Same(post7, result.AnswersWithComments[4].Post);
		Assert.Same(post4, result.AnswersWithComments[5].Post);
	}

	[Fact]
	public void MapTopicContainerDoesNotMapCommentsForTopQuestionAsReplies()
	{
		var post1 = new Post { PostID = 1, ParentPostID = 0 };
		var post2 = new Post { PostID = 2, IsFirstInTopic = true };
		var post3 = new Post { PostID = 3, ParentPostID = 0 };
		var post4 = new Post { PostID = 4, ParentPostID = 1 };
		var post5 = new Post { PostID = 5, ParentPostID = 2 };
		var post6 = new Post { PostID = 6, ParentPostID = 3 };
		var post7 = new Post { PostID = 7, ParentPostID = 3 };
		var posts = new List<Post> { post1, post2, post3, post4, post5, post6, post7 };
		var topicContainer = new TopicContainer { Posts = posts, Topic = new Topic { TopicID = 1234 } };
		var service = GetService();
		var result = service.MapTopicContainerForQA(topicContainer);
		Assert.DoesNotContain(result.AnswersWithComments, x => x.Post.PostID == post5.PostID);
	}

	[Fact]
	public void MapTopicContainerMapsLastReadTimeToQuestionAndAnswerSets()
	{
		var post1 = new Post { PostID = 1, ParentPostID = 0 };
		var post2 = new Post { PostID = 2, IsFirstInTopic = true };
		var post3 = new Post { PostID = 3, ParentPostID = 0 };
		var post4 = new Post { PostID = 4, ParentPostID = 1 };
		var post5 = new Post { PostID = 5, ParentPostID = 2 };
		var post6 = new Post { PostID = 6, ParentPostID = 3 };
		var post7 = new Post { PostID = 7, ParentPostID = 3 };
		var posts = new List<Post> { post1, post2, post3, post4, post5, post6, post7 };
		var lastRead = new DateTime(2000, 1, 1);
		var topicContainer = new TopicContainer { Posts = posts, Topic = new Topic { TopicID = 1234 }, LastReadTime = lastRead };
		var service = GetService();
		var result = service.MapTopicContainerForQA(topicContainer);
		Assert.Equal(lastRead, result.AnswersWithComments[0].LastReadTime);
		Assert.Equal(lastRead, result.AnswersWithComments[1].LastReadTime);
		Assert.Equal(lastRead, result.QuestionPostWithComments.LastReadTime);
	}

	public class ModifyForumRoles : ForumServiceTests
	{
		[Fact]
		public async Task ThrowsIfNoForumMatch()
		{
			var service = GetService();
			_mockForumRepo.Get(Arg.Any<int>()).Returns((Forum) null);

			await Assert.ThrowsAsync<Exception>(async () => await service.ModifyForumRoles(new ModifyForumRolesContainer()));
		}

		private async Task<Tuple<int, string>> CallSetup(ModifyForumRolesType modifyType)
		{
			var service = GetService();
			var forum = new Forum { ForumID = 123 };
			var role = "role";
			_mockForumRepo.Get(forum.ForumID).Returns(Task.FromResult(forum));
			await service.ModifyForumRoles(new ModifyForumRolesContainer { ForumID = forum.ForumID, ModifyType = modifyType, Role = role });
			return Tuple.Create(forum.ForumID, role);
		}

		[Fact]
		public async Task AddPostCallsRepo()
		{
			var (forumID, role) = await CallSetup(ModifyForumRolesType.AddPost);

			await _mockForumRepo.Received().AddPostRole(forumID, role);
		}

		[Fact]
		public async Task RemovePostCallsRepo()
		{
			var (forumID, role) = await CallSetup(ModifyForumRolesType.RemovePost);

			await _mockForumRepo.Received().RemovePostRole(forumID, role);
		}

		[Fact]
		public async Task AddViewCallsRepo()
		{
			var (forumID, role) = await CallSetup(ModifyForumRolesType.AddView);

			await _mockForumRepo.Received().AddViewRole(forumID, role);
		}

		[Fact]
		public async Task RemoveViewCallsRepo()
		{
			var (forumID, role) = await CallSetup(ModifyForumRolesType.RemoveView);

			await _mockForumRepo.Received().RemoveViewRole(forumID, role);
		}

		[Fact]
		public async Task RemoveAllPostCallsRepo()
		{
			var (forumID, _) = await CallSetup(ModifyForumRolesType.RemoveAllPost);

			await _mockForumRepo.Received().RemoveAllPostRoles(forumID);
		}

		[Fact]
		public async Task RemoveAllViewCallsRepo()
		{
			var (forumID, _) = await CallSetup(ModifyForumRolesType.RemoveAllView);

			await _mockForumRepo.Received().RemoveAllViewRoles(forumID);
		}
	}
}