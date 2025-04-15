using Microsoft.AspNetCore.Identity;

namespace Document_Management_System.Models
{
    public class Users : IdentityUser
    {
        public string fullName { get; set; }
        public byte[]? ProfileImage { get; internal set; }
    }
}
