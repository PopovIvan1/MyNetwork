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

        public async Task<IActionResult> ReviewPage(string review)
        {
            currentReviewId = int.Parse(review);
            ViewData.Add("reviewId", currentReviewId);
            if (User.Identity.IsAuthenticated) 
            { 
                ViewData.Add("userId", (await db.FindUserByNameAsync(User.Identity.Name)).Id); 
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
            ImageService.Remove(currentReview.ImageUrl);
            await db.Rates.Where(rate => rate.ReviewId == currentReview.Id).ForEachAsync(rate => db.Rates.Remove(rate));
            db.Reviews.Remove(currentReview);
            db.SaveChanges();
            return RedirectToAction("MyPage", "MyPage");
        }

        public async Task<IActionResult> UpdateReview(string reviewName, string creationName, string[] tags, string category, string description, string rate, IFormFile image)
        {
            await db.RemoveTags(currentReviewId);
            db.SaveChanges();
            Review currentReview = await db.Reviews.FirstOrDefaultAsync(review => review.Id == currentReviewId);
            string imgName = "";
            if (image != null && image.ContentType.Contains("image"))
            {
                ImageService.Remove(currentReview.ImageUrl);
                imgName = DateTime.Now.ToString().Replace('.', '-').Replace(' ', '-').Replace(':', '-').Replace('/', '-') + '.' + image.FileName.Split('.').Last();
                using (var fileStream = image.OpenReadStream())
                {
                    byte[] bytes = new byte[image.Length];
                    fileStream.Read(bytes, 0, (int)image.Length);
                    await ImageService.Upload(imgName, bytes);
                }
                currentReview.ImageUrl = imgName;
            }
            currentReview.Name = reviewName;
            currentReview.CreationName = creationName;
            currentReview.Category = category;
            currentReview.Description = description;
            currentReview.AuthorRate= int.Parse(rate);
            db.SaveChanges();
            await db.SetTagsToDb(tags, currentReviewId);
            return RedirectToAction("MyPage", "MyPage");
        }

        public async Task<IActionResult> NewComment(string commentContext)
        {
            User user = await db.FindUserByNameAsync(ReviewSettings.CurrentUser);
            db.Comments.Add(new Comment { ReviewId = currentReviewId, Context = commentContext, Date = DateTime.Now, UserId = user.Id, UserName = ReviewSettings.CurrentUser, UserLikes = user.Likes });
            db.SaveChanges();
            return RedirectToAction("ReviewPage", "Review", new { review = currentReviewId.ToString() });
        }

        public async Task LikeReview(int likeCount, int likeOrDislike)
        {
            string userId = (await db.FindUserByNameAsync(ReviewSettings.CurrentUser == "" ? User.Identity.Name: ReviewSettings.CurrentUser)).Id;
            if (likeOrDislike == 1)
            {
                db.Likes.Add(new Like { ReviewId = currentReviewId, UserId = userId });
                (await db.Reviews.FirstOrDefaultAsync(review => review.Id == currentReviewId)).Likes++;
            }
            else
            {
                db.Likes.Remove(await db.Likes.FirstOrDefaultAsync(like => like.UserId == userId && like.ReviewId == currentReviewId));
                (await db.Reviews.FirstOrDefaultAsync(review => review.Id == currentReviewId)).Likes--;
            }
            db.SaveChanges();
        }

        public async Task ChangeRate(int rate, string creationName)
        {
            string userId = (await db.FindUserByNameAsync(ReviewSettings.CurrentUser == "" ? User.Identity.Name : ReviewSettings.CurrentUser)).Id;
            Rate currentRate = await db.Rates.FirstOrDefaultAsync(rate => rate.UserId == userId && rate.ReviewId == currentReviewId);
            if (currentRate == null) db.Rates.Add(new Rate { UserRate = rate, ReviewId = currentReviewId, UserId = userId, CreationName = creationName });
            else currentRate.UserRate = rate;
            db.SaveChanges();
        }
    }
}
