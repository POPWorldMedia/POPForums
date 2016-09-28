using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Email;
using PopForums.ScoringGame;

namespace PopForums.Services
{
    public class BackgroundServices
	{
		public static void SetupServices(IServiceProvider serviceProvider)
		{
			var setupService = serviceProvider.GetService<ISetupService>();
			if (!setupService.IsConnectionPossible() || !setupService.IsDatabaseSetup())
				return;
			_emailService = new EmailApplicationService();
			_userSessionService = new UserSessionApplicationService();
			_searchIndexService = new SearchIndexApplicationService();
			_awardCalcService = new AwardCalculatorApplicationService();
			_emailService.Start(serviceProvider);
			_userSessionService.Start(serviceProvider);
			_searchIndexService.Start(serviceProvider);
			_awardCalcService.Start(serviceProvider);
			ApplicationServices.Add(_emailService);
			ApplicationServices.Add(_userSessionService);
			ApplicationServices.Add(_searchIndexService);
			ApplicationServices.Add(_awardCalcService);
		}

		private static EmailApplicationService _emailService;
		private static UserSessionApplicationService _userSessionService;
		private static SearchIndexApplicationService _searchIndexService;
		private static AwardCalculatorApplicationService _awardCalcService;

		public static readonly List<ApplicationServiceBase> ApplicationServices = new List<ApplicationServiceBase>();
	}
}
