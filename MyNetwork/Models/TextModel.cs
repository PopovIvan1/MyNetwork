namespace MyNetwork.Models
{
    public static class TextModel
    {
        public static Dictionary<string, string> Context = new Dictionary<string, string>();

        public static void setContext(string language)
        {
            Context = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("wwwroot/" + language + ".json"))!;
        }
    }
}
