namespace PollSpark.Models;

public class Poll
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsPublic { get; set; }
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
    public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
} 