namespace Body4U.EmailSender.Data
{
    using Body4U.Common.Messages;
    using Microsoft.EntityFrameworkCore;
    using System;

    public class EmailSenderDbContext : DbContext
    {
        public EmailSenderDbContext(DbContextOptions<EmailSenderDbContext> options)
            : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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
