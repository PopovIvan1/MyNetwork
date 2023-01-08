﻿using Microsoft.AspNetCore.Mvc;
using Korzh.EasyQuery.Linq;
using Microsoft.EntityFrameworkCore;
using MyNetwork.Models;
using MyNetwork.Services;
using System.Diagnostics;
using System.Web;

namespace MyNetwork.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationContext _db;
        private static string _category = "all";
        private static string _searchType = "best views";
        private static List<string> _tags = new List<string>();

        public HomeController(ApplicationContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            await setParamsAsync();
            ViewData.Model = _db;
            ViewData.Add("category", _category);
            ViewData.Add("searchType", _searchType);
            ViewData.Add("tags", string.Join(' ', _tags));
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
            _category = category;
            _searchType = searchTupe;
            if (searchTupe == "tags" && tags != null) _tags = tags.Skip(1).Distinct().Select(tag => HttpUtility.UrlEncode(tag)).ToList();
            else _tags = new List<string>();
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
                List<Creation> creations = await _db.Creations.FullTextSearchQuery(searchString).ToListAsync();
                searchResults.AddRange(fullDataReview.Where(r => creations.Contains(r.Creation)));
                List<string> users = await _db.AspNetUsers.FullTextSearchQuery(searchString).Select(user => user.Id).ToListAsync();
                searchResults.AddRange(fullDataReview.Where(review => users.Contains(review.AuthorId)));
                List<int> comments = await _db.Comments.FullTextSearchQuery(searchString).Select(comment => comment.ReviewId).ToListAsync();
                searchResults.AddRange(fullDataReview.Where(review => comments.Contains(review.Id)));
                List<int> tags = await _db.Tags.FullTextSearchQuery(searchString).Select(tag => tag.Id).ToListAsync();
                tags = await _db.ReviewTags.Where(reviewTag => tags.Contains(reviewTag.TagId)).Select(tag => tag.ReviewId).ToListAsync();
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
                CurrentUserSettings.CurrentUser = await _db.AspNetUsers.FirstOrDefaultAsync(user => user.UserName == User.Identity.Name);
            }
        }
    }
}