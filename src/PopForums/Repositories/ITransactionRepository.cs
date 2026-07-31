namespace PopForums.Repositories;

public interface ITransactionRepository
{
	Task Create(Transaction transaction);
	Task<List<Transaction>> GetByUserID(int userID);
	Task<Transaction> GetLastTransaction(int userID);
}
