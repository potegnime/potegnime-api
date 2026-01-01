using System.Text.Json.Serialization;

namespace PotegniMe.Helpers.Tmdb;

public class TmdbMovie
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("overview")]
    public required string Overview { get; set; }

    [JsonPropertyName("release_date")]
    public required string ReleaseDate { get; set; }

    [JsonPropertyName("poster_path")]
    public required string PosterPath { get; set; }

    [JsonPropertyName("genre_ids")]
    public required List<int> GenreIds { get; set; }
}