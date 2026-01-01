using System.Text.Json;
using PotegniMe.DTOs.Recommend;
using PotegniMe.Helpers.Tmdb;

namespace PotegniMe.Services.ExploreService;
// Explore is done via TMDB API https://developer.themoviedb.org/reference/intro/getting-started

public class ExploreService : IExploreService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _tmdbUrlBase;
    private readonly string _tmdbApiKey;
    
    public ExploreService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _tmdbUrlBase = _configuration["Tmdb:Url"] ?? throw new Exception($"{Constants.Constants.AppSettingsErrorCode} Tmdb:Url");
        _tmdbApiKey = Environment.GetEnvironmentVariable("POTEGNIME_TMDB_KEY") ?? throw new Exception($"{Constants.Constants.DotEnvErrorCode} POTEGNIME_TMDB_KEY");
    }
    
    public async Task<Recommendation> RandomRecommendation()
    {
        const string language = "en-US";
        const bool includeAdult = false;
        const bool includeVideo = false;
        const string monetizationType = "flatrate";

        var random = new Random();
        int page = random.Next(1, 10);
        int indexOnPage = random.Next(0, 20);

        var url = $"{_tmdbUrlBase}discover/movie?api_key={_tmdbApiKey}&language={language}&sort_by=popularity.desc&include_adult={includeAdult}&include_video={includeVideo}&page={page}&with_watch_monetization_types={monetizationType}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var tmdbResponse = JsonSerializer.Deserialize<TmdbMovieApiResponse>(json) ?? throw new Exception($"{Constants.Constants.InternalErrorCode} TMDB API returned null");

        if (tmdbResponse.Results == null || !tmdbResponse.Results.Any())
            throw new Exception($"{Constants.Constants.InternalErrorCode} TMDB API returned empty results");

        // Ensure random index is within bounds
        indexOnPage = Math.Min(indexOnPage, tmdbResponse.Results.Count - 1);
        var movie = tmdbResponse.Results[indexOnPage];

        return new Recommendation
        {
            Date = DateOnly.FromDateTime(DateTime.Today),
            Name = movie.Title,
            Type = "movie"
        };
    }
    
    public Task<List<TmdbMovieResponse>> NowPlaying(string lang, int page, string region)
    {
        return GetMovieList("movie/now_playing", lang, page, region);
    }

    public Task<List<TmdbMovieResponse>> Popular(string lang, int page, string region)
    {
        return GetMovieList("movie/popular", lang, page, region);
    }

    public Task<List<TmdbMovieResponse>> TopRated(string lang, int page, string region)
    {
        return GetMovieList("movie/top_rated", lang, page, region);
    }

    public Task<List<TmdbMovieResponse>> Upcoming(string lang, int page, string region)
    {
        return GetMovieList("movie/upcoming", lang, page, region);
    }

    public async Task<List<TmdbTrendingResponse>> TrendingMovie(string lang)
    {
        return MapTrending(await HttpFetch<TmdbMovieApiResponse>($"trending/movie/{Constants.Constants.DefaultTimeWindow}", lang), lang, isTv: false);
    }

    public async Task<List<TmdbTrendingResponse>> TrendingTv(string lang)
    {
        return MapTrending(await HttpFetch<TmdbMovieApiResponse>($"trending/tv/{Constants.Constants.DefaultTimeWindow}", lang), lang, isTv: true);
    }

    // Helper methods
    private static string GetGenreName(int genreId, string lang)
    {
        if (lang == "en-US") return Constants.Constants.TmdbGenresEng.TryGetValue(genreId, out var g) ? g : "Unknown category";
        return Constants.Constants.TmdbGenresSl.TryGetValue(genreId, out var s) ? s : "Neznana kategorija";
    }
    
            private async Task<List<TmdbMovieResponse>> GetMovieList(string endpoint, string lang, int page, string region)
    {
        ValidateLanguage(lang);
        TmdbMovieApiResponse data = await HttpFetch<TmdbMovieApiResponse>(endpoint, lang, $"page={page}&region={region}");
        return data.Results.Select(m => new TmdbMovieResponse
        {
            Title = m.Title,
            Description = m.Overview,
            ReleaseDate = m.Release_Date,
            ImageUrl = m.Poster_Path,
            Genres = m.Genre_Ids.Select(id => GetGenreName(id, lang)).ToList()
        }).ToList();
    }

    private async Task<T> HttpFetch<T>(string endpoint, string lang, string? extra = null)
    {
        var url = $"{_tmdbUrlBase}{endpoint}?api_key={_tmdbApiKey}&language={lang}" + (extra is null ? "" : $"&{extra}");

        var res = await _httpClient.GetAsync(url);
        res.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<T>(
            await res.Content.ReadAsStringAsync()) ?? 
               throw new Exception($"{Constants.Constants.InternalErrorCode} TMDB null response"
            );
    }
    
    private static void ValidateLanguage(string lang)
    {
        if (lang != Constants.Constants.TmdbLanguageEnUs && lang != Constants.Constants.TmdbLanguageSlSi)
        {
            throw new ArgumentException("Unsupported language");
        }
    }
    
    private static List<TmdbTrendingResponse> MapTrending<T>(T response, string lang, bool isTv) where T : class
    {
        IEnumerable<dynamic> results = response switch
        {
            TmdbMovieApiResponse m => m.Results,
            TmdbTrendingApiResponse t => t.Results,
            _ => throw new ArgumentException("Invalid response")
        };

        return results.Select((x) => new TmdbTrendingResponse
        {
            Title = isTv ? x.Name : x.Title,
            Description = x.Overview,
            ImageUrl = x.Poster_Path,
            Genres = ((IEnumerable<int>)x.Genre_Ids).Select(id => GetGenreName(id, lang)).ToList()
        }).ToList();
    }
    
}