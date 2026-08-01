namespace PopForums.Test.Services.Subscriptions;

public class SkuServiceTests
{
	private ISkuRepository _mockSkuRepo;

	private SkuService GetService()
	{
		_mockSkuRepo = Substitute.For<ISkuRepository>();
		return new SkuService(_mockSkuRepo);
	}

	[Fact]
	public async Task GetAll()
	{
		var service = GetService();
		var skus = new List<Sku> { new Sku { SkuID = "a" } };
		_mockSkuRepo.GetAll().Returns(Task.FromResult(skus));
		var result = await service.GetAll();
		Assert.Same(skus, result);
	}

	[Fact]
	public async Task GetAllActive()
	{
		var service = GetService();
		var skus = new List<Sku> { new Sku { SkuID = "a", IsActive = true } };
		_mockSkuRepo.GetAllActive().Returns(Task.FromResult(skus));
		var result = await service.GetAllActive();
		Assert.Same(skus, result);
	}

	[Fact]
	public async Task Get()
	{
		const string skuID = "sku1";
		var service = GetService();
		_mockSkuRepo.Get(skuID).Returns(Task.FromResult(new Sku { SkuID = skuID }));
		var result = await service.Get(skuID);
		Assert.Equal(skuID, result.SkuID);
	}

	[Fact]
	public async Task SaveUpdatesExisting()
	{
		var service = GetService();
		var sku = new Sku { SkuID = "sku1" };
		_mockSkuRepo.Get(sku.SkuID).Returns(Task.FromResult(new Sku { SkuID = sku.SkuID }));
		await service.Save(sku);
		await _mockSkuRepo.Received().Update(sku);
		await _mockSkuRepo.DidNotReceive().Create(Arg.Any<Sku>());
	}

	[Fact]
	public async Task SaveNewSkuAppendsToEnd()
	{
		var service = GetService();
		var sku = new Sku { SkuID = "newSku" };
		_mockSkuRepo.Get(sku.SkuID).Returns(Task.FromResult((Sku)null));
		var existing = new List<Sku> { new Sku { SkuID = "a", SortOrder = 0 }, new Sku { SkuID = "b", SortOrder = 4 } };
		_mockSkuRepo.GetAll().Returns(Task.FromResult(existing));
		await service.Save(sku);
		Assert.Equal(6, sku.SortOrder);
		await _mockSkuRepo.Received().Create(sku);
	}

	[Fact]
	public async Task SaveFirstSkuIsSortOrderZero()
	{
		var service = GetService();
		var sku = new Sku { SkuID = "newSku" };
		_mockSkuRepo.Get(sku.SkuID).Returns(Task.FromResult((Sku)null));
		_mockSkuRepo.GetAll().Returns(Task.FromResult(new List<Sku>()));
		await service.Save(sku);
		Assert.Equal(0, sku.SortOrder);
	}

	[Fact]
	public async Task MoveUp()
	{
		var s1 = new Sku { SkuID = "s1", SortOrder = 0 };
		var s2 = new Sku { SkuID = "s2", SortOrder = 2 };
		var s3 = new Sku { SkuID = "s3", SortOrder = 4 };
		var s4 = new Sku { SkuID = "s4", SortOrder = 6 };
		var skus = new List<Sku> { s1, s2, s3, s4 };
		var service = GetService();
		_mockSkuRepo.Get(s3.SkuID).Returns(Task.FromResult(s3));
		_mockSkuRepo.GetAll().Returns(Task.FromResult(skus));

		await service.MoveSkuUp(s3.SkuID);

		await _mockSkuRepo.Received(4).UpdateSortOrder(Arg.Any<string>(), Arg.Any<int>());
		await _mockSkuRepo.Received().UpdateSortOrder(s1.SkuID, s1.SortOrder);
		await _mockSkuRepo.Received().UpdateSortOrder(s2.SkuID, s2.SortOrder);
		await _mockSkuRepo.Received().UpdateSortOrder(s3.SkuID, s3.SortOrder);
		await _mockSkuRepo.Received().UpdateSortOrder(s4.SkuID, s4.SortOrder);
		Assert.Equal(0, s1.SortOrder);
		Assert.Equal(2, s3.SortOrder);
		Assert.Equal(4, s2.SortOrder);
		Assert.Equal(6, s4.SortOrder);
	}

	[Fact]
	public async Task MoveDown()
	{
		var s1 = new Sku { SkuID = "s1", SortOrder = 0 };
		var s2 = new Sku { SkuID = "s2", SortOrder = 2 };
		var s3 = new Sku { SkuID = "s3", SortOrder = 4 };
		var s4 = new Sku { SkuID = "s4", SortOrder = 6 };
		var skus = new List<Sku> { s1, s2, s3, s4 };
		var service = GetService();
		_mockSkuRepo.Get(s3.SkuID).Returns(Task.FromResult(s3));
		_mockSkuRepo.GetAll().Returns(Task.FromResult(skus));

		await service.MoveSkuDown(s3.SkuID);

		await _mockSkuRepo.Received(4).UpdateSortOrder(Arg.Any<string>(), Arg.Any<int>());
		Assert.Equal(0, s1.SortOrder);
		Assert.Equal(2, s2.SortOrder);
		Assert.Equal(4, s4.SortOrder);
		Assert.Equal(6, s3.SortOrder);
	}

	[Fact]
	public async Task MoveSkuUpThrowsIfNoSku()
	{
		var service = GetService();
		_mockSkuRepo.Get(Arg.Any<string>()).Returns((Sku)null);

		await Assert.ThrowsAsync<Exception>(async () => await service.MoveSkuUp("nope"));
	}

	[Fact]
	public async Task MoveSkuDownThrowsIfNoSku()
	{
		var service = GetService();
		_mockSkuRepo.Get(Arg.Any<string>()).Returns((Sku)null);

		await Assert.ThrowsAsync<Exception>(async () => await service.MoveSkuDown("nope"));
	}
}
