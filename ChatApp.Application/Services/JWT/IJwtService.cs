using ChatApp.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Application.Services.JWT
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
