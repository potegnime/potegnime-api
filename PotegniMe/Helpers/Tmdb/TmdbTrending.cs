using System.Text.Json.Serialization;

namespace PotegniMe.Helpers.Tmdb;

public class TmdbTrending
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("overview")]
    public required string Overview { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("poster_path")]
    public required string PosterPath { get; set; }

    [JsonPropertyName("genre_ids")]
    public required List<int> GenreIds { get; set; }
}