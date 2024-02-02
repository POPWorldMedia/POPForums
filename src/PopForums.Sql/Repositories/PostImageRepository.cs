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

	public async Task<PostImagePersistPayload> Persist(byte[] bytes, string contentType)
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
		var url = "/Forums/Image/PostImage/" + postImage.ID;
		var payload = new PostImagePersistPayload {Url = url, ID = postImage.ID};
		return payload;
	}

	public async Task DeletePostImageData(string id, string tenantID)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("DELETE FROM pf_PostImage WHERE ID = @id", new {id}));
	}

	public async Task<PostImage> GetWithoutData(string id)
	{
		Task<PostImage> image = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			image = connection.QuerySingleOrDefaultAsync<PostImage>("SELECT ID, TimeStamp, ContentType, TenantID FROM pf_PostImage WHERE ID = @id", new { id }));
		return await image;
	}

	[Obsolete("Use the combination of GetWithoutData(int) and GetImageStream(int) instead.")]
	public async Task<PostImage> Get(string id)
	{
		Task<PostImage> image = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			image = connection.QuerySingleOrDefaultAsync<PostImage>("SELECT * FROM pf_PostImage WHERE ID = @id", new {id}));
		return await image;
	}

	public async Task<IStreamResponse> GetImageStream(string id)
	{
		var connection = (SqlConnection)_sqlObjectFactory.GetConnection();
		var command = new SqlCommand("SELECT ImageData FROM pf_PostImage WHERE ID = @id", connection);
		command.Parameters.AddWithValue("id", id);
		connection.Open();
		var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
		if (await reader.ReadAsync() && !await reader.IsDBNullAsync(0))
		{
			var stream = reader.GetStream(0);
			var streamResponse = new StreamResponse(stream, connection, reader);
			return streamResponse;
		}
		return default;
	}
}