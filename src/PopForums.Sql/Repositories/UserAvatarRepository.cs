namespace PopForums.Sql.Repositories;

public class UserAvatarRepository : IUserAvatarRepository
{
	public UserAvatarRepository(ISqlObjectFactory sqlObjectFactory)
	{
		_sqlObjectFactory = sqlObjectFactory;
	}

	private readonly ISqlObjectFactory _sqlObjectFactory;

	[Obsolete("Use GetImageStream(int) instead.")]
	public async Task<byte[]> GetImageData(int userAvatarID)
	{
		Task<byte[]> data = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			data = connection.ExecuteScalarAsync<byte[]>("SELECT ImageData FROM pf_UserAvatar WHERE UserAvatarID = @UserAvatarID", new { UserAvatarID = userAvatarID }));
		return await data;
	}

	public async Task<IStreamResponse> GetImageStream(int userAvatarID)
	{
		var connection = (SqlConnection)_sqlObjectFactory.GetConnection();
		var command = new SqlCommand("SELECT ImageData FROM pf_UserAvatar WHERE UserAvatarID = @UserAvatarID", connection);
		command.Parameters.AddWithValue("@UserAvatarID", userAvatarID);
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

	public async Task<List<int>> GetUserAvatarIDs(int userID)
	{
		Task<IEnumerable<int>> ids = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			ids = connection.QueryAsync<int>("SELECT UserAvatarID FROM pf_UserAvatar WHERE UserID = @UserID",
				new {UserID = userID}));
		return ids.Result.ToList();
	}

	public async Task<int> SaveNewAvatar(int userID, byte[] imageData, DateTime timeStamp)
	{
		Task<int> avatarID = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			avatarID = connection.QuerySingleAsync<int>("INSERT INTO pf_UserAvatar (UserID, ImageData, [TimeStamp]) VALUES (@UserID, @ImageData, @TimeStamp);SELECT CAST(SCOPE_IDENTITY() as int)", new { UserID = userID, TimeStamp = timeStamp, ImageData = imageData }));
		return await avatarID;
	}

	public async Task DeleteAvatarsByUserID(int userID)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("DELETE FROM pf_UserAvatar WHERE UserID = @UserID", new { UserID = userID }));
	}

	public async Task<DateTime?> GetLastModificationDate(int userAvatarID)
	{
		Task<DateTime?> timeStamp = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			timeStamp = connection.QuerySingleOrDefaultAsync<DateTime?>("SELECT [TimeStamp] FROM pf_UserAvatar WHERE UserAvatarID = @UserAvatarID", new { UserAvatarID = userAvatarID }));
		return await timeStamp;
	}
}