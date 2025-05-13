namespace PollSpark.Models;

public record Vote
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public Poll Poll { get; set; } = null!;
    public Guid OptionId { get; set; }
    public PollOption Option { get; set; } = null!;
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? IpAddress { get; set; }
}
