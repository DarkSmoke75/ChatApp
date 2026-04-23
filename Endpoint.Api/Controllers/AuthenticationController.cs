using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Application.Interfaces.FacadPatterns;
using ChatApp.Application.Services.JWT;
using ChatApp.Application.Services.Users.Commands.RegisterUser;
using ChatApp.Application.Services.Users.Commands.UserLogin;
using ChatApp.Application.Services.Users.FacadPattern;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Endpoint.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IDatabaseContext _context;
        private readonly IJwtService _jwtService;
        private readonly IUserFacad _userFacad;

        public AuthenticationController(IDatabaseContext context, IJwtService jwtService,IUserFacad userFacad)
        {
            _context = context;
            _jwtService = jwtService;
            _userFacad = userFacad;
        }

        [HttpPost("Login")]
        public IActionResult Login(RequestUserLoginDto request)
        {
            var userData = _userFacad.UserLoginService.Execute(request);

            if (!userData.IsSuccess || userData.Data == null)
            {
                return Unauthorized("Invalid credentials");
            }
            var user = _context.Users.FirstOrDefault(u=>u.Id==userData.Data.UserId);
            

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new { token });
        }

        [HttpPost("SignUp")]
        public IActionResult SignUp(RequestRegisterUserDto request)
        {
            var signUpResult = _userFacad.RegisterUserService.Execute(request);
            if (!signUpResult.IsSuccess)
            {
                return BadRequest(signUpResult.Message);
            }
            var user = _context.Users.FirstOrDefault(u => u.Id ==signUpResult.Data.UserId);
            var token = _jwtService.GenerateToken(user);
            return Ok(new { token });
        }

    }
}
