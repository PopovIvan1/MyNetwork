using Microsoft.EntityFrameworkCore;
using MyNetwork.Models;
using System.Web;

namespace MyNetwork.Services
{
    public class ReviewService
    {
        private ApplicationContext _db;

        public ReviewService(ApplicationContext db)
        {
            _db = db;
        }

        public async Task<List<Review>> GetFullReviewData()
        {
            return await _db.Reviews.Include(r => r.Likes).Include(r => r.Creation)
                                .ThenInclude(c => c.Rates).Include(r => r.Author).ThenInclude(a => a.Likes)
                                .Include(r => r.Tags).ThenInclude(t => t.Tag).ToListAsync();
        }

        public async Task<List<Review>> SelectReviewsWithSettings(string category, string searchType, string tags)
        {
            if (string.IsNullOrEmpty(category)) category = "all";
            if (string.IsNullOrEmpty(searchType)) searchType = "last views";
            if (string.IsNullOrEmpty(tags)) tags = "";
            List<string> tagsToList = tags.Split(' ').ToList();
            List<Review> fullReviews = await GetFullReviewData();
            var reviewsWithCurrentCategory = category == "all" ? fullReviews
                : fullReviews.Where(review => review.Category == category);
            var resultReviews = searchType == "best views" ?
                reviewsWithCurrentCategory.OrderByDescending(review => review.Rates.Count() != 0 ? review.Rates.Average(r => r.UserRate) : 0).ToList() :
                searchType == "last views" ?
                reviewsWithCurrentCategory.OrderByDescending(review => review.Date).ToList() :
                tagsToList[0] == "" ? reviewsWithCurrentCategory.ToList() :
                await getReviewsByTags(reviewsWithCurrentCategory.ToList(), tagsToList);
            return resultReviews.Take(resultReviews.Count > 10 ? 10 : resultReviews.Count).ToList();
        }

        public async Task RemoveReview(Review currentReview)
        {
            if (!string.IsNullOrEmpty(currentReview.ImageUrl)) await ImageService.Remove(currentReview.ImageUrl);
            Review fullReview = await _db.Reviews.Include(r => r.Likes)
                .Include(r => r.Rates).Include(r => r.Comments)
                .FirstOrDefaultAsync(review => review.Id == currentReview.Id);
            if (fullReview != null)
            {
                _db.Reviews.Remove(fullReview);
                _db.SaveChanges();
                await _db.Services.Creations.CheckOldCreation(currentReview.CreationId);
            }
        }

        private async Task<List<Review>> getReviewsByTags(List<Review> reviewsWithCurrentCategory, List<string> tags)
        {
            List<Review> resultReviews = new List<Review>();
            foreach (var review in reviewsWithCurrentCategory)
            {
                List<string> tagNames = review.Tags.Select(t => t.Tag.Name).ToList();
                if (review.Tags.Count() != 0 && tags.All(tag => tagNames.Contains(HttpUtility.UrlDecode(tag)))) resultReviews.Add(review);
            }
            return resultReviews;
        }
    }
}
