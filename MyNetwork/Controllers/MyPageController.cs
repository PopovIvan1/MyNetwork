using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyNetwork.Data;
using MyNetwork.Models;
using System.Web;

namespace MyNetwork.Controllers
{
    public class MyPageController : Controller
    {
        private ApplicationContext db;
        private static string adminMode = "not available";
        private static string category = "all";
        private static string sortOrder = "no sort";

        public MyPageController(ApplicationContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            await checkUserAsync();
            ViewData.Model = db;
            ViewData.Add("admin mode", adminMode);
            ViewData.Add("username", ReviewSettings.CurrentUser);
            ViewData.Add("category", category);
            ViewData.Add("sortOrder", sortOrder);
            return View();
        }

        public IActionResult AdminMode()
        {
            ViewData.Model = db.AspNetUsers;
            return View();
        }

        public IActionResult NewReview()
        {
            ViewData.Model = db.Tags;
            return View();
        }

        public async Task<IActionResult> AddReviewToDbAsync(string reviewName, string creationName, string[] tags, string category, string description, string rate)
        {
            if (description.Contains(TextModel.Context["typing description"])) description = "";
            string userId = (await db.FindUserByNameAsync(ReviewSettings.CurrentUser)).Id;
            Review review = new Review() { Name = reviewName, CreationName = creationName, Category = category, Date = DateTime.Now, Description = description, AuthorRate = int.Parse(rate), AuthorId = userId };
            db.Reviews.Add(review);
            db.SaveChanges();
            await db.SetTagsToDb(tags, (await db.Reviews.FirstOrDefaultAsync(reviewFromDb => reviewFromDb.Date == review.Date && reviewFromDb.AuthorId == review.AuthorId)).Id);
            return RedirectToAction("Index", "MyPage");
        }

        public void UploadImage()
        {

        }

        public IActionResult BackToMyPage()
        {
            setAdminSettings();
            return RedirectToAction("Index", "MyPage");
        }

        public IActionResult BackToAdminMode()
        {
            setAdminSettings();
            return RedirectToAction("AdminMode", "MyPage");
        }

        public IActionResult SelectUser(string selectedUser)
        {
            if (selectedUser != null) ReviewSettings.CurrentUser = selectedUser;
            return RedirectToAction("Index", "MyPage");
        }

        public IActionResult ChangeParameters(string categoryFromView, string sortOrderFromView)
        {
            category = categoryFromView;
            sortOrder = sortOrderFromView;
            return RedirectToAction("Index", "MyPage");
        }

        private async Task checkUserAsync()
        {
            if (ReviewSettings.CurrentUser == "")
            {
                ReviewSettings.CurrentUser = User.Identity?.Name!;
                adminMode = (await db.FindUserByNameAsync(ReviewSettings.CurrentUser)).IsAdmin == "No        " ? "not available" : "available";
            }
            else if (ReviewSettings.CurrentUser != User.Identity?.Name!)
            {
                adminMode = TextModel.Context["in admin mode"] + ReviewSettings.CurrentUser;
            }
        }

        private void setAdminSettings()
        {
            adminMode = "available";
            ReviewSettings.CurrentUser = User.Identity?.Name!;
        }
    }
}
