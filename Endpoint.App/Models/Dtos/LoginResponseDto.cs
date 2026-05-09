namespace Endpoint.App.Models.Dtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string DisplayName { get; set; }
        public long UserId { get; set; }
    }

}
