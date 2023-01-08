using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyNetwork.Models;
using MyNetwork.Services;

namespace MyNetwork.Controllers
{
    public class ReviewController : Controller
    {
        private ApplicationContext _db;
        private static Review _currentReview = new Review();

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
                ViewData.Add("userId", CurrentUserSettings.CurrentUser.Id);
            }
            List<Review> viewModel = new List<Review>();
            Review currentReview = (await _db.Services.Reviews.GetFullReviewData()).FirstOrDefault(r => r.Id == int.Parse(review));
            currentReview.Comments.ToList().AddRange(_db.Comments.Include(c => c.Author).ThenInclude(a => a.Likes)
                .Where(comment => comment.ReviewId == currentReview.Id));
            viewModel.Add(currentReview);
            viewModel.AddRange((await _db.Services.Reviews.GetFullReviewData()).Where(r => r.CreationId == currentReview.CreationId && r.Id != currentReview.Id));
            ViewData.Model = viewModel;
            _currentReview = currentReview;
            return View();
        }

        public async Task<IActionResult> EditReview(string review)
        {
            Review currentReview = (await _db.Services.Reviews.GetFullReviewData()).FirstOrDefault(r => r.Id == int.Parse(review));
            ViewData.Model = currentReview;
            _currentReview = currentReview;
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
            await _db.Services.Tags.RemoveTags(_currentReview.Id);
            Review currentReview = await _db.Reviews.Include(review => review.Creation).FirstOrDefaultAsync(review => review.Id == _currentReview.Id);
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
            await _db.Services.Creations.CheckOldCreation(oldCreation);
            return RedirectToAction("MyPage", "MyPage");
        }

        public async Task<IActionResult> AddReviewToDbAsync(string reviewName, string creationName, string[] tags, string category, string description, string rate, IFormFile[] image)
        {
            if (description.Contains(TextModel.Context["typing description"])) description = "";
            string imgName = await ImageService.GetImageName(image);
            Creation creation = await _db.Services.Creations.GetCreation(creationName);
            Review review = new Review() { Name = reviewName, Creation = creation, Category = category, Date = DateTime.Now, Description = description, AuthorRate = int.Parse(rate), ImageUrl = imgName };
            CurrentUserSettings.CurrentUser.Reviews.ToList().Add(review);
            review.Author = CurrentUserSettings.CurrentUser;
            review.AuthorId = CurrentUserSettings.CurrentUser.Id;
            _db.Reviews.Attach(review);
            _db.SaveChanges();
            review.Tags.ToList().AddRange(await _db.Services.Tags.SetTagsToDb(tags, review));
            _db.SaveChanges();
            return RedirectToAction("MyPage", "MyPage");
        }

        public IActionResult NewComment(string commentContext)
        {
            Comment comment = new Comment { ReviewId = _currentReview.Id, Context = commentContext, Date = DateTime.Now, UserId = CurrentUserSettings.CurrentUser.Id, Author = CurrentUserSettings.CurrentUser, Review = _currentReview };
            _db.Comments.Attach(comment);
            _currentReview.Comments.ToList().Add(comment);
            _db.SaveChanges();
            return RedirectToAction("ReviewPage", "Review", new { review = _currentReview.Id.ToString() });
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
            if (likeOrDislike == 1)
            {
                Like like = new Like { ReviewId = _currentReview.Id, UserId = CurrentUserSettings.CurrentUser.Id, Author = CurrentUserSettings.CurrentUser, Review = _currentReview };
                _db.Likes.Attach(like);
                _currentReview.Likes.ToList().Add(like);
            }
            else
            {
                _db.Likes.Remove(await _db.Likes.FirstOrDefaultAsync(like => like.UserId == CurrentUserSettings.CurrentUser.Id && like.ReviewId == _currentReview.Id));
            }
            _db.SaveChanges();
        }

        public async Task ChangeCreationRate(int rate, string creationName)
        {
            Rate currentRate = await _db.Rates.FirstOrDefaultAsync(rate => rate.UserId == CurrentUserSettings.CurrentUser.Id && rate.ReviewId == _currentReview.Id);
            if (currentRate == null)
            {
                Rate newRate = new Rate { UserRate = rate, ReviewId = _currentReview.Id, UserId = CurrentUserSettings.CurrentUser.Id, CreationId = _currentReview.CreationId,
                Author = CurrentUserSettings.CurrentUser, Review = _currentReview, Creation = _currentReview.Creation};
                _db.Rates.Attach(newRate);
                _currentReview.Rates.ToList().Add(newRate);
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
    }
}
