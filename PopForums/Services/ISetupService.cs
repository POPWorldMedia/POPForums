using System;
using PopForums.Models;

namespace PopForums.Services
{
	public interface ISetupService
	{
		bool IsConnectionPossible();
		bool IsDatabaseSetup();
		User SetupDatabase(SetupVariables setupVariables, out Exception exception);
	}
}