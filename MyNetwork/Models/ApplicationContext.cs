using Microsoft.EntityFrameworkCore;
using System.Web;

namespace MyNetwork.Models
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options) { }

        public DbSet<User> AspNetUsers { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Rate> Rates { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<ReviewTag> ReviewTags { get; set; } = null!;
        public DbSet<Like> Likes { get; set; } = null!;
        public DbSet<AdminData> AdminDatas { get; set; } = null!;

        public async Task<User> FindUserByNameAsync(string username)
        {
            return await AspNetUsers.FirstOrDefaultAsync(user => user.UserName == username);
        }

        public List<string> SelectPopularTags()
        {
            return Tags.OrderByDescending(tag => tag.ReviewsCount).Take(Tags.Count() > 5 ? 5 : Tags.Count())
                .Select(tag => tag.Name).ToList();
        }

        public async Task<List<Review>> SelectUserReviews(string username, string category, string sortOrder)
        {
            string userId = (await FindUserByNameAsync(username)).Id;
            var userReviews = category == "all" ? Reviews.Where(review => review.AuthorId == userId).ToList()
                : Reviews.Where(review => review.AuthorId == userId && review.Category == category).ToList();
            return sortOrder == "date" ? userReviews.OrderByDescending(review => review.Date).ToList()
                : sortOrder == "popular" ? userReviews.OrderByDescending(review => review.UsersReviewRate).ToList() : userReviews;
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await AspNetUsers.FirstOrDefaultAsync(user => user.Id == userId);
        }

        public List<string> SelectReviewTags(int reviewId)
        {
            return Tags.Where(tag => 
            ReviewTags.Where(reviewTag => reviewTag.ReviewId == reviewId).Select(reviewTag => reviewTag.TagId)
            .Contains(tag.Id)).ToList().Select(tag => tag.Name).ToList();
        }

        public List<Comment> SelectReviewComments(int reviewId)
        {
            return Comments.Where(comment => comment.ReviewId == reviewId).OrderBy(comment => comment.Date).ToList();
        }

        public List<Review> SelectReviewsWithSettings(string category, string searchType, string tags)
        {
            List<string> tagsToList = tags.Split(' ').ToList();
            var reviewsWithCurrentCategory = category == "all" ? Reviews : Reviews.Where(review => review.Category == category);
            var resultReviews = searchType == "best views" ?
                reviewsWithCurrentCategory.OrderByDescending(review => review.UsersReviewRate).ToList() :
                searchType == "last views" ?
                reviewsWithCurrentCategory.OrderByDescending(review => review.Date).ToList() :
                tagsToList[0] == "" ? reviewsWithCurrentCategory.ToList() :
                getReviewsByTags(reviewsWithCurrentCategory.ToList(), tagsToList);
            return resultReviews.Take(resultReviews.Count > 10 ? 10 : resultReviews.Count).ToList();
        }

        public async Task SetTagsToDb(string[] tags, int reviewId)
        {
            foreach (var tag in tags.Skip(1).Distinct())
            {
                Tag tagFromDb = await Tags.FirstOrDefaultAsync(tagFromDb => tagFromDb.Name == tag);
                if (tagFromDb != null) tagFromDb.ReviewsCount++;
                else Tags.Add(new Tag() { Name = tag, ReviewsCount = 0 });
                SaveChanges();
                ReviewTags.Add(new ReviewTag() { ReviewId = reviewId, TagId = (await Tags.FirstOrDefaultAsync(tagFromDb => tagFromDb.Name == tag)).Id });
            }
            SaveChanges();
        }

        public async Task RemoveUser(User user)
        {
            await Comments.Where(comment => comment.UserName == user.UserName).ForEachAsync(comment => Comments.Remove(comment));
            await Likes.Where(like => like.UserId == user.Id).ForEachAsync(like => Likes.Remove(like));
            await Rates.Where(rate => rate.UserId == user.Id).ForEachAsync(rate => Rates.Remove(rate));
            SaveChanges();
            await removeUserReviews(user.Id);
            AspNetUsers.Remove(user);
            SaveChanges();
        }

        public void UpdateRates(int reviewId, string newCreationName, string oldCreationName)
        {
            Rates.Where(rate => rate.ReviewId == reviewId).ToList().ForEach(rate => rate.CreationName = newCreationName);
            SaveChanges();
            Rate newRate = new Rate() { CreationName = oldCreationName, ReviewId = reviewId, UserId = CurrentUserSettings.CurrentUser.Id };
            Rates.Add(newRate);
            SaveChanges();
            Rates.Remove(newRate);
            SaveChanges();
        }

        public async Task RemoveTags(int reviewId)
        {
            var currentReviewTags = ReviewTags.Where(tag => tag.ReviewId == reviewId).ToList();
            foreach (var reviewTag in currentReviewTags)
            {
                Tag tag = await Tags.FirstOrDefaultAsync(tag => tag.Id == reviewTag.TagId);
                ReviewTags.Remove(reviewTag);
                if (tag.ReviewsCount == 0) Tags.Remove(tag);
                else tag.ReviewsCount--;
                SaveChanges();
            }
            SaveChanges();
        }

        private List<Review> getReviewsByTags(List<Review> reviewsWithCurrentCategory, List<string> tags)
        {
            List<Review> resultReviews = new List<Review>();
            foreach (var review in reviewsWithCurrentCategory)
            {
                var reviewTags = SelectReviewTags(review.Id);
                if (reviewTags.Count != 0 && tags.All(tag => reviewTags.Contains(HttpUtility.UrlDecode(tag)))) resultReviews.Add(review);
            }
            return resultReviews;
        }

        private async Task removeUserReviews(string userId)
        {
            foreach (var review in Reviews.Where(review => review.AuthorId == userId).ToList())
            {
                await RemoveTags(review.Id);
                await ImageService.Remove(review.ImageUrl);
                await Rates.Where(rate => rate.ReviewId == review.Id).ForEachAsync(rate => Rates.Remove(rate));
            }
            SaveChanges();
        }
    }
}
