namespace PopForums.Services.Subscriptions;

public interface ISkuService
{
	Task<List<Sku>> GetAll();
	Task<List<Sku>> GetAllActive();
	Task<Sku> Get(string skuID);
	Task Save(Sku sku);
}

public class SkuService(ISkuRepository skuRepository) : ISkuService
{
	public async Task<List<Sku>> GetAll()
	{
		var skus = await skuRepository.GetAll();
		return skus.OrderByDescending(s => s.IsActive).ThenBy(s => s.Name).ToList();
	}

	public async Task<List<Sku>> GetAllActive()
	{
		var skus = await skuRepository.GetAllActive();
		return skus.OrderBy(s => s.Name).ToList();
	}

	public async Task<Sku> Get(string skuID)
	{
		return await skuRepository.Get(skuID);
	}

	public async Task Save(Sku sku)
	{
		var existing = await skuRepository.Get(sku.SkuID);
		if (existing == null)
			await skuRepository.Create(sku);
		else
			await skuRepository.Update(sku);
	}
}
