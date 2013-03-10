using System.Collections.Generic;

namespace PopForums.Configuration
{
	public interface ISettingsManager
	{
		Settings Current { get; }
		void SaveCurrent();
		void FlushCurrent();
		void SaveCurrent(Dictionary<string, object> dictionary);
		bool IsLoaded();
	}
}