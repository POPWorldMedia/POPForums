namespace PopForums.Services.Interfaces;

public interface ISitemapService
{
    Task<string> GenerateIndex(Func<int, string> pageLinkGenerator);
    Task<int> GetSitemapPageCount();
    Task<string> GeneratePage(Func<string, string> topicLinkGenerator, int page);
}
