using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Domain.Entities.Common
{
    public class BaseEntityWithoutId
    {
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime? ModificationDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletionDate { get; set; }
    }
}
