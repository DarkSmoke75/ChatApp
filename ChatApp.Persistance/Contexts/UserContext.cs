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

        public long UserId
        {
            get
            {
                var user = _context.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true ||
                    !long.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ||
                    userId <= 0)
                {
                    throw new UnauthorizedAccessException("An authenticated user is required.");
                }

                return userId;
            }
        }
    }
}
