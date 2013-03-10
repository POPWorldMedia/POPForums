using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PopForums.Models;

namespace PopForums.Services
{
	public class IPHistoryService : IIPHistoryService
	{
		public IPHistoryService(IPostService postService, ISecurityLogService securityLogService)
		{
			_postService = postService;
			_securityLogService = securityLogService;
		}

		private readonly IPostService _postService;
		private readonly ISecurityLogService _securityLogService;

		public List<IPHistoryEvent> GetHistory(string ip, DateTime start, DateTime end)
		{
			var list = new List<IPHistoryEvent>();
			list.AddRange(_postService.GetIPHistory(ip, start, end));
			list.AddRange(_securityLogService.GetIPHistory(ip, start, end));
			return list.OrderBy(i => i.EventTime).ToList();
		}
	}
}
