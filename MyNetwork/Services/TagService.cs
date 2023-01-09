using Microsoft.EntityFrameworkCore;
using MyNetwork.Models;

namespace MyNetwork.Services
{
    public class TagService
    {
        private ApplicationContext _db;

        public TagService(ApplicationContext db)
        {
            _db = db;
        }

        public List<string> SelectPopularTags()
        {
            return _db.Tags.Include(t => t.ReviewTags).OrderByDescending(tag => tag.ReviewTags.Count()).Take(_db.Tags.Count() > 5 ? 5 : _db.Tags.Count())
            .Select(tag => tag.Name).ToList();
        }

        public async Task<List<ReviewTag>> SetTagsToDb(string[] tags, Review review)
        {
            List<ReviewTag> reviewTags = new List<ReviewTag>();
            foreach (var tag in tags.Skip(1).Distinct())
            {
                Tag tagFromDb = await _db.Tags.FirstOrDefaultAsync(tagFromDb => tagFromDb.Name == tag);
                if (tagFromDb == null)
                {
                    tagFromDb = new Tag() { Name = tag };
                    _db.Tags.Attach(tagFromDb);
                }
                reviewTags.Add(new ReviewTag { ReviewId = review.Id, TagId = tagFromDb.Id, Review = review, Tag = tagFromDb });
                _db.ReviewTags.Attach(reviewTags.Last());
            }
            return reviewTags;
        }

        public void RemoveTags()
        {
            foreach (var tag in _db.Tags.Include(t => t.ReviewTags).ToList())
            {
                if (tag.ReviewTags.Count() == 0) _db.Tags.Remove(tag);
                _db.SaveChanges();
            }
            _db.SaveChanges();
        }
    }
}
