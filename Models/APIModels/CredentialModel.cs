using System.ComponentModel.DataAnnotations;

namespace AquariumMonitor.Models.APIModels
{
    public class CredentialModel
    {
        [Required(ErrorMessage = "Invalid Username / Password")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Invalid Username / Password")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Invalid Username / Password")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Invalid Username / Password")]
        //Minimum eight characters, at least one letter and one number
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", ErrorMessage = "Invalid Username / Password")]
        public string Password { get; set; }
    }
}
