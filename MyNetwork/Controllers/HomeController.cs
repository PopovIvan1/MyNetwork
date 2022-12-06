using Microsoft.AspNetCore.Mvc;
using MyNetwork.Models;
using System.Diagnostics;

namespace MyNetwork.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (Response.HttpContext.Request.Cookies["language"] == "ru") TextModel.setContext("ru");
            else TextModel.setContext("en");
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

        public ActionResult ChangeTheme(string theme)
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

        public ActionResult ChangeLanguage(string language)
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
    }
}