﻿using Microsoft.EntityFrameworkCore;
using System.Linq;
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
                : sortOrder == "popular" ? userReviews.OrderByDescending(review => review.Likes).ToList() : userReviews;
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await AspNetUsers.FirstOrDefaultAsync(user => user.Id == userId);
        }

        public List<string> SelectReviewTags(int reviewId)
        {
            return Tags.Where(tag => 
            ReviewTags.Where(reviewTag => reviewTag.ReviewId == reviewId).Select(reviewTag => reviewTag.TagId)
            .Contains(tag.Id)).Select(tag => tag.Name).ToList();
        }

        public List<Comment> SelectReviewComments(int reviewId)
        {
            return Comments.Where(comment => comment.ReviewId == reviewId).OrderBy(comment => comment.Date).ToList();
        }

        public List<Review> SelectReviewsWithSettings()
        {
            var reviewsWithCurrentCategory = ReviewSettings.Category == "all" ? Reviews : Reviews.Where(review => review.Category == ReviewSettings.Category);
            var resultReviews = ReviewSettings.SearchType == "best views" ?
                reviewsWithCurrentCategory.OrderByDescending(review => review.Likes).ToList() :
                ReviewSettings.SearchType == "last views" ?
                reviewsWithCurrentCategory.OrderByDescending(review => review.Date).ToList() :
                ReviewSettings.Tags.Count == 0 ? reviewsWithCurrentCategory.ToList() :
                getReviewsByTags(reviewsWithCurrentCategory.ToList());
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
        }

        private List<Review> getReviewsByTags(List<Review> reviewsWithCurrentCategory)
        {
            List<Review> resultReviews = new List<Review>();
            foreach (var review in reviewsWithCurrentCategory)
            {
                var reviewTags = SelectReviewTags(review.Id);
                if (reviewTags.Count != 0 && ReviewSettings.Tags.All(tag => reviewTags.Contains(HttpUtility.UrlDecode(tag)))) resultReviews.Add(review);
            }
            return resultReviews;
        }
    }
}
