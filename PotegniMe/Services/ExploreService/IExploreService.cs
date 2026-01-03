using PotegniMe.DTOs.Recommend;

namespace PotegniMe.Services.ExploreService;

public interface IExploreService
{
    // Random title recommendation
    Task<Recommendation> RandomRecommendation();
    
    // TMDB movies
    Task<List<TmdbMovieResponse>> NowPlaying(string language, int page, string region);
    Task<List<TmdbMovieResponse>> Popular(string language, int page, string region);
    Task<List<TmdbMovieResponse>> TopRated(string language, int page, string region);
    Task<List<TmdbMovieResponse>> Upcoming(string language, int page, string region);

    // TMDB trending
    Task<List<TmdbTrendingResponse>> TrendingMovie(string language);
    Task<List<TmdbTrendingResponse>> TrendingTv(string language);
}