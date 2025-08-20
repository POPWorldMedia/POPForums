namespace PopForums.Sql.Repositories;

public class IgnoreRepository(ISqlObjectFactory sqlObjectFactory) : IIgnoreRepository
{
	public async Task AddIgnore(int userID, int ignoreUserID)
    {
        await sqlObjectFactory.GetConnection().UsingAsync(async connection =>
        {
            await connection.ExecuteAsync(
                @"INSERT INTO pf_Ignore (UserID, IgnoreUserID) VALUES (@UserID, @IgnoreUserID)",
                new { UserID = userID, IgnoreUserID = ignoreUserID });
        });
    }

    public async Task DeleteIgnore(int userID, int ignoreUserID)
    {
        await sqlObjectFactory.GetConnection().UsingAsync(async connection =>
        {
            await connection.ExecuteAsync(
                @"DELETE FROM pf_Ignore WHERE UserID = @UserID AND IgnoreUserID = @IgnoreUserID",
                new { UserID = userID, IgnoreUserID = ignoreUserID });
        });
    }

    public async Task<IList<IgnoreWithName>> GetIgnoreList(int userID)
    {
	    Task<IEnumerable<IgnoreWithName>> result = null;
	    await sqlObjectFactory.GetConnection().UsingAsync(connection =>
		    result = connection.QueryAsync<IgnoreWithName>("SELECT I.UserID, IgnoreUserID, Name FROM pf_Ignore I JOIN pf_PopForumsUser U ON I.UserID = U.UserID WHERE UserID = @UserID", new { UserID = userID }));
	    return result.Result.ToList();
    }
    
    public async Task<List<int>> GetIgnoredUserIdsInList(int userID, List<int> userIDs)
    {
        if (userIDs == null || userIDs.Count == 0)
            return new List<int>();
        var inList = userIDs.Aggregate(string.Empty, (current, id) => current + ("," + id));
        if (inList.StartsWith(","))
            inList = inList.Remove(0, 1);
        var sql = $"SELECT IgnoreUserID FROM pf_Ignore WHERE UserID = @UserID AND IgnoreUserID IN ({inList})";
        Task<IEnumerable<int>> result = null;
        await sqlObjectFactory.GetConnection().UsingAsync(connection =>
            result = connection.QueryAsync<int>(sql, new { UserID = userID }));
        return result.Result.ToList();
    }
}