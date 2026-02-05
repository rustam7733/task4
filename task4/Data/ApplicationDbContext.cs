using Microsoft.EntityFrameworkCore;
using task4.Models;

namespace task4.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<ApplicationUser> Users { get; set; }
    }
}
