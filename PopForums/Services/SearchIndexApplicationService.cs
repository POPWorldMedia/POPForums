// TODO: SearchIndex service

//using PopForums.Configuration;
//using StructureMap;

//namespace PopForums.Services
//{
//	public class SearchIndexApplicationService : ApplicationServiceBase
//	{
//		public override void Start(IContainer container)
//		{
//			_searchService = container.GetInstance<ISearchService>();
//			_settingsManager = container.GetInstance<ISettingsManager>();
//			_postService = container.GetInstance<IPostService>();
//			base.Start(container);
//		}

//		private ISearchService _searchService;
//		private ISettingsManager _settingsManager;
//		private IPostService _postService;

//		protected override void ServiceAction()
//		{
//			SearchIndexWorker.Instance.IndexNextTopic(_searchService, _settingsManager, _postService, ErrorLog);
//		}

//		protected override int GetInterval()
//		{
//			return _settingsManager.Current.SearchIndexingInterval;
//		}
//	}
//}