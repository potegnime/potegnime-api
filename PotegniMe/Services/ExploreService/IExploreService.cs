using PotegniMe.DTOs.Recommend;

namespace PotegniMe.Services.ExploreService;

public interface IExploreService
{
    // Random title recommendation
    Task<Recommendation> RandomRecommendation();
    
    // Get now playing movies
    Task<List<TmdbMovieResponse>> NowPlaying(string language, int page, string region);

    // Get popular movies
    Task<List<TmdbMovieResponse>> Popular(string language, int page, string region);

    // Get top-rated movies
    Task<List<TmdbMovieResponse>> TopRated(string language, int page, string region);

    // Get upcoming movies
    Task<List<TmdbMovieResponse>> Upcoming(string language, int page, string region);

    // Get trending movies
    Task<List<TmdbTrendingResponse>> TrendingMovie(string language);

    // Get trending TV
    Task<List<TmdbTrendingResponse>> TrendingTv(string language);
}