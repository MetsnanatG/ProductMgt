using Microsoft.AspNetCore.Identity;

namespace webapi.Models
{
    public class User : IdentityUser
    {

       
        public string Role { get; set; } // e.g., Tester, TestLead, Manager
    }

}

