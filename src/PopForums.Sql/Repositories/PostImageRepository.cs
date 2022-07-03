namespace PopForums.Sql.Repositories;

public class PostImageRepository : IPostImageRepository
{
	private readonly ISqlObjectFactory _sqlObjectFactory;
	private readonly ITenantService _tenantService;

	public PostImageRepository(ISqlObjectFactory sqlObjectFactory, ITenantService tenantService)
	{
		_sqlObjectFactory = sqlObjectFactory;
		_tenantService = tenantService;
	}

	public async Task<string> Persist(byte[] bytes, string contentType)
	{
		var guid = Guid.NewGuid();
		var tenantID = _tenantService.GetTenant();
		var postImage = new PostImage
		{
			ID = guid.ToString(),
			TimeStamp = DateTime.UtcNow,
			ContentType = contentType,
			TenantID = tenantID,
			ImageData = bytes
		};
		await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
			connection.ExecuteAsync("INSERT INTO pf_PostImage (ID, TimeStamp, ContentType, TenantID, ImageData) VALUES (@ID, @TimeStamp, @ContentType, @TenantID, @ImageData)", postImage));
		return "/Forums/Image/PostImage/" + postImage.ID;
	}

	public async Task<PostImage> GetWithoutData(string id)
	{
		Task<PostImage> image = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			image = connection.QuerySingleOrDefaultAsync<PostImage>("SELECT ID, TimeStamp, ContentType, TenantID FROM pf_PostImage WHERE ID = @id", new { id }));
		return await image;
	}

	public async Task<PostImage> Get(string id)
	{
		Task<PostImage> image = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			image = connection.QuerySingleOrDefaultAsync<PostImage>("SELECT * FROM pf_PostImage WHERE ID = @id", new {id}));
		return await image;
	}
}