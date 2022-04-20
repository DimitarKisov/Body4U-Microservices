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

        public DbSet<Food> Foods { get; set; }

        public DbSet<OtherFoodValues> OtherFoodValues { get; set; }

        public DbSet<Supplement> Supplements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder
                .Entity<OtherFoodValues>()
                .HasOne(x => x.Food)
                .WithOne(y => y.OtherValues)
                .HasForeignKey<OtherFoodValues>(y => y.FoodId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
