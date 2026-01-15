using System;

namespace PlatformaImprumut
{
    // clasa de baza pentru utilizatori
    public abstract class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    // admin
    public class Admin : User 
    { 
        public Admin() => Role = "Admin"; 
    }

    // utilizator normal
    public class RegularUser : User 
    { 
        public RegularUser() => Role = "User"; 
    }
}
