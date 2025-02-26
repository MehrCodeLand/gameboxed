using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Leyer.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Navigation property
    public ICollection<UserRole> UserRoles { get; set; }
}
