using Microsoft.AspNetCore.Identity;

namespace MyNetwork.Models
{
    public class User: IdentityUser
    {
        public string IsAdmin { get; set; } = string.Empty!;
        public int Likes { get; set; } = 0!;
    }
}
