using PotegniMe.Core.Exceptions;

namespace PotegniMe.Services.RecommendService;

public class RecommendService(DataContext context) : IRecommendService
{
    public async Task<Recommendation> SetRecommendation(Recommendation recommendation)
    {
        recommendation.Type = recommendation.Type.ToLower();
        if (recommendation.Type != "movie" && recommendation.Type != "series") throw new ArgumentException("Tip mora imeti vrednost movie ali series");
        
        // Check if recommendation already exists for the given day
        var todayRecommendation = await context.Recommendation.FirstOrDefaultAsync(
            r => r.Date.Equals(recommendation.Date) && r.Type.Equals(recommendation.Type)
        );
        if (todayRecommendation != null)
        {
            // Recommendation already exists, delete it from the database
            context.Recommendation.Remove(todayRecommendation);
            await context.SaveChangesAsync();
        }
        // Set new recommendation
        Recommendation newRecommendation = new Recommendation
        {
            Date = recommendation.Date,
            Type = recommendation.Type,
            Name = recommendation.Name
        };
        context.Recommendation.Add(newRecommendation);
        await context.SaveChangesAsync();
        return newRecommendation;
    }

    public async Task<Recommendation> GetRecommendation(DateOnly date, string type)
    {
        type = type.ToLower();
        if (type != "movie" && type != "series") throw new ArgumentException("Tip mora imeti vrednost movie ali series");
        
        return await context.Recommendation.FirstOrDefaultAsync(r => r.Date == date && r.Type == type)
               ?? throw new NotFoundException();
    }

    public async Task DeleteRecommendation(DateOnly date, string type)
    {
        type = type.ToLower();
        if (type != "movie" && type != "series") throw new ArgumentException("Tip mora imeti vrednost movie ali series");

        var recommendation = await context.Recommendation.FirstOrDefaultAsync(r => r.Date == date && r.Type == type)
                             ?? throw new NotFoundException();
        context.Recommendation.Remove(recommendation);
        await context.SaveChangesAsync();
    }
}
