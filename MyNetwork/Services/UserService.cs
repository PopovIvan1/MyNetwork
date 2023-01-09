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
            User fullUser = await _db.AspNetUsers.Include(u => u.Comments).Include(u => u.Likes)
                .Include(u => u.Rates).Include(u => u.Reviews).FirstOrDefaultAsync(u => u.Id == user.Id);
            if (fullUser != null)
            {
                var reviews = await _db.Reviews.Where(review => review.AuthorId == user.Id).ToListAsync();
                foreach (var review in reviews)
                {
                    await _db.Services.Reviews.RemoveReview(review);
                    _db.SaveChanges();
                }
                _db.AspNetUsers.Remove(fullUser);
                _db.Services.Tags.RemoveTags();
                _db.SaveChanges();
            }
        }
    }
}
