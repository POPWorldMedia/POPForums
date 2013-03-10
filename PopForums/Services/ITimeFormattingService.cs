using System;
using PopForums.Models;

namespace PopForums.Services
{
	public interface ITimeFormattingService
	{
		void Init(Profile profile);
		string GetFormattedTime(DateTime utcDateTime);
		int UtcOffset { get; }
		bool UsesDaylightSaving { get; }
	}
}