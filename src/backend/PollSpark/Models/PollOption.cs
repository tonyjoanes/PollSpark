namespace PollSpark.Models;

public class PollOption
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid PollId { get; set; }
    public Poll Poll { get; set; } = null!;
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
} 