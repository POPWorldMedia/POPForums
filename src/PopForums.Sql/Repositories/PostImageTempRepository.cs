namespace PopForums.Sql.Repositories;

public class PostImageTempRepository : IPostImageTempRepository
{
	private readonly ISqlObjectFactory _sqlObjectFactory;

	public PostImageTempRepository(ISqlObjectFactory sqlObjectFactory)
	{
		_sqlObjectFactory = sqlObjectFactory;
	}

	public async Task Save(Guid postImageTempID, DateTime timeStamp)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("INSERT INTO pf_PostImageTemp (PostImageTempID, TimeStamp) VALUES (@postImageTempID, @timeStamp)", new { postImageTempID, timeStamp }));
	}

	public async Task Delete(Guid id)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("DELETE FROM pf_PostImageTemp WHERE PostImageTempID = @id", new { id }));
	}

	public async Task Purge(DateTime olderThan)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("DELETE FROM pf_PostImageTemp WHERE TimeStamp < @olderThan", new { olderThan }));
	}
}