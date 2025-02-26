using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Leyer.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; }
    public ICollection<GameRating> GameRatings { get; set; }
}
