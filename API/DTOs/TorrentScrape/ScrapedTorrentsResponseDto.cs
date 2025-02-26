using Newtonsoft.Json;

namespace API.DTOs.TorrentScrape
{
    public class ScrapedTorrentsResponseDto
    {
        [JsonProperty("thepiratebay")]
        public List<ScrapredTorrentDto> ThePirateBay { get; set; } = new List<ScrapredTorrentDto>();
        
        [JsonProperty("yts")]
        public List<ScrapredTorrentDto> Yts { get; set; } = new List<ScrapredTorrentDto>();

        [JsonProperty("torrentproject")]
        public List<ScrapredTorrentDto> TorrentProject { get; set; } = new List<ScrapredTorrentDto>();
    }
}