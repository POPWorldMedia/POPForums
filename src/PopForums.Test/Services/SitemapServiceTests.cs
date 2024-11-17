namespace PopForums.Test.Services;

public class SitemapServiceTests
{
	private SitemapService GetService()
	{
		_topicRepo = Substitute.For<ITopicRepository>();
		_forumRepo = Substitute.For<IForumRepository>();
		return new SitemapService(_topicRepo, _forumRepo);
	}

	private ITopicRepository _topicRepo;
	private IForumRepository _forumRepo;

	public class GetSitemapPageCount : SitemapServiceTests
	{
		[Fact]
		public async Task ZeroTopicsReturns1()
		{
			var service = GetService();
			var list = new Dictionary<int, List<string>>();
			_forumRepo.GetForumViewRestrictionRoleGraph().Returns(Task.FromResult(list));
			_topicRepo.GetTopicCount(false, Arg.Any<List<int>>()).Returns(Task.FromResult(0));

			var pageCount = await service.GetSitemapPageCount();

			Assert.Equal(1, pageCount);
		}
			
		[Fact]
		public async Task MaxTopicsReturns1()
		{
			var service = GetService();
			var list = new Dictionary<int, List<string>>();
			_forumRepo.GetForumViewRestrictionRoleGraph().Returns(Task.FromResult(list));
			_topicRepo.GetTopicCount(false, Arg.Any<List<int>>()).Returns(Task.FromResult(30000));

			var pageCount = await service.GetSitemapPageCount();

			Assert.Equal(1, pageCount);
		}
			
		[Fact]
		public async Task MaxPlusOneTopicsReturns2()
		{
			var service = GetService();
			var list = new Dictionary<int, List<string>>();
			_forumRepo.GetForumViewRestrictionRoleGraph().Returns(Task.FromResult(list));
			_topicRepo.GetTopicCount(false, Arg.Any<List<int>>()).Returns(Task.FromResult(30001));

			var pageCount = await service.GetSitemapPageCount();

			Assert.Equal(2, pageCount);
		}
			
		[Fact]
		public async Task NonViewableListPassedToTopicRepoForCount()
		{
			var service = GetService();
			var list = new Dictionary<int, List<string>>
			{
				{1, new List<string>()},
				{2, new List<string>{"Admin"}},
				{3, new List<string>{"Admin","Moderator"}},
				{4, new List<string>()}
			};
			_forumRepo.GetForumViewRestrictionRoleGraph().Returns(Task.FromResult(list));
			var returnList = new List<int>();
			_topicRepo.GetTopicCount(false, Arg.Do<List<int>>(x => returnList = x)).Returns(Task.FromResult(30001));

			await service.GetSitemapPageCount();

			Assert.Equal(2, returnList.Count);
			Assert.Equal(2, returnList[0]);
			Assert.Equal(3, returnList[1]);
		}
	}
}