using Microsoft.EntityFrameworkCore;
using MyNetwork.Services;

namespace MyNetwork.Models
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options) 
        {
            Services = new DbServices(this);
        }

        public DbSet<User> AspNetUsers { get; set; } 
        public DbSet<Review> Reviews { get; set; } 
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Creation> Creations { get; set; }
        public DbSet<Rate> Rates { get; set; } 
        public DbSet<Tag> Tags { get; set; } 
        public DbSet<ReviewTag> ReviewTags { get; set; } 
        public DbSet<Like> Likes { get; set; }
        public DbSet<AdminData> AdminDatas { get; set; } 
        public DbServices Services { get; set; }
    }
}
