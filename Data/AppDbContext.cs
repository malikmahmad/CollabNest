using Microsoft.EntityFrameworkCore;
using CollabNest.Models;

namespace CollabNest.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<CollabRequest> CollabRequests { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } // ← ADDED

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CollabRequest>()
                .HasOne(r => r.Project)
                .WithMany(p => p.CollabRequests)
                .HasForeignKey(r => r.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CollabRequest>()
                .HasOne(r => r.Sender)
                .WithMany(u => u.SentRequests)
                .HasForeignKey(r => r.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            // ← ADDED
            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(t => t.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}