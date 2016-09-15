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
			_emailService.Start(serviceProvider);
			_userSessionService.Start(serviceProvider);
			_searchIndexService.Start(serviceProvider);
			_awardCalcService.Start(serviceProvider);
			ApplicationServices.Add(_emailService);
			ApplicationServices.Add(_userSessionService);
			ApplicationServices.Add(_searchIndexService);
			ApplicationServices.Add(_awardCalcService);
		}

		private static readonly EmailApplicationService _emailService = new EmailApplicationService();
		private static readonly UserSessionApplicationService _userSessionService = new UserSessionApplicationService();
		private static readonly SearchIndexApplicationService _searchIndexService = new SearchIndexApplicationService();
		private static readonly AwardCalculatorApplicationService _awardCalcService = new AwardCalculatorApplicationService();

		public static readonly List<ApplicationServiceBase> ApplicationServices = new List<ApplicationServiceBase>();
	}
}
