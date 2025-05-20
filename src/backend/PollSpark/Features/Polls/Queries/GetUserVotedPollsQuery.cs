using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.DTOs;
using PollSpark.Features.Polls.Commands;
using PollSpark.Models;

namespace PollSpark.Features.Polls.Queries;

public record GetUserVotedPollsQuery(int Page, int PageSize)
    : IRequest<OneOf<PaginatedResponse<PollDto>, ValidationError>>;

public class GetUserVotedPollsQueryHandler
    : IRequestHandler<GetUserVotedPollsQuery, OneOf<PaginatedResponse<PollDto>, ValidationError>>
{
    private readonly PollSparkContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetUserVotedPollsQueryHandler(
        PollSparkContext context,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OneOf<PaginatedResponse<PollDto>, ValidationError>> Handle(
        GetUserVotedPollsQuery request,
        CancellationToken cancellationToken
    )
    {
        if (request.Page < 1)
        {
            return new ValidationError("Page number must be greater than 0");
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return new ValidationError("Page size must be between 1 and 100");
        }

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

        // Get polls that the user has voted on
        var query = _context
            .Polls.Include(p => p.Options)
            .Include(p => p.CreatedBy)
            .Include(p => p.Votes)
            .Where(p =>
                p.Votes.Any(v =>
                    (userId.HasValue && v.UserId == userId)
                    || (!userId.HasValue && v.IpAddress == ipAddress)
                )
            );

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var polls = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var pollDtos = polls
            .Select(p => new PollDto(
                p.Id,
                p.Title,
                p.Description,
                p.CreatedAt,
                p.ExpiresAt,
                p.IsPublic,
                p.CreatedBy.Username,
                p.Options.Select(o => new PollOptionDto(o.Id, o.Text)).ToList()
            ))
            .ToList();

        return new PaginatedResponse<PollDto>(
            pollDtos,
            request.Page,
            request.PageSize,
            totalItems,
            totalPages
        );
    }
}
