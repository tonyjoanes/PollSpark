using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.DTOs;
using PollSpark.Features.Polls.Commands;
using PollSpark.Models;

namespace PollSpark.Features.Polls.Queries;

public record GetPollsQuery(int Page, int PageSize, Guid? CategoryId = null)
    : IRequest<OneOf<PaginatedResponse<PollDto>, ValidationError>>;

public class GetPollsQueryHandler
    : IRequestHandler<GetPollsQuery, OneOf<PaginatedResponse<PollDto>, ValidationError>>
{
    private readonly PollSparkContext _context;

    public GetPollsQueryHandler(PollSparkContext context)
    {
        _context = context;
    }

    public async Task<OneOf<PaginatedResponse<PollDto>, ValidationError>> Handle(
        GetPollsQuery request,
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

        var query = _context
            .Polls.Include(p => p.Options)
            .Include(p => p.Categories)
            .Include(p => p.Hashtags)
            .Include(p => p.CreatedBy)
            .Where(p => p.IsPublic || p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow);

        // Apply category filter if specified
        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.Categories.Any(c => c.Id == request.CategoryId.Value));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var polls = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var pollDtos = polls
            .Select(p => new { Poll = p, Username = p.CreatedBy?.UserName })
            .Where(x => x.Username != null)
            .Select(x => new PollDto(
                x.Poll.Id,
                x.Poll.Title,
                x.Poll.Description,
                x.Poll.CreatedAt,
                x.Poll.ExpiresAt,
                x.Poll.IsPublic,
                x.Username!,
                x.Poll.Options.Select(o => new PollOptionDto(o.Id, o.Text)).ToList(),
                x.Poll.Categories.Select(c => new CategoryDto(c.Id, c.Name, c.Description))
                    .ToList(),
                x.Poll.Hashtags.Select(h => new HashtagDto(h.Id, h.Name)).ToList()
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
