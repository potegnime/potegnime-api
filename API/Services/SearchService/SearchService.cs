using API.DTOs.Search;
using API.DTOs.TorrentScrape;
using Newtonsoft.Json;
using System.Diagnostics;

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
        public async Task<dynamic> GetScrapedTorrentsAsync(SearchRequestDto request)
        {
            // Input validation
            if (request.Query == null)
            {
                throw new Exception("Search query parameter is required!");
            }

            // Get scraped torrents from Node.js project
            if (_configuration["NodeScripts:NodePath"] == null || _configuration["NodeScripts:ScriptsPath"] == null)
            {
                throw new Exception("Cannot find internal script paths!");
            }

            string? constLimit = _configuration["InternalApiSettings:BaseScrapeSearchLimit"];
            string? nodePath = _configuration["NodeScripts:NodePath"];
            string? scriptPath = _configuration["NodeScripts:ScriptsPath"];
            string args = $"\"{request.Query}\" \"{request.Category}\" \"{request.Source}\" {constLimit}";
            if (nodePath == null || scriptPath == null)
            {
                throw new Exception("Cannot find internal script paths");
            }

            // Start a new process for running Node.js
            ProcessStartInfo startInfo = new ProcessStartInfo(nodePath, scriptPath + " " + args);
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            // Create new process
            Process process = new Process();
            process.StartInfo = startInfo;
            try
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                await process.WaitForExitAsync();
                // Check if scraping was successful
                if (output != null && output.StartsWith("ERR-"))
                {
                    throw new Exception("Error scraping torrents!");
                }
                // First checking for empty results
                if (output == null || output == "[]" || output == "")
                {
                    throw new NotFoundException();
                }
                // Deserialize output
                ScrapedTorrentsResponseDto jsonResponse = JsonConvert.DeserializeObject<ScrapedTorrentsResponseDto>(output) 
                    ?? throw new Exception("Internal exception");
                // Second checking for empty results
                try
                {
                    bool allEmpty = jsonResponse.ThePirateBay.Count == 0 &&
                    jsonResponse.Yts.Count == 0 &&
                    jsonResponse.TorrentProject.Count == 0;
                    if (allEmpty)
                    {
                        throw new NotFoundException();
                    }
                }
                catch
                {
                    throw new NotFoundException();
                }
                return jsonResponse ?? throw new Exception("Error scraping torrents");
            }
            finally
            {
                process.Close();
            }
        }
    
        public IDictionary<string, List<string>> GetAllProviderCategories()
        {
            // Data completely depends on the command bellow, executed in the app.js
            // console.log(TorrentSearchApi.getProviders());

            IDictionary<string, List<string>> categorySource = new Dictionary<string, List<string>>
            {
                { "All", new List<string> { "All" } },
                { "ThePirateBay", new List<string> { "All", "Audio", "Video", "Applications", "Games", "Porn", "Other", "Top100" } },
                { "Yts", new List<string> { "All", "Movies" } },
                { "TorrentProject", new List<string> { "All" } }
            };
            return categorySource;
        }

        public List<string> GetProviderCategories(string provider)
        {
            // Don't modify method! Only modify the GetAllProviderCategories()

            IDictionary<string, List<string>> allProviderCategories = this.GetAllProviderCategories();
            if (allProviderCategories.ContainsKey(provider))
            {
                return allProviderCategories[provider];
            } else
            {
                throw new ArgumentException($"Provider {provider} not found!");
            }
        }

        public List<string> GetAllSupportedProviders()
        {
            // Don't modify method! Only modify the GetAllProviderCategories()

            IDictionary<string, List<string>> allProviderCategories = this.GetAllProviderCategories();
            return allProviderCategories.Keys.ToList();
        }
    }
}
