using Microsoft.AspNetCore.Mvc;
using MyNetwork.Models;
using System.Diagnostics;
using System.Web;

namespace MyNetwork.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationContext db;
        private static string _category = "all";
        private static string _searchType = "best views";
        private static List<string> _tags = new List<string>();

        public HomeController(ApplicationContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            await setParamsAsync();
            ViewData.Model = db;
            ViewData.Add("category", _category);
            ViewData.Add("searchType", _searchType);
            ViewData.Add("tags", string.Join(' ', _tags));
            ViewData.Add("popular tags", string.Join(' ', db.SelectPopularTags()));
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult ChangeTheme(string theme)
        {
            if (theme == "dark")
            {
                Response.Cookies.Append("theme", "dark");
            }
            else
            {
                Response.Cookies.Append("theme", "light");
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ChangeLanguage(string language)
        {
            if (language == "en")
            {
                Response.Cookies.Append("language", "en");
            }
            else
            {
                Response.Cookies.Append("language", "ru");
            }
            TextModel.setContext(language);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ChangeReviewParameters(string category, string searchTupe, string[]? tags = null)
        {
            _category = category;
           _searchType = searchTupe;
            if (searchTupe == "tags" && tags != null) _tags = tags.Skip(1).Distinct().Select(tag => HttpUtility.UrlEncode(tag)).ToList();
            else _tags = new List<string>();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult FullTextSearch(string searchString)
        {
            ViewData.Model = db;
            ViewData.Add("searchString", searchString == null ? "" : searchString);
            return View();
        }

        private async Task setParamsAsync()
        {
            if (ImageService.getToken() == "")
            {
                ImageService.setToken(db.AdminDatas.FirstOrDefault(data => data.Name == "token") == null ? "" : 
                    db.AdminDatas.FirstOrDefault(data => data.Name == "token").Value);
            }
            if (Response.HttpContext.Request.Cookies["language"] == null)
            {
                if (Response.HttpContext.Request.Cookies["language"] == "ru") TextModel.setContext("ru");
                else TextModel.setContext("en");
            }
            await setUserSettings();
        }

        private async Task setUserSettings()
        {
            if (User.Identity?.Name! == null)
            {
                CurrentUserSettings.CurrentUser = new User();
                CurrentUserSettings.AdminMode = "";
            }
            else if (CurrentUserSettings.CurrentUser.UserName == null)
            {
                CurrentUserSettings.CurrentUser = await db.FindUserByNameAsync(User.Identity?.Name!);
            }
        }
    }
}