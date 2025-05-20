using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.DTOs;
using PollSpark.Models;

namespace PollSpark.Features.Polls.Queries;

public record GetPollResultsQuery(Guid PollId) : IRequest<OneOf<PollResultsDto, ErrorResponse>>;

public record PollResultsDto(
    Guid PollId,
    List<PollOptionResultDto> Results,
    int TotalVotes
);

public record PollOptionResultDto(
    Guid OptionId,
    string OptionText,
    int Votes,
    double Percentage
);

public class GetPollResultsQueryHandler : IRequestHandler<GetPollResultsQuery, OneOf<PollResultsDto, ErrorResponse>>
{
    private readonly PollSparkContext _context;

    public GetPollResultsQueryHandler(PollSparkContext context)
    {
        _context = context;
    }

    public async Task<OneOf<PollResultsDto, ErrorResponse>> Handle(
        GetPollResultsQuery request,
        CancellationToken cancellationToken
    )
    {
        // Get the poll with its options and votes
        var poll = await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.Votes)
            .FirstOrDefaultAsync(p => p.Id == request.PollId, cancellationToken);

        if (poll == null)
        {
            return new ErrorResponse("Poll not found");
        }

        var totalVotes = poll.Votes.Count;
        var results = poll.Options.Select(option =>
        {
            var votes = poll.Votes.Count(v => v.OptionId == option.Id);
            var percentage = totalVotes > 0 ? (double)votes / totalVotes * 100 : 0;
            return new PollOptionResultDto(
                option.Id,
                option.Text,
                votes,
                Math.Round(percentage, 1)
            );
        }).ToList();

        return new PollResultsDto(poll.Id, results, totalVotes);
    }
} 