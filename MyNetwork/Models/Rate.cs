namespace MyNetwork.Models
{
    public class Rate
    {
        public int Id { get; set; } = 0!;
        public string UserId { get; set; } = string.Empty!;
        public int ReviewId { get; set; } = 0!;
        public int UserRate { get; set; } = 0!;
    }
}
