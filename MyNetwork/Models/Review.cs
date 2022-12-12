using NuGet.Protocol;

namespace MyNetwork.Models
{
    public class Review
    {
        public int Id { get; set; } = 0!;
        public string Name { get; set; } = string.Empty!;
        public string CreationName { get; set; } = string.Empty!;
        public string Description { get; set; } = string.Empty!;
        public string Category { get; set; } = string.Empty!;
        public int AuthorRate { get; set; } = 0!;
        public string ImageUrl { get; set; } = string.Empty!;
        public float UsersRate { get; set; } = 0!;
    }
}
