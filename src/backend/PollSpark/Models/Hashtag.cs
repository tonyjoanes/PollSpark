namespace PollSpark.Models;

public record Hashtag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Poll> Polls { get; set; } = new List<Poll>();
} 