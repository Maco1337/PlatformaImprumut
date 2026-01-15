using System;

namespace PlatformaImprumut
{
    // Clasa de baza (Abstracta - nu poti crea un User "gol")
    public abstract class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    // Moștenire: Admin este un User
    public class Admin : User 
    { 
        public Admin() => Role = "Admin"; 
    }

    // Moștenire: RegularUser este un User
    public class RegularUser : User 
    { 
        public RegularUser() => Role = "User"; 
    }
}