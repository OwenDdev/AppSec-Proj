using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SecureApp.API.Models;

namespace SecureApp.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<AppFile> Files { get; set; }
    }
}
