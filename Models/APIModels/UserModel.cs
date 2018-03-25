using AquariumMonitor.APIModels;
using System.ComponentModel.DataAnnotations;

namespace AquariumMonitor.Models.APIModels
{
    public class UserModel : IAPIModel
    {
        public string Url { get; set; }
        [Required]
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
