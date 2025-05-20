using Microsoft.EntityFrameworkCore;
using PollSpark.Models;

namespace PollSpark.Data;

public static class DbSeeder
{
    public static async Task SeedCategories(PollSparkContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            return; // Categories already seeded
        }

        var categories = new List<Category>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Technology",
                Description = "Polls about technology, software, hardware, and digital trends",
                CreatedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Entertainment",
                Description = "Polls about movies, music, TV shows, and other entertainment topics",
                CreatedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Sports",
                Description = "Polls about various sports, teams, and athletic events",
                CreatedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Food & Dining",
                Description = "Polls about restaurants, recipes, and culinary preferences",
                CreatedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Travel",
                Description = "Polls about destinations, travel experiences, and vacation planning",
                CreatedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Business",
                Description = "Polls about business trends, entrepreneurship, and workplace topics",
                CreatedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Education",
                Description = "Polls about learning, teaching, and educational topics",
                CreatedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Health & Wellness",
                Description = "Polls about fitness, health, and wellness topics",
                CreatedAt = DateTime.UtcNow,
            },
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    public static async Task SeedPolls(PollSparkContext context)
    {
        // Clear existing polls and related data
        context.Votes.RemoveRange(context.Votes);
        context.PollOptions.RemoveRange(context.PollOptions);
        context.Polls.RemoveRange(context.Polls);
        await context.SaveChangesAsync();

        // Get all categories
        var categories = await context.Categories.ToListAsync();
        var random = new Random();

        // Get or create a default user for polls
        var defaultUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        
        if (defaultUser == null)
        {
            defaultUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = "admin@pollspark.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                CreatedAt = DateTime.UtcNow,
            };
            context.Users.Add(defaultUser);
            await context.SaveChangesAsync();
        }

        var polls = new List<Poll>
        {
            // Technology Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Best Programming Language for Beginners",
                Description =
                    "Which programming language would you recommend for someone just starting their coding journey?",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                ExpiresAt = DateTime.UtcNow.AddDays(25),
                IsPublic = true,
                CreatedById = defaultUser.Id,
                Options = new List<PollOption>
                {
                    new() { Id = Guid.NewGuid(), Text = "Python" },
                    new() { Id = Guid.NewGuid(), Text = "JavaScript" },
                    new() { Id = Guid.NewGuid(), Text = "Java" },
                    new() { Id = Guid.NewGuid(), Text = "C#" },
                },
                Categories = new List<Category> { categories.First(c => c.Name == "Technology") },
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Most Anticipated Tech Gadget of 2024",
                Description = "Which upcoming tech gadget are you most excited about?",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ExpiresAt = DateTime.UtcNow.AddDays(27),
                IsPublic = true,
                CreatedById = defaultUser.Id,
                Options = new List<PollOption>
                {
                    new() { Id = Guid.NewGuid(), Text = "Apple Vision Pro" },
                    new() { Id = Guid.NewGuid(), Text = "Samsung Galaxy S24" },
                    new() { Id = Guid.NewGuid(), Text = "Tesla Cybertruck" },
                    new() { Id = Guid.NewGuid(), Text = "Meta Quest 3" },
                },
                Categories = new List<Category> { categories.First(c => c.Name == "Technology") },
            },
            // Entertainment Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Best Movie Genre",
                Description = "What's your favorite movie genre?",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                ExpiresAt = DateTime.UtcNow.AddDays(23),
                IsPublic = true,
                CreatedById = defaultUser.Id,
                Options = new List<PollOption>
                {
                    new() { Id = Guid.NewGuid(), Text = "Action" },
                    new() { Id = Guid.NewGuid(), Text = "Comedy" },
                    new() { Id = Guid.NewGuid(), Text = "Drama" },
                    new() { Id = Guid.NewGuid(), Text = "Sci-Fi" },
                    new() { Id = Guid.NewGuid(), Text = "Horror" },
                },
                Categories = new List<Category>
                {
                    categories.First(c => c.Name == "Entertainment"),
                },
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Best Streaming Platform",
                Description = "Which streaming service do you prefer?",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ExpiresAt = DateTime.UtcNow.AddDays(28),
                IsPublic = true,
                CreatedById = defaultUser.Id,
                Options = new List<PollOption>
                {
                    new() { Id = Guid.NewGuid(), Text = "Netflix" },
                    new() { Id = Guid.NewGuid(), Text = "Disney+" },
                    new() { Id = Guid.NewGuid(), Text = "Amazon Prime" },
                    new() { Id = Guid.NewGuid(), Text = "HBO Max" },
                    new() { Id = Guid.NewGuid(), Text = "Hulu" },
                },
                Categories = new List<Category>
                {
                    categories.First(c => c.Name == "Entertainment"),
                },
            },
            // Sports Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Greatest Basketball Player of All Time",
                Description = "Who do you think is the greatest basketball player in history?",
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                ExpiresAt = DateTime.UtcNow.AddDays(26),
                IsPublic = true,
                CreatedById = defaultUser.Id,
                Options = new List<PollOption>
                {
                    new() { Id = Guid.NewGuid(), Text = "Michael Jordan" },
                    new() { Id = Guid.NewGuid(), Text = "LeBron James" },
                    new() { Id = Guid.NewGuid(), Text = "Kareem Abdul-Jabbar" },
                    new() { Id = Guid.NewGuid(), Text = "Magic Johnson" },
                },
                Categories = new List<Category> { categories.First(c => c.Name == "Sports") },
            },
            // Food & Dining Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Favorite Pizza Topping",
                Description = "What's your go-to pizza topping?",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ExpiresAt = DateTime.UtcNow.AddDays(29),
                IsPublic = true,
                CreatedById = defaultUser.Id,
                Options = new List<PollOption>
                {
                    new() { Id = Guid.NewGuid(), Text = "Pepperoni" },
                    new() { Id = Guid.NewGuid(), Text = "Mushrooms" },
                    new() { Id = Guid.NewGuid(), Text = "Extra Cheese" },
                    new() { Id = Guid.NewGuid(), Text = "Pineapple" },
                    new() { Id = Guid.NewGuid(), Text = "Sausage" },
                },
                Categories = new List<Category>
                {
                    categories.First(c => c.Name == "Food & Dining"),
                },
            },
            // Travel Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Dream Vacation Destination",
                Description = "Where would you most like to travel?",
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                ExpiresAt = DateTime.UtcNow.AddDays(24),
                IsPublic = true,
                CreatedById = defaultUser.Id,
                Options = new List<PollOption>
                {
                    new() { Id = Guid.NewGuid(), Text = "Bali, Indonesia" },
                    new() { Id = Guid.NewGuid(), Text = "Paris, France" },
                    new() { Id = Guid.NewGuid(), Text = "Tokyo, Japan" },
                    new() { Id = Guid.NewGuid(), Text = "New York, USA" },
                    new() { Id = Guid.NewGuid(), Text = "Sydney, Australia" },
                },
                Categories = new List<Category> { categories.First(c => c.Name == "Travel") },
            },
            // Business Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Most Important Business Skill",
                Description = "What do you think is the most crucial skill for business success?",
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                ExpiresAt = DateTime.UtcNow.AddDays(22),
                IsPublic = true,
                CreatedById = defaultUser.Id,
                Options = new List<PollOption>
                {
                    new() { Id = Guid.NewGuid(), Text = "Leadership" },
                    new() { Id = Guid.NewGuid(), Text = "Communication" },
                    new() { Id = Guid.NewGuid(), Text = "Financial Management" },
                    new() { Id = Guid.NewGuid(), Text = "Strategic Planning" },
                },
                Categories = new List<Category> { categories.First(c => c.Name == "Business") },
            },
            // Education Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Most Effective Learning Method",
                Description = "What's your preferred way of learning new things?",
                CreatedAt = DateTime.UtcNow.AddDays(-9),
                ExpiresAt = DateTime.UtcNow.AddDays(21),
                IsPublic = true,
                CreatedById = defaultUser.Id,
                Options = new List<PollOption>
                {
                    new() { Id = Guid.NewGuid(), Text = "Online Courses" },
                    new() { Id = Guid.NewGuid(), Text = "Reading Books" },
                    new() { Id = Guid.NewGuid(), Text = "Hands-on Practice" },
                    new() { Id = Guid.NewGuid(), Text = "Group Study" },
                },
                Categories = new List<Category> { categories.First(c => c.Name == "Education") },
            },
            // Health & Wellness Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Favorite Exercise Type",
                Description = "What's your preferred way to stay fit?",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ExpiresAt = DateTime.UtcNow.AddDays(20),
                IsPublic = true,
                CreatedById = defaultUser.Id,
                Options = new List<PollOption>
                {
                    new() { Id = Guid.NewGuid(), Text = "Running" },
                    new() { Id = Guid.NewGuid(), Text = "Yoga" },
                    new() { Id = Guid.NewGuid(), Text = "Weight Training" },
                    new() { Id = Guid.NewGuid(), Text = "Swimming" },
                    new() { Id = Guid.NewGuid(), Text = "Cycling" },
                },
                Categories = new List<Category>
                {
                    categories.First(c => c.Name == "Health & Wellness"),
                },
            },
        };

        // Add some random votes to each poll
        foreach (var poll in polls)
        {
            var votes = new List<Vote>();
            var usedIps = new HashSet<string>();
            foreach (var option in poll.Options)
            {
                // Add random number of votes (0-50) for each option
                var numVotes = random.Next(0, 51);
                for (int i = 0; i < numVotes; i++)
                {
                    string ip;
                    int attempts = 0;
                    do
                    {
                        ip = $"192.168.1.{random.Next(1, 255)}";
                        attempts++;
                    } while (usedIps.Contains(ip) && attempts < 300);
                    if (usedIps.Contains(ip))
                        break; // Can't generate more unique IPs
                    usedIps.Add(ip);
                    votes.Add(
                        new Vote
                        {
                            Id = Guid.NewGuid(),
                            PollId = poll.Id,
                            OptionId = option.Id,
                            CreatedAt = DateTime.UtcNow.AddHours(-random.Next(1, 168)), // Random time in the last week
                            IpAddress = ip
                        }
                    );
                }
            }
            poll.Votes = votes;
        }

        await context.Polls.AddRangeAsync(polls);
        await context.SaveChangesAsync();
    }
}
