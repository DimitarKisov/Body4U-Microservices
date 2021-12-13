namespace Body4U.Identity.Data
{
    using Body4U.Identity.Data.Models.Identity;
    using Body4U.Identity.Data.Models.Trainer;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class IdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserImageData> UserImageDatas { get; set; }

        public DbSet<Trainer> Trainers { get; set; }

        public DbSet<TrainerImageData> TrainerImagesDatas { get; set; }

        public DbSet<TrainerVideo> TrainerVideos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region Table names
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "IdentityUsers");
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "IdentityRoles");
            });
            #endregion

            #region ApplicationUser
            builder.Entity<ApplicationUser>()
                .HasMany(e => e.Claims)
                .WithOne()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(e => e.Logins)
                .WithOne()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(e => e.Roles)
                .WithOne()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasOne(x => x.UserImageData)
                .WithOne(y => y.ApplicationUser)
                .HasForeignKey<UserImageData>(y => y.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Trainer
            builder.Entity<Trainer>()
                .HasOne(x => x.ApplicationUser)
                .WithOne(y => y.Trainer)
                .HasForeignKey<Trainer>(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Trainer>()
                .HasMany(x => x.TrainerImagesDatas)
                .WithOne(y => y.Trainer)
                .HasForeignKey(y => y.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Trainer>()
                .HasMany(x => x.TrainerVideos)
                .WithOne(y => y.Trainer)
                .HasForeignKey(y => y.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}
