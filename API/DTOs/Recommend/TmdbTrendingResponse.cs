namespace API.DTOs.Recommend
{
    public class TmdbTrendingResponse
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string ImageUrl { get; set; }
        public required List<string> Genres { get; set; }
    }
}
