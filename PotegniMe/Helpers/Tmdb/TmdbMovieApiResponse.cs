using System.Text.Json.Serialization;

namespace PotegniMe.Helpers.Tmdb;

public class TmdbMovieApiResponse
{
    [JsonPropertyName("results")]
    public required List<TmdbMovie> Results { get; set; }
}