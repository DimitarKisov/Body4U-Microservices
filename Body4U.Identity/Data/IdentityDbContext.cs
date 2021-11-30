﻿namespace Body4U.Identity.Data
{
    using Body4U.Identity.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class IdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }

        public DbSet<Trainer> Trainers { get; set; }

        public DbSet<TrainerImage> TrainerImages { get; set; }

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
            #endregion

            #region Trainer
            builder.Entity<Trainer>()
                .HasOne(x => x.ApplicationUser)
                .WithOne(y => y.Trainer)
                .HasForeignKey<Trainer>(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Trainer>()
                .HasMany(x => x.TrainerImages)
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
