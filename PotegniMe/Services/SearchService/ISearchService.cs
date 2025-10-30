using PotegniMe.DTOs.Search;
using PotegniMe.DTOs.TorrentScrape;

namespace PotegniMe.Services.SearchService
{
    public interface ISearchService
    {
        // Get scraped torrents
        Task<ScrapedTorrentsResponseDto> GetScrapedTorrentsAsync(SearchRequestDto request);

        // Get all supported providers and their categories
        Task<IDictionary<string, List<string>>> GetCategoriesAsync(bool lowercase = false);

        // Get all supported providers
        // Somewhat redundant, can just get keys from GetCategoriesAsync()
        Task<IList<string>> GetProvidersAsync();
    }
}
