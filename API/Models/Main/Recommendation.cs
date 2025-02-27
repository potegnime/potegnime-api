using System.ComponentModel.DataAnnotations;

namespace API.Models.Main
{
    public class Recommendation
    {
        [Key]
        public required DateOnly Date { get; set; }
        
        [Required]
        public required string Type { get; set; }

        [Required]
        public required string Name { get; set; }
    }
}
