using System.Text.Json;
using System.Text.Json.Serialization;
using PotegniMe.DTOs.Recommend;

namespace PotegniMe.Services.RecommendService
{
    public class RecommendService : IRecommendService
    {
        // Fields
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly string _tmdbUrlBase;
        private readonly string _tmdbApiKey;

        // Constructor
        public RecommendService(IConfiguration configuration, DataContext context)
        {
            _configuration = configuration;
            _context = context;
            _tmdbUrlBase = _configuration["Tmdb:Url"] ?? throw new Exception($"{Constants.Constants.AppSettingsErrorCode} Tmdb:Url");
            _tmdbApiKey = Environment.GetEnvironmentVariable("POTEGNIME_TMDB_KEY") ?? throw new Exception($"{Constants.Constants.DotEnvErrorCode} POTEGNIME_TMDB_KEY");
        }

        // Methods
        public async Task<Recommendation> SetRecommendation(Recommendation recommendation)
        {
            recommendation.Type = recommendation.Type.ToLower();
            if (recommendation.Type != "movie" && recommendation.Type != "series") throw new ArgumentException("Tip mora imeti vrednost movie ali series");
            
            // Check if recommendation already exists for the given day
            var todayRecommendation = await _context.Recommendation.FirstOrDefaultAsync(
                r => r.Date.Equals(recommendation.Date) && r.Type.Equals(recommendation.Type)
            );
            if (todayRecommendation != null)
            {
                // Recommendation already exists, delete it from the database
                _context.Recommendation.Remove(todayRecommendation);
                await _context.SaveChangesAsync();
            }
            // Set new recommendation
            Recommendation newRecommendation = new Recommendation
            {
                Date = recommendation.Date,
                Type = recommendation.Type,
                Name = recommendation.Name
            };
            _context.Recommendation.Add(newRecommendation);
            await _context.SaveChangesAsync();
            return newRecommendation;
        }

        public async Task<Recommendation> GetRecommendation(DateOnly date, string type)
        {
            type = type.ToLower();
            if (type != "movie" && type != "series") throw new ArgumentException("Tip mora imeti vrednost movie ali series");
            
            return await _context.Recommendation.FirstOrDefaultAsync(r => r.Date == date && r.Type == type)
                   ?? throw new ArgumentException("Recommendation not found");
        }

        public async Task DeleteRecommendation(DateOnly date, string type)
        {
            type = type.ToLower();
            if (type != "movie" && type != "series") throw new ArgumentException("Tip mora imeti vrednost movie ali series");

            var recommendation = await _context.Recommendation.FirstOrDefaultAsync(r => r.Date == date && r.Type == type)
                                 ?? throw new ArgumentException("Recommendation not found");
            _context.Recommendation.Remove(recommendation);
            await _context.SaveChangesAsync();
        }

        public async Task<Recommendation> RandomRecommendation()
        {
            Random random = new Random();
            const string language = "en-US";
            const bool includeAdult = false;
            const bool includeVideo = false;
            const string watchMonetizationType = "flatrate";
            int randomPage = random.Next(1, 100);
            int randomMovieOnPage = random.Next(1, 20);

            string tmdbUrlBase = _tmdbUrlBase + "discover/movie";
            string tmdbUrlInit = tmdbUrlBase + $"?api_key={_tmdbApiKey}&language={language}&sort_by=popularity.desc&include_adult={includeAdult}&include_video={includeVideo}&page={randomPage}&with_watch_monetization_types={watchMonetizationType}";

            using HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrlInit);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonSerializer.Deserialize<TmdbMovieApiResponse>(jsonResponse);

            if (serializedJsonResponse?.Results == null) throw new Exception($"{Constants.Constants.InternalErrorCode} TMDB API response is null");

            var rnd = serializedJsonResponse.Results[randomMovieOnPage];

            return new Recommendation()
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                Name = rnd.Title,
                Type = "movie"
            };
        }

        public async Task<List<TmdbMovieResponse>> NowPlaying(string language, int page, string region)
        {
            if (language != "sl-SI" && language != "en-US") throw new ArgumentException("Unsupported language");
            
            // Get TMDB PotegniMe key from configuration
            string tmdbUrl = _tmdbUrlBase + "movie/now_playing";
            tmdbUrl += $"?api_key={_tmdbApiKey}&language={language}&page={page}&region={region}";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrl);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonSerializer.Deserialize<TmdbMovieApiResponse>(jsonResponse);

            if (serializedJsonResponse?.Results == null) return new List<TmdbMovieResponse>();

            var recommendResponseDtoList = serializedJsonResponse.Results.Select(movie => new TmdbMovieResponse
            {
                Title = movie.Title,
                Description = movie.Overview,
                ReleaseDate = movie.Release_Date,
                ImageUrl = movie.Poster_Path,
                Genres = movie.Genre_Ids.Select(id => GetGenreName(id, language)).ToList()
            }).ToList();

            return recommendResponseDtoList;
        }

        public async Task<List<TmdbMovieResponse>> Popular(string language, int page, string region)
        {
            if (language != "sl-SI" && language != "en-US") throw new ArgumentException("Unsupported language");
            
            // Get TMDB PotegniMe key from configuration
            string tmdbUrl = _tmdbUrlBase + "movie/popular";
            tmdbUrl += $"?api_key={_tmdbApiKey}&language={language}&page={page}&region={region}";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrl);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonSerializer.Deserialize<TmdbMovieApiResponse>(jsonResponse);

            if (serializedJsonResponse?.Results == null) return new List<TmdbMovieResponse>();

            var recommendResponseDtoList = serializedJsonResponse.Results.Select(movie => new TmdbMovieResponse
            {
                Title = movie.Title,
                Description = movie.Overview,
                ReleaseDate = movie.Release_Date,
                ImageUrl = movie.Poster_Path,
                Genres = movie.Genre_Ids.Select(id => GetGenreName(id, language)).ToList()
            }).ToList();

            return recommendResponseDtoList;
        }

        public async Task<List<TmdbMovieResponse>> TopRated(string language, int page, string region)
        {
            if (language != "sl-SI" && language != "en-US") throw new ArgumentException("Unsupported language");
            
            // Get TMDB PotegniMe key from configuration
            string tmdbUrl = _tmdbUrlBase + "movie/top_rated";
            tmdbUrl += $"?api_key={_tmdbApiKey}&language={language}&page={page}&region={region}";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrl);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonSerializer.Deserialize<TmdbMovieApiResponse>(jsonResponse);

            if (serializedJsonResponse?.Results == null) return new List<TmdbMovieResponse>();

            var recommendResponseDtoList = serializedJsonResponse.Results.Select(movie => new TmdbMovieResponse
            {
                Title = movie.Title,
                Description = movie.Overview,
                ReleaseDate = movie.Release_Date,
                ImageUrl = movie.Poster_Path,
                Genres = movie.Genre_Ids.Select(id => GetGenreName(id, language)).ToList()
            }).ToList();

            return recommendResponseDtoList;
        }

        public async Task<List<TmdbMovieResponse>> Upcoming(string language, int page, string region)
        {
            if (language != "sl-SI" && language != "en-US") throw new ArgumentException("Unsupported language");
            
            // Get TMDB PotegniMe key from configuration
            string tmdbUrl = this._tmdbUrlBase + "movie/upcoming";
            tmdbUrl += $"?api_key={this._tmdbApiKey}&language={language}&page={page}&region={region}";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrl);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonSerializer.Deserialize<TmdbMovieApiResponse>(jsonResponse);

            if (serializedJsonResponse?.Results == null) return new List<TmdbMovieResponse>();

            var recommendResponseDtoList = serializedJsonResponse.Results.Select(movie => new TmdbMovieResponse
            {
                Title = movie.Title,
                Description = movie.Overview,
                ReleaseDate = movie.Release_Date,
                ImageUrl = movie.Poster_Path,
                Genres = movie.Genre_Ids.Select(id => GetGenreName(id, language)).ToList()
            }).ToList();

            return recommendResponseDtoList;
        }

        public async Task<List<TmdbTrendingResponse>> TrendingMovie(string language)
        {
            if (language != "sl-SI" && language != "en-US") throw new ArgumentException("Unsupported language");
            
            string tmdbUrl = _tmdbUrlBase + $"trending/movie/{Constants.Constants.DefaultTimeWindow}";
            tmdbUrl += $"?api_key={_tmdbApiKey}&language={language}";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrl);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonSerializer.Deserialize<TmdbMovieApiResponse>(jsonResponse);

            if (serializedJsonResponse?.Results == null) return new List<TmdbTrendingResponse>();

            var recommendResponseDtoList = serializedJsonResponse.Results.Select(movie => new TmdbTrendingResponse
            {
                Title = movie.Title,
                Description = movie.Overview,
                ImageUrl = movie.Poster_Path,
                Genres = movie.Genre_Ids.Select(id => GetGenreName(id, language)).ToList()
            }).ToList();

            return recommendResponseDtoList;
        }

        public async Task<List<TmdbTrendingResponse>> TrendingTv(string language)
        {
            if (language != "sl-SI" && language != "en-US") throw new ArgumentException("Unsupported language");
            
            string tmdbUrl = _tmdbUrlBase + $"trending/tv/{Constants.Constants.DefaultTimeWindow}";
            tmdbUrl += $"?api_key={_tmdbApiKey}&language={language}";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrl);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonSerializer.Deserialize<TmdbTrendingApiResponse>(jsonResponse);

            if (serializedJsonResponse?.Results == null) return new List<TmdbTrendingResponse>();

            var recommendResponseDtoList = serializedJsonResponse.Results.Select(movie => new TmdbTrendingResponse
            {
                Title = movie.Name,
                Description = movie.Overview,
                ImageUrl = movie.Poster_Path,
                Genres = movie.Genre_Ids.Select(id => GetGenreName(id, language)).ToList()
            }).ToList();

            return recommendResponseDtoList;
        }

        // Helper methods
        private static string GetGenreName(int genreId, string language)
        {
            if (language == "en-US")
            {
                if (Constants.Constants.TmdbGenresEng.TryGetValue(genreId, out string? genreName))
                {
                    return genreName;
                }
                return "Unknown category";
            }
            if (language == "sl-SI")
            {
                if (Constants.Constants.TmdbGenresSl.TryGetValue(genreId, out string? genreName))
                {
                    return genreName;
                }
                return "Neznana kategorija";
            }
            throw new Exception($"{Constants.Constants.InternalErrorCode} Neveljaven izbor jezika");
        }
    }



    // TMDB movie formats for MOVIE LISTS
    public class TmdbMovieApiResponse
    {
        [JsonPropertyName("results")]
        public required List<TmdbMovie> Results { get; set; }
    }

    public class TmdbMovie
    {
        [JsonPropertyName("title")]
        public required string Title { get; set; }

        [JsonPropertyName("overview")]
        public required string Overview { get; set; }

        [JsonPropertyName("release_date")]
        public required string Release_Date { get; set; }

        [JsonPropertyName("poster_path")]
        public required string Poster_Path { get; set; }

        [JsonPropertyName("genre_ids")]
        public required List<int> Genre_Ids { get; set; }
    }

    // TMDB trending formats for TRENDING
    public class TmdbTrendingApiResponse
    {
        [JsonPropertyName("results")]
        public required List<TmdbTrending> Results { get; set; }
    }

    public class TmdbTrending
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("overview")]
        public required string Overview { get; set; }

        [JsonPropertyName("release_date")]
        public string Release_Date { get; set; }

        [JsonPropertyName("poster_path")]
        public required string Poster_Path { get; set; }

        [JsonPropertyName("genre_ids")]
        public required List<int> Genre_Ids { get; set; }
    }
}
