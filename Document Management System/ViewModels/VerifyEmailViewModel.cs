using System.ComponentModel.DataAnnotations;

namespace Document_Management_System.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage ="Email is required")]
        [EmailAddress]
        public string email { get; set; }
    }
}
