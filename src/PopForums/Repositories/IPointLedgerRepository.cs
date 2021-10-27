namespace PopForums.Repositories;

public interface IPointLedgerRepository
{
	Task RecordEntry(PointLedgerEntry entry);
	Task<int> GetPointTotal(int userID);
	Task<int> GetEntryCount(int userID, string eventDefinitionID);
}