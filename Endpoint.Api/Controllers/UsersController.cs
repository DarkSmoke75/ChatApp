using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Application.Interfaces.FacadPatterns;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace Endpoint.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatabaseContext _context;
        private readonly IUserFacad _userFacad;
        public UsersController(IDatabaseContext context, IUserFacad userFacad)
        {
            _context = context;
            _userFacad = userFacad;
        }
        [HttpGet("GetUsers")]
        public IActionResult GetUsers()
        {
            var users = _userFacad.GetUsersService.Execute();
            if(!users.IsSuccess)
            {
                return BadRequest(users.Message);
            }
            return Ok(users);
        }
    }
}
