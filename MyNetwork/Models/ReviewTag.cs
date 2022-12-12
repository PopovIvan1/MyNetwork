namespace MyNetwork.Models
{
    public class ReviewTag
    {
        public int Id { get; set; } = 0!;
        public int ReviewId { get; set; }= 0!;
        public string TagName { get; set; } = string.Empty!;
    }
}
