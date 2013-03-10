using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.ScoringGame;
using PopForums.Services;
using PopForums.Web;

[assembly: PreApplicationStartMethod(typeof(PopForumsActivation), "Initialize")]

namespace PopForums.Web
{
	public static class PopForumsActivation
	{
		public static void Initialize()
		{
			lock (_syncRoot)
			{
				if (!_isInitialized)
				{
					Kernel = new StandardKernel(new CoreInjectionModule());
					DynamicModuleUtility.RegisterModule(typeof (PopForumsLoggingModule));
					RouteTable.Routes.MapHubs(); 
					_isInitialized = true;
				}
			}
		}

		public static bool IsServiceRunningInstance;

		public static void StartServices()
		{
			Initialize();
			IsServiceRunningInstance = true;
			var setupService = Kernel.Get<ISetupService>();
			if (!setupService.IsDatabaseSetup())
				return;
			SetupServices();
		}

		public static void StartServicesIfRunningInstance()
		{
			if (IsServiceRunningInstance)
				SetupServices();
		}

		private static void SetupServices()
		{
			_emailService.Start(Kernel);
			_userSessionService.Start(Kernel);
			_searchIndexService.Start(Kernel);
			_awardCalcService.Start(Kernel);
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

		public static void SetUserAttribute()
		{
			var userAttribute = new PopForumsUserAttribute(Kernel.Get<IUserService>(), Kernel.Get<IUserSessionService>());
			GlobalFilters.Filters.Add(userAttribute);
		}

		public static void SetGlobalUserAttribute()
		{
			var userAttribute = new PopForumsGlobalUserAttribute(Kernel.Get<IUserService>(), Kernel.Get<IUserSessionService>());
			GlobalFilters.Filters.Add(userAttribute);
		}

		public static void SetAdditionalControllerNamespaces(string[] nameSpaces)
		{
			AdditionalControllerNamespaces = nameSpaces;
		}

		public static string[] AdditionalControllerNamespaces { get; private set; }

		public static IKernel Kernel { get; private set; }

		private static bool _isInitialized;

		private static readonly object _syncRoot = new object();
	}
}