namespace MyNetwork.Models
{
    public class Creation
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public IEnumerable<Review> Reviews { get; set; } = new List<Review>();
        public IEnumerable<Rate> Rates { get; set; } = new List<Rate>();
    }
}
