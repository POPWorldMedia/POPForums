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
	public class SearchServiceTests
	{
		private Mock<ISettingsManager> _mockSettingsManager;
		private Mock<ISearchRepository> _mockSearchRepo;
		private Mock<IForumService> _mockForumService;
		private Mock<ISearchIndexQueueRepository> _searchIndexQueueRepo;

		private SearchService GetService()
		{
			_mockSearchRepo = new Mock<ISearchRepository>();
			_mockSettingsManager = new Mock<ISettingsManager>();
			_mockForumService = new Mock<IForumService>();
			_searchIndexQueueRepo = new Mock<ISearchIndexQueueRepository>();
			return new SearchService(_mockSearchRepo.Object, _mockSettingsManager.Object, _mockForumService.Object, _searchIndexQueueRepo.Object);
		}

		[Fact]
		public void GetJunkWords()
		{
			var words = new List<string>();
			var service = GetService();
			_mockSearchRepo.Setup(s => s.GetJunkWords()).Returns(words);
			var result = service.GetJunkWords();
			_mockSearchRepo.Verify(s => s.GetJunkWords(), Times.Once());
			Assert.Same(words, result);
		}

		[Fact]
		public void CreateWord()
		{
			var service = GetService();
			service.CreateJunkWord("blah");
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
		public void GetTopicsReturnsValidResponseWithNoResultsWhenSearchTermIsNull()
		{
			var service = GetService();
			_mockForumService.Setup(x => x.GetNonViewableForumIDs(null)).Returns(new List<int>());
			_mockSettingsManager.Setup(x => x.Current.TopicsPerPage).Returns(20);

			var result = service.GetTopics(null, SearchType.Rank, null, false, 1, out var pagerContext);

			Assert.Empty(result.Data);
			Assert.True(result.IsValid);
		}

		[Fact]
		public void GetTopicsReturnsValidResponseWithNoResultsWhenSearchTermIsEmpty()
		{
			var service = GetService();
			_mockForumService.Setup(x => x.GetNonViewableForumIDs(null)).Returns(new List<int>());
			_mockSettingsManager.Setup(x => x.Current.TopicsPerPage).Returns(20);

			var result = service.GetTopics(String.Empty, SearchType.Rank, null, false, 1, out var pagerContext);

			Assert.Empty(result.Data);
			Assert.True(result.IsValid);
		}

		[Fact]
		public void GetTopicsIsCalledWithTheRightParameters()
		{
			var query = "test";
			var user = new User();
			var noViewIDs = new List<int> {1};
			var service = GetService();
			_mockForumService.Setup(x => x.GetNonViewableForumIDs(user)).Returns(noViewIDs);
			_mockSettingsManager.Setup(x => x.Current.TopicsPerPage).Returns(20);
			int count;
			_mockSearchRepo.Setup(x => x.SearchTopics(query, noViewIDs, SearchType.Rank, 1, 20, out count)).Returns(new Response<List<Topic>>(new List<Topic>()));

			service.GetTopics(query, SearchType.Rank, user, false, 1, out var pagerContext);

			_mockSearchRepo.Verify(x => x.SearchTopics(query, noViewIDs, SearchType.Rank, 1, 20, out count), Times.Once);
		}

		[Fact]
		public void GetTopicsOutsCorrectPagerContextAndValidResult()
		{
			var query = "test";
			var user = new User();
			var noViewIDs = new List<int> { 1 };
			var list = new List<Topic>();
			var service = GetService();
			_mockForumService.Setup(x => x.GetNonViewableForumIDs(user)).Returns(noViewIDs);
			_mockSettingsManager.Setup(x => x.Current.TopicsPerPage).Returns(20);
			var count = 50;
			_mockSearchRepo.Setup(x => x.SearchTopics(query, noViewIDs, SearchType.Rank, 21, 20, out count)).Returns(new Response<List<Topic>>(list));

			var response = service.GetTopics(query, SearchType.Rank, user, false, 2, out var pagerContext);

			Assert.Equal(20, pagerContext.PageSize);
			Assert.Equal(2, pagerContext.PageIndex);
			Assert.Equal(3, pagerContext.PageCount);
			Assert.True(response.IsValid);
			Assert.Same(list, response.Data);
		}

		[Fact]
		public void GetTopicsReturnsEmptyResultWithIsValidFalseAndAnemicPagerContext()
		{
			var query = "test";
			var user = new User();
			var noViewIDs = new List<int> { 1 };
			var service = GetService();
			_mockForumService.Setup(x => x.GetNonViewableForumIDs(user)).Returns(noViewIDs);
			_mockSettingsManager.Setup(x => x.Current.TopicsPerPage).Returns(20);
			var count = 50;
			_mockSearchRepo.Setup(x => x.SearchTopics(query, noViewIDs, SearchType.Rank, 21, 20, out count)).Returns(new Response<List<Topic>>(new List<Topic>(), false));

			var response = service.GetTopics(query, SearchType.Rank, user, false, 2, out var pagerContext);

			Assert.Empty(response.Data);
			Assert.False(response.IsValid);
			Assert.Equal(1, pagerContext.PageSize);
			Assert.Equal(1, pagerContext.PageIndex);
			Assert.Equal(1, pagerContext.PageCount);
		}
	}
}