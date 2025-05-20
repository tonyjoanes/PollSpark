using Microsoft.AspNetCore.Identity;
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

    public static async Task SeedPolls(PollSparkContext context, UserManager<User> userManager)
    {
        // Clear existing polls and related data
        context.Votes.RemoveRange(context.Votes);
        context.PollOptions.RemoveRange(context.PollOptions);
        context.Polls.RemoveRange(context.Polls);
        context.Hashtags.RemoveRange(context.Hashtags);
        await context.SaveChangesAsync();

        // Get all categories
        var categories = await context.Categories.ToListAsync();
        var random = new Random();

        // Get or create a default user for polls
        var defaultUser = await userManager.FindByNameAsync("admin");

        if (defaultUser == null)
        {
            defaultUser = new User
            {
                UserName = "admin",
                Email = "admin@pollspark.com",
                CreatedAt = DateTime.UtcNow,
            };
            var result = await userManager.CreateAsync(defaultUser, "Admin123!");
            if (!result.Succeeded)
            {
                throw new Exception(
                    "Failed to create default user: "
                        + string.Join(", ", result.Errors.Select(e => e.Description))
                );
            }
        }

        // Create hashtags
        var hashtags = new Dictionary<string, Hashtag>();
        var allHashtags = new[]
        {
            "coding",
            "programming",
            "tech",
            "learning",
            "gadgets",
            "innovation",
            "future",
            "movies",
            "entertainment",
            "cinema",
            "film",
            "streaming",
            "tvshows",
            "basketball",
            "sports",
            "NBA",
            "GOAT",
            "food",
            "pizza",
            "dining",
            "taste",
            "travel",
            "vacation",
            "wanderlust",
            "adventure",
            "business",
            "career",
            "leadership",
            "success",
            "education",
            "study",
            "knowledge",
            "fitness",
            "health",
            "wellness",
            "workout",
        };

        foreach (var hashtagName in allHashtags)
        {
            var hashtag = new Hashtag { Id = Guid.NewGuid(), Name = hashtagName };
            hashtags[hashtagName] = hashtag;
        }

        await context.Hashtags.AddRangeAsync(hashtags.Values);
        await context.SaveChangesAsync();

        var polls = new List<Poll>
        {
            // Technology Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Best Programming Language for Beginners #coding #programming",
                Description =
                    "Which programming language would you recommend for someone just starting their coding journey? #tech #learning",
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
                Hashtags = new List<Hashtag>
                {
                    hashtags["coding"],
                    hashtags["programming"],
                    hashtags["tech"],
                    hashtags["learning"],
                },
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Most Anticipated Tech Gadget of 2024 #tech #gadgets",
                Description =
                    "Which upcoming tech gadget are you most excited about? #innovation #future",
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
                Hashtags = new List<Hashtag>
                {
                    hashtags["tech"],
                    hashtags["gadgets"],
                    hashtags["innovation"],
                    hashtags["future"],
                },
            },
            // Entertainment Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Best Movie Genre #movies #entertainment",
                Description = "What's your favorite movie genre? #cinema #film",
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
                Hashtags = new List<Hashtag>
                {
                    hashtags["movies"],
                    hashtags["entertainment"],
                    hashtags["cinema"],
                    hashtags["film"],
                },
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Best Streaming Platform #streaming #entertainment",
                Description = "Which streaming service do you prefer? #movies #tvshows",
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
                Hashtags = new List<Hashtag>
                {
                    hashtags["streaming"],
                    hashtags["entertainment"],
                    hashtags["movies"],
                    hashtags["tvshows"],
                },
            },
            // Sports Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Greatest Basketball Player of All Time #basketball #sports",
                Description =
                    "Who do you think is the greatest basketball player in history? #NBA #GOAT",
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
                Hashtags = new List<Hashtag>
                {
                    hashtags["basketball"],
                    hashtags["sports"],
                    hashtags["NBA"],
                    hashtags["GOAT"],
                },
            },
            // Food & Dining Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Favorite Pizza Topping #food #pizza",
                Description = "What's your go-to pizza topping? #dining #taste",
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
                Hashtags = new List<Hashtag>
                {
                    hashtags["food"],
                    hashtags["pizza"],
                    hashtags["dining"],
                    hashtags["taste"],
                },
            },
            // Travel Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Dream Vacation Destination #travel #vacation",
                Description = "Where would you most like to travel? #wanderlust #adventure",
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
                Hashtags = new List<Hashtag>
                {
                    hashtags["travel"],
                    hashtags["vacation"],
                    hashtags["wanderlust"],
                    hashtags["adventure"],
                },
            },
            // Business Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Most Important Business Skill #business #career",
                Description =
                    "What do you think is the most crucial skill for business success? #leadership #success",
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
                Hashtags = new List<Hashtag>
                {
                    hashtags["business"],
                    hashtags["career"],
                    hashtags["leadership"],
                    hashtags["success"],
                },
            },
            // Education Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Most Effective Learning Method #education #learning",
                Description = "What's your preferred way of learning new things? #study #knowledge",
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
                Hashtags = new List<Hashtag>
                {
                    hashtags["education"],
                    hashtags["learning"],
                    hashtags["study"],
                    hashtags["knowledge"],
                },
            },
            // Health & Wellness Polls
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Favorite Exercise Type #fitness #health",
                Description = "What's your preferred way to stay fit? #wellness #workout",
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
                Hashtags = new List<Hashtag>
                {
                    hashtags["fitness"],
                    hashtags["health"],
                    hashtags["wellness"],
                    hashtags["workout"],
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
                            IpAddress = ip,
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
