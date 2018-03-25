using AquariumMonitor.APIModels;
using System.ComponentModel.DataAnnotations;

namespace AquariumMonitor.Models.APIModels
{
    public class PasswordModel : IAPIModel
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
