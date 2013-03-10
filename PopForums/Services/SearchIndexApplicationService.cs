using Ninject;
using PopForums.Configuration;

namespace PopForums.Services
{
	public class SearchIndexApplicationService : ApplicationServiceBase
	{
		public override void Start(IKernel kernel)
		{
			_searchService = kernel.Get<ISearchService>();
			_settingsManager = kernel.Get<ISettingsManager>();
			_postService = kernel.Get<IPostService>();
			base.Start(kernel);
		}

		private ISearchService _searchService;
		private ISettingsManager _settingsManager;
		private IPostService _postService;

		protected override void ServiceAction()
		{
			SearchIndexWorker.Instance.IndexNextTopic(_searchService, _settingsManager, _postService, ErrorLog);
		}

		protected override int GetInterval()
		{
			return _settingsManager.Current.SearchIndexingInterval;
		}
	}
}