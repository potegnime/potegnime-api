namespace PotegniMe.Services.RecommendService;

public interface IRecommendService
{
    // Set recommendation
    Task<Recommendation> SetRecommendation(Recommendation recommendation);

    // Get recommendation
    Task<Recommendation> GetRecommendation(DateOnly date, string type);

    // Delete recommendation
    Task DeleteRecommendation(DateOnly date, string type);
}
