using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using PopForums.Configuration;
using PopForums.Configuration.DependencyResolution;
using PopForums.Email;
using PopForums.ScoringGame;
using PopForums.Services;
using PopForums.Web;
using StructureMap;

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
					Container = ContainerFactory.Initialize();
					ServiceLocator = new StructureMapDependencyScope(Container);
					DependencyResolver = new StructureMapDependencyResolver(Container);
					StructuremapMvc.StructureMapDependencyScope = new StructureMapDependencyScope(Container);
					DynamicModuleUtility.RegisterModule(typeof(StructureMapScopeModule));
					DynamicModuleUtility.RegisterModule(typeof(PopForumsLoggingModule));
					_isInitialized = true;
				}
			}
		}

		public static void InitializeOutOfWeb()
		{
			lock (_syncRoot)
			{
				if (!_isInitialized)
				{
					Container = ContainerFactory.Initialize();
					Container.Configure(x => x.AddRegistry<PopForumsRegistry>());
					_isInitialized = true;
				}
			}
		}

		public static bool IsServiceRunningInstance;

		public static void StartServices()
		{
			Initialize();
			IsServiceRunningInstance = true;
			var setupService = ServiceLocator.GetInstance<ISetupService>();
			if (!setupService.IsDatabaseSetup())
				return;
			SetupServices();
		}

		public static void StartServicesOutOfWeb()
		{
			var setupService = Container.GetInstance<ISetupService>();
			if (!setupService.IsDatabaseSetup())
			{
				Trace.WriteLine("Database is not setup.");
				return;
			}
			SetupServices();
		}

		public static void StartServicesIfRunningInstance()
		{
			if (IsServiceRunningInstance)
				SetupServices();
		}

		private static void SetupServices()
		{
			_emailService.Start(Container);
			_userSessionService.Start(Container);
			_searchIndexService.Start(Container);
			_awardCalcService.Start(Container);
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
			var userAttribute = new PopForumsUserAttribute(ServiceLocator.GetInstance<IUserService>(), ServiceLocator.GetInstance<IUserSessionService>());
			GlobalFilters.Filters.Add(userAttribute);
		}

		public static void SetGlobalUserAttribute()
		{
			var userAttribute = new PopForumsGlobalUserAttribute(ServiceLocator.GetInstance<IUserService>(), ServiceLocator.GetInstance<IUserSessionService>());
			GlobalFilters.Filters.Add(userAttribute);
		}

		public static void SetAdditionalControllerNamespaces(string[] nameSpaces)
		{
			AdditionalControllerNamespaces = nameSpaces;
		}

		public static string[] AdditionalControllerNamespaces { get; private set; }
		public static IContainer Container { get; private set; }
		public static IServiceLocator ServiceLocator { get; private set; }
		public static System.Web.Http.Dependencies.IDependencyResolver DependencyResolver { get; private set; }

		private static bool _isInitialized;

		private static readonly object _syncRoot = new object();
	}
}