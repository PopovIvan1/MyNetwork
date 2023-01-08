namespace MyNetwork.Models
{
    public class Like
    {
        public int Id { get; set; } = 0;
        public string UserId { get; set; } = string.Empty;
        public int ReviewId { get; set; } = 0;
        public User Author { get; set; } = new User();
        public Review Review { get; set; } = new Review();
    }
}
