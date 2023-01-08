using Microsoft.EntityFrameworkCore;
using MyNetwork.Models;

namespace MyNetwork.Services
{
    public class CreationService
    {
        private ApplicationContext _db;

        public CreationService(ApplicationContext db)
        {
            _db = db;
        }

        public async Task<Creation> GetCreation(string creationName)
        {
            Creation creation = await _db.Creations.AsNoTracking().FirstOrDefaultAsync(creation => creation.Name == creationName);
            if (creation == null)
            {
                creation = new Creation { Name = creationName };
            }
            return creation;
        }

        public async Task CheckOldCreation(int oldCreationId)
        {
            Creation oldCreation = await _db.Creations.AsNoTracking().Include(c => c.Reviews).FirstOrDefaultAsync(c => c.Id == oldCreationId);
            if (oldCreation.Reviews.Count() == 0)
            {
                _db.Creations.Remove(oldCreation);
                _db.SaveChanges();
            }
        }
    }
}
