using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyNetwork.Models;
using MyNetwork.Services;

namespace MyNetwork.Controllers
{
    public class ReviewController : Controller
    {
        private ApplicationContext _db;

        public ReviewController(ApplicationContext db)
        {
            _db = db;
        }

        public IActionResult NewReview()
        {
            ViewData.Model = _db.Tags;
            return View();
        }

        public async Task<IActionResult> CreationReview(string reviewName, string creationName, string[] tags, string category, string description, string rate, IFormFile[] image, string action, string isImageDeleted)
        {
            if (action == "addToDb") return await AddReviewToDbAsync(reviewName, creationName, tags, category, description, rate, image);
            else return await UpdateReview(reviewName, creationName, tags, category, description, rate, image, isImageDeleted);
        }

        public async Task<IActionResult> ReviewPage(string review)
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewData.Add("userId", (await _db.AspNetUsers.FirstOrDefaultAsync(user => user.UserName == Response.HttpContext.Request.Cookies["currentUser"])).Id);
            }
            List<Review> viewModel = new List<Review>();
            Review currentReview = (await _db.Services.Reviews.GetFullReviewData()).FirstOrDefault(r => r.Id == int.Parse(review));
            currentReview.Comments.ToList().AddRange(_db.Comments.Include(c => c.Author).ThenInclude(a => a.Likes)
                .Where(comment => comment.ReviewId == currentReview.Id));
            viewModel.Add(currentReview);
            viewModel.AddRange((await _db.Services.Reviews.GetFullReviewData()).Where(r => r.CreationId == currentReview.CreationId && r.Id != currentReview.Id));
            ViewData.Model = viewModel;
            Response.Cookies.Append("currentReview", currentReview.Id.ToString());
            return View();
        }

        public async Task<IActionResult> EditReview(string review)
        {
            Review currentReview = (await _db.Services.Reviews.GetFullReviewData()).FirstOrDefault(r => r.Id == int.Parse(review));
            ViewData.Model = currentReview;
            Response.Cookies.Append("currentReview", currentReview.Id.ToString());
            return View();
        }

        public async Task<IActionResult> RemoveReview(string reviewId)
        {
            Review currentReview = await _db.Reviews.Include(review => review.Likes).FirstOrDefaultAsync(review => review.Id == int.Parse(reviewId));
            await _db.Services.Reviews.RemoveReview(currentReview);
            return RedirectToAction("MyPage", "MyPage");
        }

        public async Task<IActionResult> UpdateReview(string reviewName, string creationName, string[] tags, string category, string description, string rate, IFormFile[] image, string isImageDeleted)
        {
            Review currentReview = (await _db.Services.Reviews.GetFullReviewData()).FirstOrDefault(review => review.Id == int.Parse(Response.HttpContext.Request.Cookies["currentReview"]));
            currentReview.Tags.ToList().ForEach(tag => _db.ReviewTags.Remove(tag));
            currentReview.ImageUrl = await getImgUrl(currentReview.ImageUrl, isImageDeleted, image);
            int oldCreation = currentReview.CreationId;
            Creation creation = await _db.Services.Creations.GetCreation(creationName);
            if (oldCreation != creation.Id)
            {
                currentReview.Creation = creation;
                currentReview.CreationId = creation.Id;
            }
            currentReview.Name = reviewName;
            currentReview.Category = category;
            currentReview.Description = description;
            currentReview.AuthorRate = int.Parse(rate);
            _db.SaveChanges();
            currentReview.Tags.ToList().AddRange(await _db.Services.Tags.SetTagsToDb(tags, currentReview));
            _db.SaveChanges();
            _db.Services.Rates.UpdateRates(currentReview.Id, currentReview.Creation.Id);
            if (oldCreation != creation.Id) await _db.Services.Creations.CheckOldCreation(oldCreation);
            _db.Services.Tags.RemoveTags();
            _db.SaveChanges();
            return RedirectToAction("MyPage", "MyPage");
        }

        public async Task<IActionResult> AddReviewToDbAsync(string reviewName, string creationName, string[] tags, string category, string description, string rate, IFormFile[] image)
        {
            User currentUser = await getCurrentUser();
            if (description.Contains(TextModel.Context["typing description"])) description = "";
            string imgName = await ImageService.GetImageName(image);
            Creation creation = await _db.Services.Creations.GetCreation(creationName);
            try
            {
                _db.Creations.Attach(creation);
            }
            catch(Exception ex) { }
            Review review = new Review() { Name = reviewName, Creation = creation, Category = category, Date = DateTime.Now, Description = description, AuthorRate = int.Parse(rate), ImageUrl = imgName };
            currentUser.Reviews.ToList().Add(review);
            review.Author = currentUser;
            review.AuthorId = currentUser.Id;
            _db.Reviews.Attach(review);
            _db.SaveChanges();
            review.Tags.ToList().AddRange(await _db.Services.Tags.SetTagsToDb(tags, review));
            _db.SaveChanges();
            return RedirectToAction("MyPage", "MyPage");
        }

        public async Task<IActionResult> NewComment(string commentContext)
        {
            User currentUser = await getCurrentUser();
            Review review = (await _db.Services.Reviews.GetFullReviewData()).FirstOrDefault(review => review.Id == int.Parse(Response.HttpContext.Request.Cookies["currentReview"]));
            Comment comment = new Comment { ReviewId = review.Id, Context = commentContext, Date = DateTime.Now, UserId = currentUser.Id, Author = currentUser, Review = review };
            _db.Comments.Attach(comment);
            review.Comments.ToList().Add(comment);
            _db.SaveChanges();
            return RedirectToAction("ReviewPage", "Review", new { review = review.Id.ToString() });
        }

        public async Task<string> UpdateComments(int currentCommentsCount, int reviewId)
        {
            Review currentReview = await _db.Reviews.Include(r => r.Comments)
                .ThenInclude(c => c.Author)
                .ThenInclude(a => a.Likes)
                .FirstAsync(review => review.Id == reviewId);
            if (currentReview.Comments.Count() == currentCommentsCount) return "";
            Comment newComment = currentReview.Comments.Last();
            return $"<tr><td style=\"width: 40%\">{newComment.Author.UserName} " +
                $"(<i style=\"color: green\">{newComment.Author.Likes.Count()}</i>):<p style=\"color: grey\">" +
                $"{newComment.Date.ToShortDateString()}</p></td><td align=\"left\" style=\"width: 60%\">" +
                $"{newComment.Context}</td></tr>";
        }

        public async Task LikeReview(int likeOrDislike)
        {
            User currentUser = await getCurrentUser();
            Review review = (await _db.Services.Reviews.GetFullReviewData()).FirstOrDefault(review => review.Id == int.Parse(Response.HttpContext.Request.Cookies["currentReview"]));
            if (likeOrDislike == 1)
            {
                Like like = new Like { ReviewId = review.Id, UserId = currentUser.Id, Author = currentUser, Review = review };
                _db.Likes.Attach(like);
                review.Likes.ToList().Add(like);
            }
            else
            {
                _db.Likes.Remove(await _db.Likes.FirstOrDefaultAsync(like => like.UserId == currentUser.Id && like.ReviewId == review.Id));
            }
            _db.SaveChanges();
        }

        public async Task ChangeCreationRate(int rate, string creationName)
        {
            User currentUser = await getCurrentUser();
            Review review = (await _db.Services.Reviews.GetFullReviewData()).FirstOrDefault(review => review.Id == int.Parse(Response.HttpContext.Request.Cookies["currentReview"]));
            Rate currentRate = await _db.Rates.FirstOrDefaultAsync(rate => rate.UserId == currentUser.Id && rate.ReviewId == review.Id);
            if (currentRate == null)
            {
                Rate newRate = new Rate { UserRate = rate, ReviewId = review.Id, UserId = currentUser.Id, CreationId = review.CreationId,
                Author = currentUser, Review = review, Creation = review.Creation};
                _db.Rates.Attach(newRate);
                review.Rates.ToList().Add(newRate);
            }
            else currentRate.UserRate = rate;
            _db.SaveChanges();
        }

        private async Task<string> getImgUrl(string imgUrl, string isDeleted, IFormFile[] image)
        {
            string imgName = await ImageService.GetImageName(image);
            if (!string.IsNullOrEmpty(imgName))
            {
                if (imgUrl != "") await ImageService.Remove(imgUrl);
                imgUrl = imgName;
            }
            else if (isDeleted == "yes")
            {
                await ImageService.Remove(imgUrl);
                imgUrl = "";
            }
            return imgUrl;
        }

        private async Task<User> getCurrentUser()
        {
            return await _db.Services.Users.GetFullUserData(await _db.AspNetUsers.
                FirstOrDefaultAsync(user => user.UserName == Response.HttpContext.Request.Cookies["currentUser"]));
        }
    }
}
