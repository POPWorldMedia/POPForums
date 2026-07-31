namespace PopForums.Repositories;

public interface ISkuRepository
{
	Task Create(Sku sku);
	Task Update(Sku sku);
	Task<Sku> Get(string skuID);
	Task<List<Sku>> GetAll();
	Task<List<Sku>> GetAllActive();
}
