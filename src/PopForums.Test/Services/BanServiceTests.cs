namespace PopForums.Test.Services;

public class BanServiceTests
{
	private IBanRepository _banRepo;

	private IBanService GetService()
	{
		_banRepo = Substitute.For<IBanRepository>();
		return new BanService(_banRepo);
	}

	[Fact]
	public async Task IPTrimmedOnSave()
	{
		var service = GetService();
		await service.BanIP("  1.1.1.1  ");
		await _banRepo.Received().BanIP("1.1.1.1");
	}

	[Fact]
	public async Task EmailTrimmedOnSave()
	{
		var service = GetService();
		await service.BanEmail("  a@b.com  ");
		await _banRepo.Received().BanEmail("a@b.com");
	}
}