namespace PollSpark.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<Poll> CreatedPolls { get; set; } = new List<Poll>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
} 