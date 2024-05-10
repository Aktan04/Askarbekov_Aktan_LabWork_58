using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Instagram.Models;

public class InstaContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public InstaContext(DbContextOptions<InstaContext> options) : base(options){}
    
    public DbSet<User> Users { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Publication> Publications { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<Comment>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Like>()
            .HasKey(l => l.Id);

        modelBuilder.Entity<Publication>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<UserSubscription>()
            .HasKey(us => us.Id);

        modelBuilder.Entity<UserSubscription>()
            .HasOne(us => us.Subscriber)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(us => us.SubscriberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserSubscription>()
            .HasOne(us => us.TargetUser)
            .WithMany(u => u.Followers)
            .HasForeignKey(us => us.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}