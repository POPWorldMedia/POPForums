namespace PopForums.Sql.Repositories;

public class RenewalEnqueueClaimRepository : IRenewalEnqueueClaimRepository
{
	private readonly ISqlObjectFactory _sqlObjectFactory;

	public RenewalEnqueueClaimRepository(ISqlObjectFactory sqlObjectFactory)
	{
		_sqlObjectFactory = sqlObjectFactory;
	}

	public async Task<bool> TryClaim(DateOnly date)
	{
		Task<int> rowsAffected = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			rowsAffected = connection.ExecuteAsync("UPDATE pf_RenewalEnqueueClaim WITH (UPDLOCK, ROWLOCK) SET ClaimDate = @date WHERE ClaimDate < @date", new { date }));
		return rowsAffected.Result == 1;
	}
}
