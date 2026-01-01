using System.Text.Json.Serialization;

namespace PotegniMe.Helpers.Tmdb;

public class TmdbMovieApiResponse
{
    [JsonPropertyName("results")]
    public List<TmdbMovie>? Results { get; set; }
}