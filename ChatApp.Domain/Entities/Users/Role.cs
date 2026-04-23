using ChatApp.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Domain.Entities.Users
{
    public class Role:BaseEntity<long>
    {
        public string Name { get; set; }
        public ICollection<UserInRole> UserRoles { get; set; }
    }
}
