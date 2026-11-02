namespace PopForums.Services.Subscriptions;

public interface ISkuService
{
	Task<List<Sku>> GetAll();
	Task<List<Sku>> GetAllActive();
	Task<Sku> Get(string skuID);
	Task<BasicServiceResponse<Sku>> Save(Sku sku);
	Task MoveSkuUp(string skuID);
	Task MoveSkuDown(string skuID);
}

public class SkuService(ISkuRepository skuRepository) : ISkuService
{
	public async Task<List<Sku>> GetAll()
	{
		return await skuRepository.GetAll();
	}

	public async Task<List<Sku>> GetAllActive()
	{
		return await skuRepository.GetAllActive();
	}

	public async Task<Sku> Get(string skuID)
	{
		return await skuRepository.Get(skuID);
	}

	public async Task<BasicServiceResponse<Sku>> Save(Sku sku)
	{
		// a subscriber's SKU ID becomes a computed role, so it can't impersonate a permanent role
		var reservedRoles = new[] { PermanentRoles.Admin, PermanentRoles.Moderator, PermanentRoles.Subscriber };
		if (reservedRoles.Any(r => string.Equals(r, sku.SkuID, StringComparison.OrdinalIgnoreCase)))
			return BasicServiceResponse<Sku>.Failed($"SKU ID can't be {sku.SkuID}, because that's a permanent role.");
		var existing = await skuRepository.Get(sku.SkuID);
		if (existing == null)
		{
			var all = await skuRepository.GetAll();
			sku.SortOrder = all.Count == 0 ? 0 : all.Max(s => s.SortOrder) + 2;
			await skuRepository.Create(sku);
		}
		else
			await skuRepository.Update(sku);
		return BasicServiceResponse<Sku>.Success(sku);
	}

	private async Task ChangeSkuSortOrder(Sku sku, int change)
	{
		var skus = await skuRepository.GetAll();
		skus.Single(s => s.SkuID == sku.SkuID).SortOrder += change;
		await SortAndUpdateSkus(skus);
	}

	private async Task SortAndUpdateSkus(IEnumerable<Sku> skus)
	{
		var sorted = skus.OrderBy(s => s.SortOrder).ToList();
		for (var i = 0; i < sorted.Count; i++)
		{
			var correctedSku = sorted[i];
			correctedSku.SortOrder = i * 2;
			await skuRepository.UpdateSortOrder(correctedSku.SkuID, correctedSku.SortOrder);
		}
	}

	public async Task MoveSkuUp(string skuID)
	{
		var sku = await skuRepository.Get(skuID);
		if (sku == null)
			throw new Exception($"Sku {skuID} doesn't exist, can't move it up.");
		const int change = -3;
		await ChangeSkuSortOrder(sku, change);
	}

	public async Task MoveSkuDown(string skuID)
	{
		var sku = await skuRepository.Get(skuID);
		if (sku == null)
			throw new Exception($"Sku {skuID} doesn't exist, can't move it down.");
		const int change = 3;
		await ChangeSkuSortOrder(sku, change);
	}
}
