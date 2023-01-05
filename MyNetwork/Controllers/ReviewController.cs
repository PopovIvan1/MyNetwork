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

        public IActionResult NewReview()
        {
            ViewData.Model = db.Tags;
            return View();
        }

        public async Task<IActionResult> CreationReview(string reviewName, string creationName, string[] tags, string category, string description, string rate, IFormFile[] image, string action, string isImageDeleted)
        {
            if (action == "addToDb") return await AddReviewToDbAsync(reviewName, creationName, tags, category, description, rate, image);
            else return await UpdateReview(reviewName, creationName, tags, category, description, rate, image, isImageDeleted);
        }

        public IActionResult ReviewPage(string review)
        {
            currentReviewId = int.Parse(review);
            ViewData.Add("reviewId", currentReviewId);
            if (User.Identity.IsAuthenticated)
            {
                ViewData.Add("userId", CurrentUserSettings.CurrentUser.Id);
            }
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

        public async Task<IActionResult> RemoveReview(string reviewId)
        {
            Review currentReview = await db.Reviews.FirstOrDefaultAsync(review => review.Id == int.Parse(reviewId));
            (await db.GetUserByIdAsync(currentReview.AuthorId)).Likes -= currentReview.Likes;
            if (!string.IsNullOrEmpty(currentReview.ImageUrl)) await ImageService.Remove(currentReview.ImageUrl);
            await db.Rates.Where(rate => rate.ReviewId == currentReview.Id).ForEachAsync(rate => db.Rates.Remove(rate));
            db.Reviews.Remove(currentReview);
            db.SaveChanges();
            return RedirectToAction("MyPage", "MyPage");
        }

        public async Task<IActionResult> UpdateReview(string reviewName, string creationName, string[] tags, string category, string description, string rate, IFormFile[] image, string isImageDeleted)
        {
            await db.RemoveTags(currentReviewId);
            Review currentReview = await db.Reviews.FirstOrDefaultAsync(review => review.Id == currentReviewId);
            string imgName = await ImageService.GetImageName(image);
            if (!string.IsNullOrEmpty(imgName)) currentReview.ImageUrl = imgName;
            else if (isImageDeleted == "yes")
            {
                await ImageService.Remove(currentReview.ImageUrl);
                currentReview.ImageUrl = "";
            }
            string oldCreationName = currentReview.CreationName;
            currentReview.CreationName = creationName;
            currentReview.Name = reviewName;
            currentReview.Category = category;
            currentReview.Description = description;
            currentReview.AuthorRate= int.Parse(rate);
            db.SaveChanges();
            await db.SetTagsToDb(tags, currentReviewId);
            if (oldCreationName != creationName) db.UpdateRates(currentReviewId, creationName, oldCreationName);
            return RedirectToAction("MyPage", "MyPage");
        }

        public async Task<IActionResult> AddReviewToDbAsync(string reviewName, string creationName, string[] tags, string category, string description, string rate, IFormFile[] image)
        {
            if (description.Contains(TextModel.Context["typing description"])) description = "";
            string imgName = await ImageService.GetImageName(image);
            Review review = new Review() { Name = reviewName, CreationName = creationName, Category = category, Date = DateTime.Now, Description = description, AuthorRate = int.Parse(rate), AuthorId = CurrentUserSettings.CurrentUser.Id, ImageUrl = imgName };
            db.Reviews.Add(review);
            db.SaveChanges();
            await db.SetTagsToDb(tags, (await db.Reviews.FirstOrDefaultAsync(reviewFromDb => reviewFromDb.Date == review.Date && reviewFromDb.AuthorId == review.AuthorId)).Id);
            return RedirectToAction("MyPage", "MyPage");
        }

        public IActionResult NewComment(string commentContext)
        {
            db.Comments.Add(new Comment { ReviewId = currentReviewId, Context = commentContext, Date = DateTime.Now, UserId = CurrentUserSettings.CurrentUser.Id, UserName = CurrentUserSettings.CurrentUser.UserName, UserLikes = CurrentUserSettings.CurrentUser.Likes });
            db.SaveChanges();
            return RedirectToAction("ReviewPage", "Review", new { review = currentReviewId.ToString() });
        }

        public string UpdateComments(int currentCommentsCount, int reviewId)
        {
            if (db.SelectReviewComments(reviewId).Count == currentCommentsCount) return "";
            Comment newComment = db.SelectReviewComments(reviewId).Last();
            if (newComment.UserName == CurrentUserSettings.CurrentUser.UserName) return "";
            return $"<tr><td style=\"width: 40%\">{newComment.UserName} " +
                $"(<i style=\"color: green\">{newComment.UserLikes}</i>):<p style=\"color: grey\">" +
                $"{newComment.Date.ToShortDateString()}</p></td><td align=\"left\" style=\"width: 60%\">" +
                $"{newComment.Context}</td></tr>";
        }

        public async Task LikeReview(int likeCount, int likeOrDislike)
        {
            if (likeOrDislike == 1)
            {
                db.Likes.Add(new Like { ReviewId = currentReviewId, UserId = CurrentUserSettings.CurrentUser.Id });
                (await db.Reviews.FirstOrDefaultAsync(review => review.Id == currentReviewId)).Likes++;
            }
            else
            {
                db.Likes.Remove(await db.Likes.FirstOrDefaultAsync(like => like.UserId == CurrentUserSettings.CurrentUser.Id && like.ReviewId == currentReviewId));
                (await db.Reviews.FirstOrDefaultAsync(review => review.Id == currentReviewId)).Likes--;
            }
            db.SaveChanges();
        }

        public async Task ChangeCreationRate(int rate, string creationName)
        {
            Rate currentRate = await db.Rates.FirstOrDefaultAsync(rate => rate.UserId == CurrentUserSettings.CurrentUser.Id && rate.ReviewId == currentReviewId);
            if (currentRate == null) db.Rates.Add(new Rate { UserRate = rate, ReviewId = currentReviewId, UserId = CurrentUserSettings.CurrentUser.Id, CreationName = creationName });
            else currentRate.UserRate = rate;
            db.SaveChanges();
        }
    }
}
