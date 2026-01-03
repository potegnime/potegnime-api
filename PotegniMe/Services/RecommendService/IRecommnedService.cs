namespace PotegniMe.Services.RecommendService;

public interface IRecommendService
{
    // Admin recommendations
    Task<Recommendation> SetRecommendation(Recommendation recommendation);
    Task<Recommendation> GetRecommendation(DateOnly date, string type);
    Task DeleteRecommendation(DateOnly date, string type);
}
