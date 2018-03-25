using System;
using System.Collections.Generic;

namespace AquariumMonitor.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTimeOffset LastLogin { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
