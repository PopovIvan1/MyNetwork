using MyNetwork.Models;

namespace MyNetwork.Services
{
    public class DbServices
    {
        public ReviewService Reviews { get; set; }
        public TagService Tags { get; set; }
        public UserService Users { get; set; }
        public RateService Rates { get; set; }
        public CreationService Creations { get; set; }

        public DbServices(ApplicationContext db) 
        {
            Reviews = new ReviewService(db);
            Tags = new TagService(db);
            Users = new UserService(db);
            Rates = new RateService(db);
            Creations = new CreationService(db);
        }

    }
}
