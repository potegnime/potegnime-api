using System.ComponentModel.DataAnnotations.Schema;

namespace PotegniMe.DTOs.User
{
    public class UpdatePfpDto
    {
        [NotMapped]
        public IFormFile? ProfilePicFile { get; set; }
    }
}
