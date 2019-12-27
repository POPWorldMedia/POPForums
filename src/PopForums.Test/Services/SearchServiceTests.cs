using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
	public class SearchServiceTests
	{
		private Mock<ISettingsManager> _mockSettingsManager;
		private Mock<ISearchRepository> _mockSearchRepo;
		private Mock<IForumService> _mockForumService;
		private Mock<ISearchIndexQueueRepository> _searchIndexQueueRepo;
		private Mock<IErrorLog> _errorLog;

		private SearchService GetService()
		{
			_mockSearchRepo = new Mock<ISearchRepository>();
			_mockSettingsManager = new Mock<ISettingsManager>();
			_mockForumService = new Mock<IForumService>();
			_searchIndexQueueRepo = new Mock<ISearchIndexQueueRepository>();
			_errorLog = new Mock<IErrorLog>();
			return new SearchService(_mockSearchRepo.Object, _mockSettingsManager.Object, _mockForumService.Object, _searchIndexQueueRepo.Object, _errorLog.Object);
		}

		[Fact]
		public async Task GetJunkWords()
		{
			var words = new List<string>();
			var service = GetService();
			_mockSearchRepo.Setup(s => s.GetJunkWords()).ReturnsAsync(words);
			var result = await service.GetJunkWords();
			_mockSearchRepo.Verify(s => s.GetJunkWords(), Times.Once());
			Assert.Same(words, result);
		}

		[Fact]
		public async Task CreateWord()
		{
			var service = GetService();
			await service.CreateJunkWord("blah");
			_mockSearchRepo.Verify(s => s.CreateJunkWord("blah"), Times.Once());
		}

		[Fact]
		public void DeleteWord()
		{
			var service = GetService();
			service.DeleteJunkWord("blah");
			_mockSearchRepo.Verify(s => s.DeleteJunkWord("blah"), Times.Once());
		}

		[Fact]
		public async Task GetTopicsReturnsValidResponseWithNoResultsWhenSearchTermIsNull()
		{
			var service = GetService();
			_mockForumService.Setup(x => x.GetNonViewableForumIDs(null)).ReturnsAsync(new List<int>());
			_mockSettingsManager.Setup(x => x.Current.TopicsPerPage).Returns(20);

			var result = await service.GetTopics(null, SearchType.Rank, null, false, 1);

			Assert.Empty(result.Item1.Data);
			Assert.True(result.Item1.IsValid);
		}

		[Fact]
		public async Task GetTopicsReturnsValidResponseWithNoResultsWhenSearchTermIsEmpty()
		{
			var service = GetService();
			_mockForumService.Setup(x => x.GetNonViewableForumIDs(null)).ReturnsAsync(new List<int>());
			_mockSettingsManager.Setup(x => x.Current.TopicsPerPage).Returns(20);

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
			_mockForumService.Setup(x => x.GetNonViewableForumIDs(user)).ReturnsAsync(noViewIDs);
			_mockSettingsManager.Setup(x => x.Current.TopicsPerPage).Returns(20);
			_mockSearchRepo.Setup(x => x.SearchTopics(query, noViewIDs, SearchType.Rank, 1, 20)).ReturnsAsync(Tuple.Create(new Response<List<Topic>>(new List<Topic>()), 0));

			await service.GetTopics(query, SearchType.Rank, user, false, 1);

			_mockSearchRepo.Verify(x => x.SearchTopics(query, noViewIDs, SearchType.Rank, 1, 20), Times.Once);
		}

		[Fact]
		public async Task GetTopicsOutsCorrectPagerContextAndValidResult()
		{
			var query = "test";
			var user = new User();
			var noViewIDs = new List<int> { 1 };
			var list = new List<Topic>();
			var service = GetService();
			_mockForumService.Setup(x => x.GetNonViewableForumIDs(user)).ReturnsAsync(noViewIDs);
			_mockSettingsManager.Setup(x => x.Current.TopicsPerPage).Returns(20);
			var count = 50;
			_mockSearchRepo.Setup(x => x.SearchTopics(query, noViewIDs, SearchType.Rank, 21, 20)).ReturnsAsync(Tuple.Create(new Response<List<Topic>>(list), count));

			var (response, pagerContext) = await service.GetTopics(query, SearchType.Rank, user, false, 2);

			Assert.Equal(20, pagerContext.PageSize);
			Assert.Equal(2, pagerContext.PageIndex);
			Assert.Equal(3, pagerContext.PageCount);
			Assert.True(response.IsValid);
			Assert.Same(list, response.Data);
		}

		[Fact]
		public async Task GetTopicsReturnsEmptyResultWithIsValidFalseAndAnemicPagerContext()
		{
			var query = "test";
			var user = new User();
			var noViewIDs = new List<int> { 1 };
			var service = GetService();
			_mockForumService.Setup(x => x.GetNonViewableForumIDs(user)).ReturnsAsync(noViewIDs);
			_mockSettingsManager.Setup(x => x.Current.TopicsPerPage).Returns(20);
			var count = 50;
			_mockSearchRepo.Setup(x => x.SearchTopics(query, noViewIDs, SearchType.Rank, 21, 20)).ReturnsAsync(Tuple.Create(new Response<List<Topic>>(new List<Topic>(), false), count));

			var (response, pagerContext) = await service.GetTopics(query, SearchType.Rank, user, false, 2);

			Assert.Empty(response.Data);
			Assert.False(response.IsValid);
			Assert.Equal(1, pagerContext.PageSize);
			Assert.Equal(1, pagerContext.PageIndex);
			Assert.Equal(1, pagerContext.PageCount);
		}
	}
}