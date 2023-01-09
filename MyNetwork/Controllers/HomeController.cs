using Microsoft.AspNetCore.Mvc;
using Korzh.EasyQuery.Linq;
using Microsoft.EntityFrameworkCore;
using MyNetwork.Models;
using MyNetwork.Services;
using System.Diagnostics;
using System.Web;
using System.Linq;

namespace MyNetwork.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationContext _db;

        public HomeController(ApplicationContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            await setParamsAsync();
            ViewData.Model = _db;
            ViewData.Add("popular tags", string.Join(' ', _db.Services.Tags.SelectPopularTags()));
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
            Response.Cookies.Append("homeCategory", category);
            Response.Cookies.Append("homeSearchType", searchTupe);
            if (searchTupe == "tags" && tags != null) Response.Cookies.Append("homeTags", string.Join(' ', tags.Skip(1).Distinct().Select(tag => HttpUtility.UrlEncode(tag)).ToList()));
            else Response.Cookies.Append("homeTags", "");
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> FullTextSearch(string searchString)
        {
            List<Review> searchResults = new List<Review>();
            if (!string.IsNullOrEmpty(searchString))
            {
                var fullDataReview = await _db.Services.Reviews.GetFullReviewData();
                List<Review> reviews = await _db.Reviews.FullTextSearchQuery(searchString).ToListAsync();
                searchResults.AddRange(fullDataReview.Where(r => reviews.Contains(r)));
                searchResults.AddRange(fullDataReview.Where(review => _db.Creations.FullTextSearchQuery(searchString).Contains(review.Creation)));
                searchResults.AddRange(fullDataReview.Where(review => _db.AspNetUsers.FullTextSearchQuery(searchString).Contains(review.Author)));
                searchResults.AddRange(fullDataReview.Where(review => _db.Comments.FullTextSearchQuery(searchString).Select(c => c.ReviewId).Contains(review.Id)));
                List<int> tags = new List<int>();
                await _db.Tags.FullTextSearchQuery(searchString).ForEachAsync(t => tags.AddRange(t.ReviewTags.Select(rt => rt.ReviewId)));
                searchResults.AddRange(fullDataReview.Where(review => tags.Contains(review.Id)));
            }
            ViewData.Model = searchResults.Distinct().ToList();
            ViewData.Add("searchString", searchString == null ? "" : searchString);
            return View();
        }

        private async Task setParamsAsync()
        {
            if (ImageService.getToken() == "")
            {
                ImageService.setToken(_db.AdminDatas.FirstOrDefault(data => data.Name == "token") == null ? "" :
                    _db.AdminDatas.FirstOrDefault(data => data.Name == "token").Value);
            }
            if (Response.HttpContext.Request.Cookies["language"] == "ru") TextModel.setContext("ru");
            else TextModel.setContext("en");
            if (string.IsNullOrEmpty(Response.HttpContext.Request.Cookies["homeCategory"])) Response.Cookies.Append("homeCategory", "all");
            if (string.IsNullOrEmpty(Response.HttpContext.Request.Cookies["homeSearchType"])) Response.Cookies.Append("homeSearchType", "last views");
            if (string.IsNullOrEmpty(Response.HttpContext.Request.Cookies["homeTags"])) Response.Cookies.Append("homeTags", "");
            await setUserSettings();
        }

        private async Task setUserSettings()
        {
            if (User.Identity?.Name! == null)
            {
                Response.Cookies.Append("currentUser", "");
                Response.Cookies.Append("adminMode", "");
            }
            else if (string.IsNullOrEmpty(Response.HttpContext.Request.Cookies["currentUser"]))
            {
                Response.Cookies.Append("currentUser", (await _db.AspNetUsers.FirstOrDefaultAsync(user => user.UserName == User.Identity.Name)).UserName);
            }
        }
    }
}