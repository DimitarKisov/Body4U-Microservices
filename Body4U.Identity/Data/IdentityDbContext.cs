namespace Body4U.Identity.Data
{
    using Body4U.Common.Messages;
    using Body4U.Identity.Data.Models.Favourites;
    using Body4U.Identity.Data.Models.Identity;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using System;

    public class IdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserImageData> UserImageDatas { get; set; }

        public DbSet<Favourite> Favourites { get; set; }

        public DbSet<Message> Messages { get; set; }

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

            #region Favourite
            builder.Entity<Favourite>()
                .HasKey(x => new { x.ArticleId, x.ApplicationUserId });

            builder.Entity<Favourite>()
                .HasOne(x => x.ApplicationUser)
                .WithMany(y => y.Favourites)
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Message
            builder.Entity<Message>()
                .Property(m => m.Type)
                .HasConversion(
                    t => t.AssemblyQualifiedName,
                    t => Type.GetType(t));
            #endregion
        }
    }
}
