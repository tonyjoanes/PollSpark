using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PollSpark.Models;

namespace PollSpark.Data;

public class PollSparkContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public PollSparkContext(DbContextOptions<PollSparkContext> options)
        : base(options) { }

    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PollOption> PollOptions => Set<PollOption>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Hashtag> Hashtags => Set<Hashtag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<Poll>()
            .HasOne(p => p.CreatedBy)
            .WithMany(u => u.CreatedPolls)
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<PollOption>()
            .HasOne(po => po.Poll)
            .WithMany(p => p.Options)
            .HasForeignKey(po => po.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<Vote>()
            .HasOne(v => v.Poll)
            .WithMany(p => p.Votes)
            .HasForeignKey(v => v.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<Vote>()
            .HasOne(v => v.Option)
            .WithMany(o => o.Votes)
            .HasForeignKey(v => v.OptionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add unique constraint for votes
        modelBuilder
            .Entity<Vote>()
            .HasIndex(v => new { v.PollId, v.UserId })
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");

        modelBuilder
            .Entity<Vote>()
            .HasIndex(v => new { v.PollId, v.IpAddress })
            .IsUnique()
            .HasFilter("[IpAddress] IS NOT NULL");

        // Configure many-to-many relationship between Poll and Category
        modelBuilder
            .Entity<Poll>()
            .HasMany(p => p.Categories)
            .WithMany(c => c.Polls)
            .UsingEntity(j => j.ToTable("PollCategories"));

        // Configure many-to-many relationship between Poll and Hashtag
        modelBuilder
            .Entity<Poll>()
            .HasMany(p => p.Hashtags)
            .WithMany(h => h.Polls)
            .UsingEntity(j => j.ToTable("PollHashtags"));

        // Add unique constraint for category names
        modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();

        // Add unique constraint for hashtag names
        modelBuilder.Entity<Hashtag>().HasIndex(h => h.Name).IsUnique();
    }
}
