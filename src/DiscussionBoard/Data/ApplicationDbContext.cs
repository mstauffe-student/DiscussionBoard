using DiscussionBoard.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {

        public DbSet<Post> Posts { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<DiscussionThread> DiscussionThreads { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Forum>(entity =>
            {
                   entity.HasMany(f => f.DiscussionThreads)
                          .WithOne(dt => dt.Forum)
                          .HasForeignKey(dt => dt.ForumId);

                   entity.ToTable("Forums");
                   entity.HasKey(f => f.Id);

                   entity.Property(f => f.Name)
                         .HasMaxLength(50)
                         .IsRequired();
            });

            //Discussion Thread:
            builder.Entity<DiscussionThread>(entity =>
            {
                entity.HasMany(dt => dt.Posts)
                    .WithOne(p => p.DiscussionThread)
                    .HasForeignKey(p => p.DiscussionThreadId);

                entity.ToTable("DiscussionThread");
                entity.HasKey(dt => dt.Id);

                entity.Property(dt => dt.Title)
                    .HasMaxLength(50)
                    .IsRequired();
            });

            //Post:
            builder.Entity<Post>(entity =>
            {
                entity.HasOne(p => p.ParentPost)
                    .WithMany(p => p.Replies)
                    .HasForeignKey(p => p.ParentPostId)
                    .IsRequired(false);

                entity.ToTable("Posts");
            });

            //Application User:
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasMany(p => p.Posts)
                    .WithOne(au => au.ApplicationUser)
                    .HasForeignKey(au => au.ApplicationUserId);

               entity.HasMany(dt => dt.DiscussionThreads)
                    .WithOne(au => au.ApplicationUser)
                    .HasForeignKey(au => au.ApplicationUserId);

               entity.ToTable("Users");
            });

        }
    }
}
