using API.DTOs.Search;

namespace API.Services.SearchService
{
    public interface ISearchService
    {
        // Get scraped torrents
        Task<dynamic> GetScrapedTorrentsAsync(SearchRequestDto request);

        // Get all provider categories
        IDictionary<string, List<string>> GetAllProviderCategories();

        // Get categories by provider
        List<string> GetProviderCategories(string provider);

        // Get supported providers
        List<string> GetAllSupportedProviders();
    }
}
