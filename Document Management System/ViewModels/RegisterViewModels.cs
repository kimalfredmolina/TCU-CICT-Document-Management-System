using System.ComponentModel.DataAnnotations;

namespace Document_Management_System.ViewModels
{
    public class RegisterViewModels
    {
        public byte[] ProfileImage { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string name { get; set; }
        
        [Required(ErrorMessage = "E-Mail is required")]
        [EmailAddress]
        public string email { get; set; }

        [Required(ErrorMessage ="Password is required")]
        [StringLength(40, MinimumLength = 8, ErrorMessage ="The {0} must be at {2} and max at {1} characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [Compare("confirmPassword", ErrorMessage ="Password does not match.")]
        public string password { get; set; }

        [Required(ErrorMessage ="Confirm Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string confirmPassword { get; set; }
    }
}
