namespace MyNetwork.Models
{
    public static class CurrentUserSettings
    {
        public  static User CurrentUser { get; set; } = new User();
        public static string AdminMode { get; set; } = "";
    }
}
