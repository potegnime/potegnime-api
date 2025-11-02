using System.Text.Json.Serialization;

namespace PotegniMe.DTOs.TorrentScrape
{
    public class ScrapedTorrentsResponseDto
    {
        [JsonPropertyName("thepiratebay")]
        public List<ScrapredTorrentDto> ThePirateBay { get; set; } = new List<ScrapredTorrentDto>();
        
        [JsonPropertyName("yts")]
        public List<ScrapredTorrentDto> Yts { get; set; } = new List<ScrapredTorrentDto>();

        [JsonPropertyName("torrentproject")]
        public List<ScrapredTorrentDto> TorrentProject { get; set; } = new List<ScrapredTorrentDto>();
    }
}