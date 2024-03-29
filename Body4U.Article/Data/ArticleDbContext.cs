﻿namespace Body4U.Article.Data
{
    using Body4U.Common.Messages;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using System;

    public class ArticleDbContext : DbContext
    {
        public ArticleDbContext(DbContextOptions<ArticleDbContext> options)
            : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }

        public DbSet<ArticleImageData> ArticleImageDatas { get; set; }

        public DbSet<Trainer> Trainers { get; set; }

        public DbSet<TrainerImageData> TrainerImagesDatas { get; set; }

        public DbSet<TrainerVideo> TrainerVideos { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region Article
            builder
                .Entity<Article>()
                .HasOne(x => x.Trainer)
                .WithMany(y => y.Articles)
                .HasForeignKey(x => x.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Entity<Article>()
                .HasOne(x => x.ArticleImageData)
                .WithOne(y => y.Article)
                .HasForeignKey<ArticleImageData>(y => y.ArticleId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region TrainerImageData
            builder
                .Entity<TrainerImageData>()
                .HasOne(x => x.Trainer)
                .WithMany(y => y.TrainerImagesDatas)
                .HasForeignKey(x => x.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region TrainerVideo
            builder
                .Entity<TrainerVideo>()
                .HasOne(x => x.Trainer)
                .WithMany(y => y.TrainerVideos)
                .HasForeignKey(x => x.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Comment
            builder
                .Entity<Comment>()
                .HasOne(x => x.Article)
                .WithMany(y => y.Comments)
                .HasForeignKey(x => x.ArticleId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Service
            builder
                .Entity<Service>()
                .HasOne(x => x.Trainer)
                .WithMany(y => y.Services)
                .HasForeignKey(x => x.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Entity<Service>()
                .Property(x => x.Price)
                .HasColumnType("decimal(18, 6)");
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
