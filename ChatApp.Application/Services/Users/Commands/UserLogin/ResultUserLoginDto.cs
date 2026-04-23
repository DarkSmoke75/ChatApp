namespace ChatApp.Application.Services.Users.Commands.UserLogin
{
    public class ResultUserLoginDto
    {
        public long UserId { get; set; }
        public List<string> Roles { get; set; }
        public string Username { get; set; } = "";
        public string DisplayName { get; set; } = "";

    }
}
