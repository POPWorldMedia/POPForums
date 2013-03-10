using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Services
{
	public interface IIPHistoryService
	{
		List<IPHistoryEvent> GetHistory(string ip, DateTime start, DateTime end);
	}
}