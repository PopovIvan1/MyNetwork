using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyNetwork.Models;

namespace MyNetwork.Controllers
{
    public class ReviewController : Controller
    {
        private ApplicationContext db;
        private static int currentReviewId = 0;

        public ReviewController(ApplicationContext db)
        {
            this.db = db;
        }

        public IActionResult ReviewPage(string review)
        {
            currentReviewId = int.Parse(review);
            ViewData.Add("reviewId", currentReviewId);
            ViewData.Model = db;
            return View();
        }

        public IActionResult EditReview(string review)
        {
            currentReviewId = int.Parse(review);
            ViewData.Add("reviewId", currentReviewId);
            ViewData.Model = db;
            return View();
        }

        public async Task<IActionResult> RemoveReview(string review)
        {
            db.Reviews.Remove(await db.Reviews.FirstOrDefaultAsync(review => review.Id == currentReviewId));
            db.SaveChanges();
            return RedirectToAction("Index", "MyPage");
        }

        public async Task<IActionResult> UpdateReview(string reviewName, string creationName, string[] tags, string category, string description, string rate)
        {
            await db.RemoveTags(currentReviewId);
            db.SaveChanges();
            Review currentReview = await db.Reviews.FirstOrDefaultAsync(review => review.Id == currentReviewId);
            currentReview.Name = reviewName;
            currentReview.CreationName = creationName;
            currentReview.Category = category;
            currentReview.Description = description;
            currentReview.AuthorRate= int.Parse(rate);
            db.SaveChanges();
            await db.SetTagsToDb(tags, currentReviewId);
            return RedirectToAction("Index", "MyPage");
        }
    }
}
