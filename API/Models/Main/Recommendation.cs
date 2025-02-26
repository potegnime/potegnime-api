namespace API.Models.Main
{
    public class Recommendation
    {
        public required DateOnly Date { get; set; }
        public required string Type { get; set; }
        public required string Name { get; set; }
    }
}
