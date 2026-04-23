namespace Endpoint.Site.Models.Dtos.Authentications
{
    public class RequestSignUpDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string RePassword { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
    }
}
