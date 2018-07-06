
using System.ComponentModel.DataAnnotations;

namespace AquariumMonitor.Models.ViewModels
{
    public class PasswordModel : IViewModel
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
