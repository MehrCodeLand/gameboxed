using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Leyer.DTOs;

public class UserUpdateDto
{
    public string Username { get; set; }
    public string Email { get; set; }

    // Optional: If you want to allow password updates through this method
    public string Password { get; set; }
}
