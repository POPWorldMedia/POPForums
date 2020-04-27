using System.Collections.Generic;
using Moq;
using PopForums.Repositories;
using PopForums.Services;
using Xunit;

namespace PopForums.Test.Services
{
	public class SitemapServiceTests
	{
		private SitemapService GetService()
		{
			_topicRepo = new Mock<ITopicRepository>();
			_forumRepo = new Mock<IForumRepository>();
			return new SitemapService(_topicRepo.Object, _forumRepo.Object);
		}

		private Mock<ITopicRepository> _topicRepo;
		private Mock<IForumRepository> _forumRepo;

		public class GetSitemapPageCount : SitemapServiceTests
		{
			[Fact]
			public async void ZeroTopicsReturns1()
			{
				var service = GetService();
				var list = new Dictionary<int, List<string>>();
				_forumRepo.Setup(x => x.GetForumViewRestrictionRoleGraph()).ReturnsAsync(list);
				_topicRepo.Setup(x => x.GetTopicCount(false, It.IsAny<List<int>>())).ReturnsAsync(0);

				var pageCount = await service.GetSitemapPageCount();

				Assert.Equal(1, pageCount);
			}
			
			[Fact]
			public async void MaxTopicsReturns1()
			{
				var service = GetService();
				var list = new Dictionary<int, List<string>>();
				_forumRepo.Setup(x => x.GetForumViewRestrictionRoleGraph()).ReturnsAsync(list);
				_topicRepo.Setup(x => x.GetTopicCount(false, It.IsAny<List<int>>())).ReturnsAsync(30000);

				var pageCount = await service.GetSitemapPageCount();

				Assert.Equal(1, pageCount);
			}
			
			[Fact]
			public async void MaxPlusOneTopicsReturns2()
			{
				var service = GetService();
				var list = new Dictionary<int, List<string>>();
				_forumRepo.Setup(x => x.GetForumViewRestrictionRoleGraph()).ReturnsAsync(list);
				_topicRepo.Setup(x => x.GetTopicCount(false, It.IsAny<List<int>>())).ReturnsAsync(30001);

				var pageCount = await service.GetSitemapPageCount();

				Assert.Equal(2, pageCount);
			}
			
			[Fact]
			public async void NonViewableListPassedToTopicRepoForCount()
			{
				var service = GetService();
				var list = new Dictionary<int, List<string>>
				{
					{1, new List<string>()},
					{2, new List<string>{"Admin"}},
					{3, new List<string>{"Admin","Moderator"}},
					{4, new List<string>()}
				};
				_forumRepo.Setup(x => x.GetForumViewRestrictionRoleGraph()).ReturnsAsync(list);
				var returnList = new List<int>();
				_topicRepo.Setup(x => x.GetTopicCount(false, It.IsAny<List<int>>())).ReturnsAsync(30001)
					.Callback<bool, List<int>>((b, l) => returnList = l);

				await service.GetSitemapPageCount();

				Assert.Equal(2, returnList.Count);
				Assert.Equal(2, returnList[0]);
				Assert.Equal(3, returnList[1]);
			}
		}
	}
}