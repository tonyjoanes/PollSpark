using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.Features.Auth.Commands;

namespace PollSpark.Features.Auth.Queries;

public record GetUserProfileQuery : IRequest<OneOf<UserProfileDto, AuthError>>;

public record UserProfileDto(
    Guid Id,
    string Username,
    string Email,
    DateTime CreatedAt,
    int CreatedPollsCount
);

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, OneOf<UserProfileDto, AuthError>>
{
    private readonly PollSparkContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetUserProfileQueryHandler(PollSparkContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OneOf<UserProfileDto, AuthError>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return new AuthError("User not authenticated");
        }

        var user = await _context.Users
            .Include(u => u.CreatedPolls)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return new AuthError("User not found");
        }

        return new UserProfileDto(
            user.Id,
            user.Username,
            user.Email,
            user.CreatedAt,
            user.CreatedPolls.Count
        );
    }
} 