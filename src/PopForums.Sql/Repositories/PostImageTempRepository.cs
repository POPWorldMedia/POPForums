namespace PopForums.Sql.Repositories;

public class PostImageTempRepository : IPostImageTempRepository
{
	private readonly ISqlObjectFactory _sqlObjectFactory;

	public PostImageTempRepository(ISqlObjectFactory sqlObjectFactory)
	{
		_sqlObjectFactory = sqlObjectFactory;
	}

	public async Task Save(Guid postImageTempID, DateTime timeStamp, string tenantID)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("INSERT INTO pf_PostImageTemp (PostImageTempID, TimeStamp, TenantID) VALUES (@postImageTempID, @timeStamp, @tenantID)", new { postImageTempID, timeStamp, tenantID }));
	}

	public async Task Delete(Guid id)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("DELETE FROM pf_PostImageTemp WHERE PostImageTempID = @id", new { id }));
	}

	public async Task<List<Guid>> GetOld(DateTime olderThan)
	{
		Task<IEnumerable<Guid>> list = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			list = connection.QueryAsync<Guid>("SELECT PostImageTempID FROM pf_PostImageTemp WHERE TimeStamp < @olderThan", new { olderThan }));
		return list.Result.ToList();
	}
}