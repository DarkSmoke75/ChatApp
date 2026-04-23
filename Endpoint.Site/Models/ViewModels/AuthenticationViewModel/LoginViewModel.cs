using System.ComponentModel.DataAnnotations;

namespace Endpoint.Site.Models.ViewModels.AuthenticationViewModel
{
    public class LoginViewModel
    {
        [Required] public string Email { get; set; }
        [Required] public string Password { get; set; }
    }
}

