namespace API.DTOs.Search
{
    public class SearchRequestDto
    {
        public required string Query { get; set; }
        public string? Category { get; set; }
        public string? Source { get; set; }
        public int? Limit { get; set; }
    }
}
