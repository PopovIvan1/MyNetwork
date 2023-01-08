namespace MyNetwork.Models
{
    public class Comment
    {
        public int Id { get; set; } = 0;
        public string UserId { get; set; } = string.Empty;
        public int ReviewId { get; set; } = 0;
        public string Context { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;

        public User Author { get; set; } = new User();
        public Review Review { get; set; } = new Review();
    }
}
