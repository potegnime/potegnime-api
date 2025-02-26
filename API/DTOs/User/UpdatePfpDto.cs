using System.ComponentModel.DataAnnotations.Schema;

namespace API.DTOs.User
{
    public class UpdatePfpDto
    {
        [NotMapped]
        public IFormFile? ProfilePicFile { get; set; }
    }
}
