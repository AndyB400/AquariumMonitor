using System;

namespace AquariumMonitor.Models
{
    public class UserPassword 
    {
        public UserPassword()
        {
        }

        public UserPassword(int userId, string passwordHashAndSalt)
        {
            UserId = userId;
            PasswordHashAndSalt = passwordHashAndSalt;
            Created = DateTimeOffset.Now;
        }

        public int UserId { get; set; }
        public string PasswordHashAndSalt { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Expired { get; set; }
    }
}
