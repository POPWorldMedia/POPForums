namespace PopForums.Test.Services;

public class SearchServiceTests
{
	private ISettingsManager _mockSettingsManager;
	private ISearchRepository _mockSearchRepo;
	private IForumService _mockForumService;
	private ISearchIndexQueueRepository _searchIndexQueueRepo;
	private IErrorLog _errorLog;

	private SearchService GetService()
	{
		_mockSearchRepo = Substitute.For<ISearchRepository>();
		_mockSettingsManager = Substitute.For<ISettingsManager>();
		_mockForumService = Substitute.For<IForumService>();
		_searchIndexQueueRepo = Substitute.For<ISearchIndexQueueRepository>();
		_errorLog = Substitute.For<IErrorLog>();
		return new SearchService(_mockSearchRepo, _mockSettingsManager, _mockForumService, _searchIndexQueueRepo, _errorLog);
	}

	[Fact]
	public async Task GetJunkWords()
	{
		var words = new List<string>();
		var service = GetService();
		_mockSearchRepo.GetJunkWords().Returns(Task.FromResult(words));
		var result = await service.GetJunkWords();
		await _mockSearchRepo.Received().GetJunkWords();
		Assert.Same(words, result);
	}

	[Fact]
	public async Task CreateWord()
	{
		var service = GetService();
		await service.CreateJunkWord("blah");
		await _mockSearchRepo.Received().CreateJunkWord("blah");
	}

	[Fact]
	public async Task DeleteWord()
	{
		var service = GetService();
		await service.DeleteJunkWord("blah");
		await _mockSearchRepo.Received().DeleteJunkWord("blah");
	}

	[Fact]
	public async Task GetTopicsReturnsValidResponseWithNoResultsWhenSearchTermIsNull()
	{
		var service = GetService();
		_mockForumService.GetNonViewableForumIDs(null).Returns(Task.FromResult(new List<int>()));
		_mockSettingsManager.Current.TopicsPerPage.Returns(20);

		var result = await service.GetTopics(null, SearchType.Rank, null, false, 1);

		Assert.Empty(result.Item1.Data);
		Assert.True(result.Item1.IsValid);
	}

	[Fact]
	public async Task GetTopicsReturnsValidResponseWithNoResultsWhenSearchTermIsEmpty()
	{
		var service = GetService();
		_mockForumService.GetNonViewableForumIDs(null).Returns(Task.FromResult(new List<int>()));
		_mockSettingsManager.Current.TopicsPerPage.Returns(20);

		var result = await service.GetTopics(String.Empty, SearchType.Rank, null, false, 1);

		Assert.Empty(result.Item1.Data);
		Assert.True(result.Item1.IsValid);
	}

	[Fact]
	public async Task GetTopicsIsCalledWithTheRightParameters()
	{
		var query = "test";
		var user = new User();
		var noViewIDs = new List<int> {1};
		var service = GetService();
		_mockForumService.GetNonViewableForumIDs(user).Returns(Task.FromResult(noViewIDs));
		_mockSettingsManager.Current.TopicsPerPage.Returns(20);
		_mockSearchRepo.SearchTopics(query, noViewIDs, SearchType.Rank, 1, 20).Returns((Tuple.Create(new Response<List<Topic>>(new List<Topic>()), 0)));

		await service.GetTopics(query, SearchType.Rank, user, false, 1);

		await _mockSearchRepo.Received().SearchTopics(query, noViewIDs, SearchType.Rank, 1, 20);
	}

	[Fact]
	public async Task GetTopicsOutsCorrectPagerContextAndValidResult()
	{
		var query = "test";
		var user = new User();
		var noViewIDs = new List<int> { 1 };
		var list = new List<Topic>();
		var service = GetService();
		_mockForumService.GetNonViewableForumIDs(user).Returns(Task.FromResult(noViewIDs));
		_mockSettingsManager.Current.TopicsPerPage.Returns(20);
		var count = 50;
		_mockSearchRepo.SearchTopics(query, noViewIDs, SearchType.Rank, 21, 20).Returns(Tuple.Create(new Response<List<Topic>>(list), count));

		var (response, pagerContext) = await service.GetTopics(query, SearchType.Rank, user, false, 2);

		Assert.Equal(20, pagerContext.PageSize);
		Assert.Equal(2, pagerContext.PageIndex);
		Assert.Equal(3, pagerContext.PageCount);
		Assert.True(response.IsValid);
		Assert.Same(list, response.Data);
	}

	[Fact]
	public async Task GetTopicsReturnsEmptyResultIsValidFalseAndAnemicPagerContext()
	{
		var query = "test";
		var user = new User();
		var noViewIDs = new List<int> { 1 };
		var service = GetService();
		_mockForumService.GetNonViewableForumIDs(user).Returns(Task.FromResult(noViewIDs));
		_mockSettingsManager.Current.TopicsPerPage.Returns(20);
		var count = 50;
		_mockSearchRepo.SearchTopics(query, noViewIDs, SearchType.Rank, 21, 20).Returns(Tuple.Create(new Response<List<Topic>>(new List<Topic>(), false), count));

		var (response, pagerContext) = await service.GetTopics(query, SearchType.Rank, user, false, 2);

		Assert.Empty(response.Data);
		Assert.False(response.IsValid);
		Assert.Equal(1, pagerContext.PageSize);
		Assert.Equal(1, pagerContext.PageIndex);
		Assert.Equal(1, pagerContext.PageCount);
	}
}