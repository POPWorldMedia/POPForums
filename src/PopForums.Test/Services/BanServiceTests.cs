using System.Threading.Tasks;
using Moq;
using PopForums.Repositories;
using PopForums.Services;
using Xunit;

namespace PopForums.Test.Services
{
	public class BanServiceTests
	{
		private Mock<IBanRepository> _banRepo;

		private IBanService GetService()
		{
			_banRepo = new Mock<IBanRepository>();
			return new BanService(_banRepo.Object);
		}

		[Fact]
		public async Task IPTrimmedOnSave()
		{
			var service = GetService();
			await service.BanIP("  1.1.1.1  ");
			_banRepo.Verify(x => x.BanIP("1.1.1.1"), Times.Once());
		}

		[Fact]
		public async Task EmailTrimmedOnSave()
		{
			var service = GetService();
			await service.BanEmail("  a@b.com  ");
			_banRepo.Verify(x => x.BanEmail("a@b.com"), Times.Once());
		}
	}
}