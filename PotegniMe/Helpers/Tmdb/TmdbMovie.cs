using System.Text.Json.Serialization;

namespace PotegniMe.Helpers.Tmdb;

public class TmdbMovie
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("overview")]
    public required string Overview { get; set; }

    [JsonPropertyName("release_date")]
    public required string Release_Date { get; set; }

    [JsonPropertyName("poster_path")]
    public required string Poster_Path { get; set; }

    [JsonPropertyName("genre_ids")]
    public required List<int> Genre_Ids { get; set; }
}