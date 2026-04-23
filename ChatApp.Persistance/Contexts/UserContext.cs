using ChatApp.Application.Interfaces.Contexts;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Persistance.Contexts
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _context;

        public UserContext(IHttpContextAccessor context)
        {
            _context = context;
        }

        public long UserId =>
            long.Parse(_context.HttpContext.User
                .FindFirst(ClaimTypes.NameIdentifier).Value);
    }
}
