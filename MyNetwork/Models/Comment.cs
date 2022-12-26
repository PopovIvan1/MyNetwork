namespace MyNetwork.Models
{
    public class Comment
    {
        public int Id { get; set; } = 0!;
        public string UserId { get; set; } = string.Empty!;
        public int ReviewId { get; set; } = 0!;
        public string Context { get; set; } = string.Empty!;
        public DateTime Date { get; set; } = DateTime.Now!;
        public string UserName { get; set; } = string.Empty!;
        public int UserLikes { get; set; } = 0!;
    }
}
