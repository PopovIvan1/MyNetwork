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

        public MyPageController(ApplicationContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            await checkUserAsync();
            ViewData.Model = db;
            ViewData.Add("admin mode", CurrentUserSettings.adminMode);
            ViewData.Add("username", CurrentUserSettings.currentUser);
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
            string userId = (await db.AspNetUsers.FirstOrDefaultAsync(user => user.UserName == CurrentUserSettings.currentUser)).Id;
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
            if (selectedUser != null) CurrentUserSettings.currentUser = selectedUser;
            return RedirectToAction("Index", "MyPage");
        }

        private async Task checkUserAsync()
        {
            if (CurrentUserSettings.currentUser == "")
            {
                CurrentUserSettings.currentUser = User.Identity?.Name!;
                CurrentUserSettings.adminMode = await db.IsAdminAsync(CurrentUserSettings.currentUser) ? "available" : "not available";
            }
            else if (CurrentUserSettings.currentUser != User.Identity?.Name!)
            {
                CurrentUserSettings.adminMode = TextModel.Context["in admin mode"] + CurrentUserSettings.currentUser;
            }
        }

        private void setAdminSettings()
        {
            CurrentUserSettings.adminMode = "available";
            CurrentUserSettings.currentUser = User.Identity?.Name!;
        }
    }
}
