using API.DTOs.Recommend;
using Newtonsoft.Json;

namespace API.Services.RecommendService  
{
    public class RecommendService : IRecommnedService
    {
        // Fields
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;

        // Constructor
        public RecommendService(IConfiguration configuration, DataContext context)
        {
            _configuration = configuration;
            _context = context;
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

            // Get TMDB API key from configugration
            string? tmdbKey = _configuration["ApiKeys:Tmdb"] ?? throw new Exception("Cannot find internal API keys");
            string tmdbUrlBase = "https://api.themoviedb.org/3/discover/movie";
            string tmdbUrlInit = tmdbUrlBase += $"?api_key={tmdbKey}&language={language}&sort_by=popularity.desc&include_adult={includeAdult}&include_video={includeVideo}&page={randomPage}&with_watch_monetization_types={watchMonetizationType}";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrlInit);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonConvert.DeserializeObject<TmdbMovieApiResponse>(jsonResponse);

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
            // Get TMDB API key from configugration
            string? tmdbKey = _configuration["ApiKeys:Tmdb"] ?? throw new Exception("Cannot find internal API keys");
            string tmdbUrl = "https://api.themoviedb.org/3/movie/now_playing";
            tmdbUrl += $"?api_key={tmdbKey}&language={language}&page={page}&region={region}";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrl);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonConvert.DeserializeObject<TmdbMovieApiResponse>(jsonResponse);

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
            // Get TMDB API key from configugration
            string? tmdbKey = _configuration["ApiKeys:Tmdb"] ?? throw new Exception("Cannot find internal API keys");
            string tmdbUrl = "https://api.themoviedb.org/3/movie/popular";
            tmdbUrl += $"?api_key={tmdbKey}&language={language}&page={page}&region={region}";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrl);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonConvert.DeserializeObject<TmdbMovieApiResponse>(jsonResponse);

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
            // Get TMDB API key from configugration
            string? tmdbKey = _configuration["ApiKeys:Tmdb"] ?? throw new Exception("Cannot find internal API keys");
            string tmdbUrl = "https://api.themoviedb.org/3/movie/top_rated";
            tmdbUrl += $"?api_key={tmdbKey}&language={language}&page={page}&region={region}";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrl);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonConvert.DeserializeObject<TmdbMovieApiResponse>(jsonResponse);

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
            // Get TMDB API key from configugration
            string? tmdbKey = _configuration["ApiKeys:Tmdb"] ?? throw new Exception("Cannot find internal API keys");
            string tmdbUrl = "https://api.themoviedb.org/3/movie/upcoming";
            tmdbUrl += $"?api_key={tmdbKey}&language={language}&page={page}&region={region}";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrl);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonConvert.DeserializeObject<TmdbMovieApiResponse>(jsonResponse);

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

        public async Task<List<TmdbTrendingResponse>> TrendingMovie(string timeWindow, string language)
        {
            // Get TMDB API key from configugration
            string? tmdbKey = _configuration["ApiKeys:Tmdb"] ?? throw new Exception("Cannot find internal API keys");
            string tmdbUrl = $"https://api.themoviedb.org/3/trending/movie/{timeWindow}";
            tmdbUrl += $"?api_key={tmdbKey}&language={language}";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrl);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonConvert.DeserializeObject<TmdbMovieApiResponse>(jsonResponse);

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

        public async Task<List<TmdbTrendingResponse>> TrendingTv(string timeWindow, string language)
        {
            // Get TMDB API key from configugration
            string? tmdbKey = _configuration["ApiKeys:Tmdb"] ?? throw new Exception("Cannot find internal API keys");
            string tmdbUrl = $"https://api.themoviedb.org/3/trending/tv/{timeWindow}";
            tmdbUrl += $"?api_key={tmdbKey}&language={language}";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(tmdbUrl);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var serializedJsonResponse = JsonConvert.DeserializeObject<TmdbTrendingApiResponse>(jsonResponse);

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
            // Updated data to include both movie and TV genres
            IDictionary<int, string> genreDictEng = new Dictionary<int, string>
            {
                { 28, "Action" },
                { 12, "Adventure" },
                { 16, "Animation" },
                { 35, "Comedy" },
                { 80, "Crime" },
                { 99, "Documentary" },
                { 18, "Drama" },
                { 10751, "Family" },
                { 14, "Fantasy" },
                { 36, "History" },
                { 27, "Horror" },
                { 10402, "Music" },
                { 9648, "Mystery" },
                { 10749, "Romance" },
                { 878, "Science Fiction" },
                { 10770, "TV Movie" },
                { 53, "Thriller" },
                { 10752, "War" },
                { 37, "Western" },
                { 10759, "Action & Adventure" },
                { 10762, "Kids" },
                { 10763, "News" },
                { 10764, "Reality" },
                { 10765, "Sci-Fi & Fantasy" },
                { 10766, "Soap" },
                { 10767, "Talk" },
                { 10768, "War & Politics" }
            };

            IDictionary<int, string> genreDictSlo = new Dictionary<int, string>
            {
                { 28, "Akcija" },
                { 12, "Pustolovščina" },
                { 16, "Animacija" },
                { 35, "Komedija" },
                { 80, "Kriminalka" },
                { 99, "Dokumentarni" },
                { 18, "Drama" },
                { 10751, "Družinski" },
                { 14, "Fantazija" },
                { 36, "Zgodovinski" },
                { 27, "Grozljivka" },
                { 10402, "Glasbeni" },
                { 9648, "Misterij" },
                { 10749, "Romantični" },
                { 878, "Znanstvena fantastika" },
                { 10770, "TV film" },
                { 53, "Triler" },
                { 10752, "Vojni" },
                { 37, "Vestern" },
                { 10759, "Akcija & Pustolovščina" },
                { 10762, "Otroški" },
                { 10763, "Novice" },
                { 10764, "Resničnostni" },
                { 10765, "Znanstvena fantastika & Fantazija" },
                { 10766, "Telenovela" },
                { 10767, "Pogovorna oddaja" },
                { 10768, "Vojna & Politika" }
            };
            if (language == "en-US")
            {
                if (genreDictEng.TryGetValue(genreId, out string genreName))
                {
                    return genreName;
                }
                return "Unknown category";
            } 
            else if (language == "sl-SI")
            {
                if (genreDictSlo.TryGetValue(genreId, out string genreName))
                {
                    return genreName;
                }
                return "Neznana kategorija";
            } 
            else
            {
                throw new Exception("Neveljaven izbor jezika");
            }
        }

    }



    // TMDB movie formats for MOVIE LISTS
    public class TmdbMovieApiResponse
    {
        public required List<TmdbMovie> Results { get; set; }
    }

    public class TmdbMovie
    {
        public required string Title { get; set; }
        public required string Overview { get; set; }
        public required string Release_Date { get; set; }
        public required string Poster_Path { get; set; }
        public required List<int> Genre_Ids { get; set; }
    }

    // TMDB trending formats for TRENDING
    public class TmdbTrendingApiResponse
    {
        public required List<TmdbTrending> Results { get; set; }
    }

    public class TmdbTrending
    {
        public required string Name { get; set; }
        public required string Overview { get; set; }
        public required string Release_Date { get; set; }
        public required string Poster_Path { get; set; }
        public required List<int> Genre_Ids { get; set; }
    }
}
