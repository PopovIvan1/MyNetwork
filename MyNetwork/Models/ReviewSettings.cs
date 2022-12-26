namespace MyNetwork.Models
{
    public static class ReviewSettings
    {
        public static string Category { get; set; } = "all";
        public static string SearchType { get; set; } = "best views";
        public static List<string> Tags { get; set; } = new List<string>();
        public  static string CurrentUser { get; set; } = "";
    }
}
