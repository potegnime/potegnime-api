namespace API.DTOs.TorrentScrape
{
    public class ScrapredTorrentDto
    {
        public required string Source { get; set; }
        public required string Title { get; set; }
        public required string Time { get; set; }
        public required string Size { get; set; }
        public required string Url { get; set; }
        public required string Seeds { get; set; }
        public required string Peers { get; set; }
        public required string Imdb { get; set; }
    }
}
