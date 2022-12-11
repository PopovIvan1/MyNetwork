using Microsoft.AspNetCore.Identity;

namespace MyNetwork.Models
{
    public class User: IdentityUser
    {
        public string IsAdmin { get; set; }
    }
}
