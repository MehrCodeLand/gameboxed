using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Leyer.Entities
{
    public class UserSession
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public User User { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Token length cannot exceed 1000 characters.")]
        public string Token { get; set; }

        [Required]
        public bool IsLoggedIn { get; set; }

        [Required]
        public DateTime LoginTime { get; set; }

        public DateTime? LogoutTime { get; set; }
    }
}
