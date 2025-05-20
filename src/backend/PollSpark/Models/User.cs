using Microsoft.AspNetCore.Identity;

namespace PollSpark.Models;

public class User : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; }
    public ICollection<Poll> CreatedPolls { get; set; } = new List<Poll>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
