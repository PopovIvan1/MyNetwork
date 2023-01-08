namespace MyNetwork.Models
{
    public class Review
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int AuthorRate { get; set; } = 0;
        public int CreationId { get; set; } = 0;
        public string AuthorId { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;

        public IEnumerable<ReviewTag> Tags { get; set; } = new List<ReviewTag>();
        public IEnumerable<Comment> Comments { get; set; } = new List<Comment>();
        public IEnumerable<Like> Likes { get; set; } = new List<Like>();
        public Creation Creation { get; set; } = new Creation();
        public User Author { get; set; } = new User();
        public IEnumerable<Rate> Rates { get; set; } = new List<Rate>();
    }
}
