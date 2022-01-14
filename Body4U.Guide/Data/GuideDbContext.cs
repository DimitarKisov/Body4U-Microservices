namespace Body4U.Guide.Data
{
    using Body4U.Guide.Data.Models;
    using Microsoft.EntityFrameworkCore;

    public class GuideDbContext : DbContext
    {
        public GuideDbContext(DbContextOptions<GuideDbContext> options)
            : base(options)
        {
        }

        public DbSet<Exercise> Exercises { get; set; }
    }
}
