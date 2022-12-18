using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyNetwork.Data;
using MyNetwork.Models;

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
            return View();
        }

        public IActionResult AdminMode()
        {
            ViewData.Model = db.AspNetUsers;
            return View();
        }

        public IActionResult NewReview()
        {
            return View();
        }

        public async Task<IActionResult> AddReviewToDbAsync(string reviewName, string creationName, string[] tags, string category, string description, string rate)
        {
            if (description == TextModel.Context["typing description"]) description = "";
            string userId = (await db.AspNetUsers.FirstOrDefaultAsync(user => user.UserName == User.Identity.Name)).Id;
            Review review = new Review() { Name = reviewName, CreationName = creationName, Category = category, Date = DateTime.Now, Description = description, AuthorRate = int.Parse(rate), AuthorId = userId };
            db.Reviews.Add(review);
            db.SaveChanges();
            await setTagsToDb(tags, (await db.Reviews.FirstOrDefaultAsync(reviewFromDb => reviewFromDb.Date == review.Date && reviewFromDb.AuthorId == review.AuthorId)).Id);
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

        private async Task setTagsToDb(string[] tags, int reviewId)
        { 
            foreach (var tag in tags.Skip(1).Distinct()) 
            {
                Tag tagFromDb = await db.Tags.FirstOrDefaultAsync(tagFromDb => tagFromDb.Name == tag);
                if (tagFromDb != null) tagFromDb.ReviewsCount++;
                else db.Tags.Add(new Tag() { Name = tag, ReviewsCount = 0 });
                db.SaveChanges();
                db.ReviewTags.Add(new ReviewTag() { ReviewId = reviewId, TagId = (await db.Tags.FirstOrDefaultAsync(tagFromDb => tagFromDb.Name == tag)).Id });
            }
            db.SaveChanges();
        }

        private void setAdminSettings()
        {
            CurrentUserSettings.adminMode = "available";
            CurrentUserSettings.currentUser = User.Identity?.Name!;
        }
    }
}
