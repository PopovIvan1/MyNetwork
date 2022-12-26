using Microsoft.AspNetCore.Mvc;
using MyNetwork.Models;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Web;

namespace MyNetwork.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ApplicationContext db;

        public HomeController(ILogger<HomeController> logger, ApplicationContext db)
        {
            _logger = logger;
            this.db = db;
            TextModel.setContext("en");
            ReviewSettings.CurrentUser = "";
        }

        public IActionResult Index()
        {
            if (Response.HttpContext.Request.Cookies["language"] == "ru") TextModel.setContext("ru");
            else TextModel.setContext("en");
            ViewData.Model = db;
            ViewData.Add("category", ReviewSettings.Category);
            ViewData.Add("searchType", ReviewSettings.SearchType);
            ViewData.Add("tags", string.Join(' ', ReviewSettings.Tags));
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
            ReviewSettings.Category = category;
            ReviewSettings.SearchType = searchTupe;
            if (searchTupe == "tags" && tags != null) ReviewSettings.Tags = tags.Skip(1).Distinct().Select(tag => HttpUtility.UrlEncode(tag)).ToList();
            else ReviewSettings.Tags = new List<string>();
            return RedirectToAction("Index", "Home");
        }
    }
}