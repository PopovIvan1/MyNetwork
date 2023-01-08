namespace MyNetwork.Models
{
    public class Tag
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public IEnumerable<ReviewTag> ReviewTags { get; set; } = new List<ReviewTag>();
    }
}
