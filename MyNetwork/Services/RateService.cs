using MyNetwork.Models;

namespace MyNetwork.Services
{
    public class RateService
    {
        private ApplicationContext _db;

        public RateService(ApplicationContext db) 
        {
            _db = db;
        }

        public void UpdateRates(int reviewId, int creationId)
        {
            _db.Rates.Where(rate => rate.ReviewId == reviewId).ToList().ForEach(rate => rate.CreationId = creationId);
            _db.SaveChanges();
        }
    }
}
