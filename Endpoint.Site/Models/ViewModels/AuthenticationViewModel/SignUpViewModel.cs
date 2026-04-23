using System.ComponentModel.DataAnnotations;

namespace Endpoint.Site.Models.ViewModels.AuthenticationViewModel
{
    public class SignUpViewModel
    {
        [Required]public string Email { get; set; }
        [Required]public string Password { get; set; }
        [Required]public string RePassword { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
    }
}
