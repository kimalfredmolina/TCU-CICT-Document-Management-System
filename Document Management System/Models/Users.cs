using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Document_Management_System.Models
{
    public class Users : IdentityUser
    {
        [Column("fullName")]
        public string fullName { get; set; }
        public byte[]? ProfileImage { get; internal set; }
    }
}
