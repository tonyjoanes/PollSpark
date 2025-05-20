using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.Models;

namespace PollSpark.Features.Polls.Queries;

public record GetUserVoteQuery(Guid PollId) : IRequest<OneOf<Guid?, ErrorResponse>>;

public class GetUserVoteQueryHandler : IRequestHandler<GetUserVoteQuery, OneOf<Guid?, ErrorResponse>>
{
    private readonly PollSparkContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetUserVoteQueryHandler(PollSparkContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OneOf<Guid?, ErrorResponse>> Handle(
        GetUserVoteQuery request,
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

        // Check if the poll exists
        var poll = await _context.Polls.FirstOrDefaultAsync(p => p.Id == request.PollId, cancellationToken);
        if (poll == null)
        {
            return new ErrorResponse("Poll not found");
        }

        // Get the user's vote
        var vote = await _context.Votes.FirstOrDefaultAsync(
            v =>
                v.PollId == request.PollId
                && (userId.HasValue ? v.UserId == userId : v.IpAddress == ipAddress),
            cancellationToken
        );

        return vote?.OptionId;
    }
} 