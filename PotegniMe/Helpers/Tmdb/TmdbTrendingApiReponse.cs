using System.Text.Json.Serialization;

namespace PotegniMe.Helpers.Tmdb;

public class TmdbTrendingApiResponse
{
    [JsonPropertyName("results")]
    public required List<TmdbTrending> Results { get; set; }
}