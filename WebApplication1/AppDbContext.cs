using Microsoft.EntityFrameworkCore;
namespace WebApplication1.data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // This creates a "Products" table in the database
        public DbSet<Product> Products { get; set; }
    }
}