using PopForums.Models;

namespace PopForums.Repositories
{
	public interface ISetupRepository
	{
		bool IsConnectionPossible();
		bool IsDatabaseSetup();
		void SetupDatabase();
	}
}
