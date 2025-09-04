using API.DTOs.Search;
using API.DTOs.TorrentScrape;
using System.Text.Json;

namespace API.Services.SearchService
{
    public class SearchService : ISearchService
    {
        // Fields
        private readonly IConfiguration _configuration;

        // Constructor
        public SearchService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Methods
        public async Task<ScrapedTorrentsResponseDto> GetScrapedTorrentsAsync(SearchRequestDto request)
        {
            // Input validation
            // If category/source is null, set to all (wasn't specified with request)
            request.Category ??= "All";
            request.Source ??= "All";

            // Call potegnime-scrapper express.js API route /search
            string baseUrl = _configuration["potegnime-scraper:Url"] ?? throw new Exception("Cannot find potegnime-scraper API URL");
            string limit = _configuration["potegnime-scraper:BaseLimit"] ?? throw new Exception("Cannot find potegnime-scraper base limit");
            string url = $"{baseUrl}search?query={request.Query}&category={request.Category.ToLower()}&source={request.Source.ToLower()}&limit={limit}";

            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                //potegnime-scrapper API returns 500 general error - either down or scraping failed
                throw new TorrentScraperException();
            }
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            ScrapedTorrentsResponseDto serializedResponse = JsonSerializer.Deserialize<ScrapedTorrentsResponseDto>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) 
                ?? throw new NotFoundException();

            // Check for empty results
            if (serializedResponse == null ||
                serializedResponse.ThePirateBay.Count == 0 &&
                serializedResponse.Yts.Count == 0 &&
                serializedResponse.TorrentProject.Count == 0)
            {
                throw new NotFoundException();
            }

            return serializedResponse;
        }

        public async Task<IDictionary<string, List<string>>> GetCategoriesAsync(bool lowercase = false)
        {
            // Call potegnime-scrapper express.js API route /categories
            string baseUrl = _configuration["potegnime-scraper:Url"] ?? throw new Exception("Cannot find potegnime-scraper API URL");
            string url = $"{baseUrl}/categories";
            if (lowercase)
            {
                url += "?lowercase=true";
            }
            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                //potegnime-scrapper API returns 500 general error - either down or scraping failed
                throw new TorrentScraperException();
            }
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Dictionary<string, List<string>> serializedResponse = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) 
                ?? throw new TorrentScraperException();

            if (serializedResponse == null)
                throw new TorrentScraperException();

            return serializedResponse ?? throw new TorrentScraperException();
        }

        public async Task<IList<string>> GetProvidersAsync()
        {
            // Call potegnime-scrapper express.js API route /providers
            string baseUrl = _configuration["potegnime-scraper:Url"] ?? throw new Exception("Cannot find potegnime-scraper API URL");
            string url = $"{baseUrl}/providers";
            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                //potegnime-scrapper API returns 500 general error - either down or scraping failed
                throw new TorrentScraperException();
            }
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Dictionary<string, List<string>> serializedResponse = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
                ?? throw new TorrentScraperException();

            if (serializedResponse != null && serializedResponse.TryGetValue("providers", out var providers))
            {
                return providers;
            }

            throw new TorrentScraperException();
        }
    }
}
