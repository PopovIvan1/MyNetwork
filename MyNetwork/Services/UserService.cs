using Microsoft.EntityFrameworkCore;
using MyNetwork.Models;

namespace MyNetwork.Services
{
    public class UserService
    {
        private ApplicationContext _db;

        public UserService(ApplicationContext db)
        {
            _db = db;
        }

        public async Task<User> GetFullUserData(User user)
        {
            user.Reviews.ToList().AddRange((await _db.Services.Reviews.GetFullReviewData()).Where(review => review.AuthorId == user.Id));
            return user;
        }

        public async Task RemoveUser(User user)
        {
            await _db.Comments.Where(comment => comment.UserId == user.Id).ForEachAsync(comment => _db.Comments.Remove(comment));
            await _db.Likes.Where(like => like.UserId == user.Id).ForEachAsync(like => _db.Likes.Remove(like));
            await _db.Rates.Where(rate => rate.UserId == user.Id).ForEachAsync(rate => _db.Rates.Remove(rate));
            var reviews = await _db.Reviews.Where(review => review.AuthorId == user.Id).ToListAsync(); 
            foreach(var review in reviews)
            {
                await _db.Services.Reviews.RemoveReview(review);
            }
            _db.AspNetUsers.Remove(_db.AspNetUsers.FirstOrDefault(u => u.Id == user.Id));
            _db.SaveChanges();
        }
    }
}
