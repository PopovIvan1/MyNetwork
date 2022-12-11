using Microsoft.EntityFrameworkCore;

namespace MyNetwork.Models
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options) { }

        public DbSet<User> AspNetUsers { get; set; } = null!;

        public async Task<bool> IsAdminAsync(string username)
        {
            return (await AspNetUsers.FirstOrDefaultAsync(user => user.UserName == username))
                .IsAdmin == "No        " ? false : true;
        }
    }
}
