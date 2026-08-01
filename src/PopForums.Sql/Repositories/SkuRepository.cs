namespace PopForums.Sql.Repositories;

public class SkuRepository : ISkuRepository
{
	public SkuRepository(ISqlObjectFactory sqlObjectFactory)
	{
		_sqlObjectFactory = sqlObjectFactory;
	}

	private readonly ISqlObjectFactory _sqlObjectFactory;

	public async Task Create(Sku sku)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("INSERT INTO pf_Sku (SkuID, Name, Description, Price, IsActive, Months, SortOrder) VALUES (@SkuID, @Name, @Description, @Price, @IsActive, @Months, @SortOrder)", new { sku.SkuID, sku.Name, Description = sku.Description.NullToEmpty(), sku.Price, sku.IsActive, Months = (short)sku.Months, sku.SortOrder }));
	}

	public async Task Update(Sku sku)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("UPDATE pf_Sku SET Name = @Name, Description = @Description, Price = @Price, IsActive = @IsActive, Months = @Months WHERE SkuID = @SkuID", new { sku.SkuID, sku.Name, Description = sku.Description.NullToEmpty(), sku.Price, sku.IsActive, Months = (short)sku.Months }));
	}

	public async Task<Sku> Get(string skuID)
	{
		Task<Sku> sku = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			sku = connection.QuerySingleOrDefaultAsync<Sku>("SELECT SkuID, Name, Description, Price, IsActive, Months, SortOrder FROM pf_Sku WHERE SkuID = @SkuID", new { SkuID = skuID }));
		return await sku;
	}

	public async Task<List<Sku>> GetAll()
	{
		Task<IEnumerable<Sku>> result = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			result = connection.QueryAsync<Sku>("SELECT SkuID, Name, Description, Price, IsActive, Months, SortOrder FROM pf_Sku ORDER BY SortOrder"));
		return result.Result.ToList();
	}

	public async Task<List<Sku>> GetAllActive()
	{
		Task<IEnumerable<Sku>> result = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			result = connection.QueryAsync<Sku>("SELECT SkuID, Name, Description, Price, IsActive, Months, SortOrder FROM pf_Sku WHERE IsActive = 1 ORDER BY SortOrder"));
		return result.Result.ToList();
	}

	public async Task UpdateSortOrder(string skuID, int newSortOrder)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("UPDATE pf_Sku SET SortOrder = @SortOrder WHERE SkuID = @SkuID", new { SortOrder = newSortOrder, SkuID = skuID }));
	}
}
