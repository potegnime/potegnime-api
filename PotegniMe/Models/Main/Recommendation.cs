using System.ComponentModel.DataAnnotations;

namespace PotegniMe.Models.Main
{
    public class Recommendation
    {
        public DateOnly Date { get; set; }

        public string Type { get; set; } = null!;

        public string Name { get; set; } = null!;
    }
}
