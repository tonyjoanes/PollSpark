using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.DTOs;
using PollSpark.Models;

namespace PollSpark.Features.Polls.Queries;

public record GetPollByIdQuery(Guid Id) : IRequest<OneOf<PollDto, ErrorResponse>>;

public class GetPollByIdQueryHandler : IRequestHandler<GetPollByIdQuery, OneOf<PollDto, ErrorResponse>>
{
    private readonly PollSparkContext _context;

    public GetPollByIdQueryHandler(PollSparkContext context)
    {
        _context = context;
    }

    public async Task<OneOf<PollDto, ErrorResponse>> Handle(
        GetPollByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var poll = await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (poll == null)
        {
            return new ErrorResponse("Poll not found");
        }

        if (!poll.IsPublic && poll.ExpiresAt.HasValue && poll.ExpiresAt.Value < DateTime.UtcNow)
        {
            return new ErrorResponse("Poll has expired");
        }

        return new PollDto(
            poll.Id,
            poll.Title,
            poll.Description,
            poll.CreatedAt,
            poll.ExpiresAt,
            poll.IsPublic,
            poll.CreatedBy.Username,
            poll.Options.Select(o => new PollOptionDto(o.Id, o.Text)).ToList()
        );
    }
} 