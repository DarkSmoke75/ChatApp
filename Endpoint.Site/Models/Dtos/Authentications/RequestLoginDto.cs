using System.ComponentModel.DataAnnotations;

namespace Endpoint.Site.Models.Dtos.Authentications
{
    public class RequestLoginDto
    {
        [Required]public string Email { get; set; }
        [Required] public string Password { get;set; }
    }
}
