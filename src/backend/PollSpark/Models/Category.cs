namespace PollSpark.Models;

public record Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<Poll> Polls { get; set; } = new List<Poll>();
} 