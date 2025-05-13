using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.Models;

namespace PollSpark.Features.Polls.Commands;

public record VoteCommand(Guid PollId, Guid OptionId) : IRequest<OneOf<Success, ValidationError>>;

public class VoteCommandHandler : IRequestHandler<VoteCommand, OneOf<Success, ValidationError>>
{
    private readonly PollSparkContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VoteCommandHandler(PollSparkContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OneOf<Success, ValidationError>> Handle(
        VoteCommand request,
        CancellationToken cancellationToken
    )
    {
        // Get the user ID from the JWT token if available
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        );
        Guid? userId = null;
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        // Get the IP address
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        // Check if the poll exists and is not expired
        var poll = await _context
            .Polls.Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Id == request.PollId, cancellationToken);

        if (poll == null)
        {
            return new ValidationError("Poll not found");
        }

        if (poll.ExpiresAt.HasValue && poll.ExpiresAt.Value < DateTime.UtcNow)
        {
            return new ValidationError("Poll has expired");
        }

        // Check if the option exists and belongs to the poll
        if (!poll.Options.Any(o => o.Id == request.OptionId))
        {
            return new ValidationError("Invalid option for this poll");
        }

        // Check if user has already voted on this poll
        var existingVote = await _context.Votes.FirstOrDefaultAsync(
            v =>
                v.PollId == request.PollId
                && (userId.HasValue ? v.UserId == userId : v.IpAddress == ipAddress),
            cancellationToken
        );

        if (existingVote != null)
        {
            return new ValidationError("You have already voted on this poll");
        }

        // Create the vote
        var vote = new Vote
        {
            PollId = request.PollId,
            OptionId = request.OptionId,
            UserId = userId,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Votes.Add(vote);
        await _context.SaveChangesAsync(cancellationToken);

        return new Success();
    }
}
