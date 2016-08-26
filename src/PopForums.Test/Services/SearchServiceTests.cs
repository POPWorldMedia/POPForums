using System.Collections.Generic;
using Moq;
using Xunit;
using PopForums.Configuration;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
	public class SearchServiceTests
	{
		private Mock<ISettingsManager> _mockSettingsManager;
		private Mock<ISearchRepository> _mockSearchRepo;
		private Mock<IForumService> _mockForumService;

		private SearchService GetService()
		{
			_mockSearchRepo = new Mock<ISearchRepository>();
			_mockSettingsManager = new Mock<ISettingsManager>();
			_mockForumService = new Mock<IForumService>();
			return new SearchService(_mockSearchRepo.Object, _mockSettingsManager.Object, _mockForumService.Object);
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
	}
}