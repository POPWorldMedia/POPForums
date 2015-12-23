using PopForums.ScoringGame;

namespace PopForums.Repositories
{
	public interface IPointLedgerRepository
	{
		void RecordEntry(PointLedgerEntry entry);
		int GetPointTotal(int userID);
		int GetEntryCount(int userID, string eventDefinitionID);
	}
}
