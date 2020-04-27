using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface ISitemapService
	{
		Task<string> GenerateIndex(Func<int, string> pageLinkGenerator);
		Task<int> GetSitemapPageCount();
		Task<string> GeneratePage(Func<string, string> topicLinkGenerator, int page);
	}

	public class SitemapService : ISitemapService
	{
		private readonly ITopicRepository _topicRepository;
		private readonly IForumRepository _forumRepository;

		private const double _pageSize = 30000;

		public SitemapService(ITopicRepository topicRepository, IForumRepository forumRepository)
		{
			_topicRepository = topicRepository;
			_forumRepository = forumRepository;
		}

		public async Task<string> GenerateIndex(Func<int, string> pageLinkGenerator)
		{
			var pageCount = await GetSitemapPageCount();
			var s = new StringBuilder(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<sitemapindex xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
");
			for (int p = 0; p < pageCount; p++)
			{
				s.Append("\t<sitemap>\r\n\t\t<loc>");
				s.Append(pageLinkGenerator(p));
				s.Append("</loc>\r\n\t</sitemap>\r\n");
			}
			s.Append("</sitemapindex>");
			var result = s.ToString();
			return result;
		}

		public async Task<string> GeneratePage(Func<string, string> topicLinkGenerator, int page)
		{
			var nonViewableForumGraph = await _forumRepository.GetForumViewRestrictionRoleGraph();
			// any forum with a role attached isn't viewable, shouldn't appear in sitemap
			var nonViewableForumIDs = nonViewableForumGraph.Where(x => x.Value.Count > 0).Select(x => x.Key).ToList();
			var startRow = page == 0 ? 1 : (page * (int)_pageSize) + 1;
			var namesAndDates = await _topicRepository.GetUrlNames(false, nonViewableForumIDs, startRow, (int)_pageSize);
			var s = new StringBuilder(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
");
			foreach (var item in namesAndDates)
			{
				s.Append("\t<url>\r\n\t\t<loc>");
				s.Append(topicLinkGenerator(item.Item1));
				s.Append("</loc>\r\n\t\t<lastmod>");
				s.Append(item.Item2.ToString("yyyy-MM-ddThh:mmzzz"));
				s.Append("</lastmod>\r\n\t\t<changefreq>daily</changefreq>\r\n\t</url>\r\n");
			}
			s.Append("</urlset>");
			var result = s.ToString();
			return result;
		}

		public async Task<int> GetSitemapPageCount()
		{
			var nonViewableForumGraph = await _forumRepository.GetForumViewRestrictionRoleGraph();
			// any forum with a role attached isn't viewable, shouldn't appear in sitemap
			var nonViewableForumIDs = nonViewableForumGraph.Where(x => x.Value.Count > 0).Select(x => x.Key).ToList();
			var topicCount = await _topicRepository.GetTopicCount(false, nonViewableForumIDs);
			if (topicCount < _pageSize)
				return 1;
			var result = Math.Ceiling(topicCount / _pageSize);
			return Convert.ToInt32(result);
		}
	}
}