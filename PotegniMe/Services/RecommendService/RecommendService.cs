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
            _tmdbUrlBase = _configuration["Tmdb:Url"] ??
                           throw new Exception("Cannot find TMDB url");
            _tmdbApiKey = Environment.GetEnvironmentVariable("POTEGNIME_TMDB_KEY") ??
                          throw new Exception("Cannot find TMDB API KEY");
        }

        // Methods
        public async Task<Recommendation> SetRecommendation(Recommendation recommendation)
        {
            // Check if recommendation already exists for the given day
            var todaysRecommendation = await _context.Recommendation.FirstOrDefaultAsync(
                r => r.Date.Equals(recommendation.Date) && r.Type.Equals(recommendation.Type)
            );
            if (todaysRecommendation != null)
            {
                // Recommendation already exists, delete it from the database
                _context.Recommendation.Remove(todaysRecommendation);
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

        public async Task<Recommendation> GetRecommendation(DateOnly datetime, string type)
        {
            Recommendation recommendation = await _context.Recommendation.FirstOrDefaultAsync(
                r => r.Date.Equals(datetime) && r.Type.Equals(type))
             ?? throw new ArgumentException();
            return recommendation;
        }

        public async Task DeleteRecommendation(DateOnly datetime, string type)
        {
            var recommendation = await _context.Recommendation.FirstOrDefaultAsync(
                r => r.Date.Equals(datetime) && r.Type.Equals(type)
            );
            if (recommendation == null)
            {
                throw new ArgumentException();
            }
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

            string tmdbUrlBase = this._tmdbUrlBase + "discover/movie";
            string tmdbUrlInit = tmdbUrlBase += $"?api_key={this._tmdbApiKey}&language={language}&sort_by=popularity.desc&include_adult={includeAdult}&include_video={includeVideo}&page={randomPage}&with_watch_monetization_types={watchMonetizationType}";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrlInit);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonSerializer.Deserialize<TmdbMovieApiResponse>(jsonResponse);

            if (serializedJsonResponse?.Results == null) throw new Exception();

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
            // Get TMDB PotegniMe key from configugration
            string tmdbUrl = this._tmdbUrlBase + "movie/now_playing";
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

        public async Task<List<TmdbMovieResponse>> Popular(string language, int page, string region)
        {
            // Get TMDB PotegniMe key from configugration
            string tmdbUrl = this._tmdbUrlBase + "movie/popular";
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

        public async Task<List<TmdbMovieResponse>> TopRated(string language, int page, string region)
        {
            // Get TMDB PotegniMe key from configugration
            string tmdbUrl = this._tmdbUrlBase + "movie/top_rated";
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

        public async Task<List<TmdbMovieResponse>> Upcoming(string language, int page, string region)
        {
            // Get TMDB PotegniMe key from configugration
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
            string tmdbUrl = this._tmdbUrlBase + $"trending/movie/{Constants.Constants.DEFAULT_TIME_WINDOW}";
            tmdbUrl += $"?api_key={this._tmdbApiKey}&language={language}";

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
            string tmdbUrl = this._tmdbUrlBase + $"trending/tv/{Constants.Constants.DEFAULT_TIME_WINDOW}";
            tmdbUrl += $"?api_key={this._tmdbApiKey}&language={language}";

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
                if (Constants.Constants.TMDB_GENRES_ENG.TryGetValue(genreId, out string? genreName))
                {
                    return genreName;
                }
                return "Unknown category";
            }
            else if (language == "sl-SI")
            {
                if (Constants.Constants.TMDB_GENRES_SL.TryGetValue(genreId, out string? genreName))
                {
                    return genreName;
                }
                return "Neznana kategorija";
            }
            throw new Exception("Neveljaven izbor jezika");
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
