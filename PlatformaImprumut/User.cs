using System;

namespace PlatformaImprumut
{
    
    public abstract class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    
    public class Admin : User 
    { 
        public Admin() => Role = "Admin"; 
    }

    
    public class RegularUser : User 
    { 
        public RegularUser() => Role = "User"; 
    }
}
