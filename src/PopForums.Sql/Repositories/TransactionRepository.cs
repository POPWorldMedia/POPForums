namespace PopForums.Sql.Repositories;

public class TransactionRepository : ITransactionRepository
{
	public TransactionRepository(ISqlObjectFactory sqlObjectFactory)
	{
		_sqlObjectFactory = sqlObjectFactory;
	}

	private readonly ISqlObjectFactory _sqlObjectFactory;

	public async Task Create(Transaction transaction)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("INSERT INTO pf_Transaction (ProcessorID, CustomerID, Status, Raw, Last4, UserID, TimeStamp, SkuID, Amount) VALUES (@ProcessorID, @CustomerID, @Status, @Raw, @Last4, @UserID, @TimeStamp, @SkuID, @Amount)", new { ProcessorID = transaction.ProcessorID.NullToEmpty(), CustomerID = transaction.CustomerID.NullToEmpty(), Status = transaction.Status.NullToEmpty(), Raw = transaction.Raw.NullToEmpty(), Last4 = transaction.Last4.NullToEmpty(), transaction.UserID, transaction.TimeStamp, transaction.SkuID, transaction.Amount }));
	}

	public async Task<List<Transaction>> GetByUserID(int userID)
	{
		Task<IEnumerable<Transaction>> result = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			result = connection.QueryAsync<Transaction>("SELECT TransactionID, ProcessorID, CustomerID, Status, Raw, Last4, UserID, TimeStamp, SkuID, Amount FROM pf_Transaction WHERE UserID = @UserID ORDER BY TimeStamp", new { UserID = userID }));
		return result.Result.ToList();
	}

	public async Task<Transaction> GetLastTransaction(int userID)
	{
		Task<Transaction> transaction = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			transaction = connection.QuerySingleOrDefaultAsync<Transaction>("SELECT TOP 1 TransactionID, ProcessorID, CustomerID, Status, Raw, Last4, UserID, TimeStamp, SkuID, Amount FROM pf_Transaction WHERE UserID = @UserID ORDER BY TimeStamp DESC", new { UserID = userID }));
		return await transaction;
	}
}
