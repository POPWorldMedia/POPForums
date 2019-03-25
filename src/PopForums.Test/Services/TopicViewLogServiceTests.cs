using System;
using System.Threading.Tasks;
using Moq;
using PopForums.Configuration;
using PopForums.Repositories;
using PopForums.Services;
using Xunit;

namespace PopForums.Test.Services
{
	public class TopicViewLogServiceTests
	{
		private TopicViewLogService GetService()
		{
			_config = new Mock<IConfig>();
			_topicViewLogRepo = new Mock<ITopicViewLogRepository>();
			return new TopicViewLogService(_config.Object, _topicViewLogRepo.Object);
		}

		private Mock<IConfig> _config;
		private Mock<ITopicViewLogRepository> _topicViewLogRepo;

		[Fact]
		public async Task LogViewDoesNotCallRepoWhenConfigIsFalse()
		{
			var service = GetService();
			_config.Setup(x => x.LogTopicViews).Returns(false);

			await service.LogView(123, 456);

			_topicViewLogRepo.Verify(x => x.Log(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
		}

		[Fact]
		public async Task LogViewDoesCallsRepoWhenConfigIsTrue()
		{
			var service = GetService();
			_config.Setup(x => x.LogTopicViews).Returns(true);

			await service.LogView(123, 456);

			_topicViewLogRepo.Verify(x => x.Log(123, 456, It.IsAny<DateTime>()), Times.Once);
		}
	}
}