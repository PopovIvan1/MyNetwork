using Microsoft.AspNetCore.Identity;

namespace MyNetwork.Models
{
    public class User: IdentityUser
    {
        public string IsAdmin { get; set; } = string.Empty;
        public string IsBlock { get; set; } = "No";

        public IEnumerable<Comment> Comments { get; set; } = new List<Comment>();
        public IEnumerable<Like> Likes { get; set; } = new List<Like>();
        public IEnumerable<Review> Reviews { get; set; } = new List<Review>();
        public IEnumerable<Rate> Rates { get; set; } = new List<Rate>();
    }
}
