using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.DTOs;
using PollSpark.Models;

namespace PollSpark.Handlers;

public static class GetPollsByHashtag
{
    public record Query(string Hashtag, int Page, int PageSize)
        : IRequest<OneOf<PaginatedResponse<PollDto>, Error>>;

    public class Handler : IRequestHandler<Query, OneOf<PaginatedResponse<PollDto>, Error>>
    {
        private readonly PollSparkContext _context;

        public Handler(PollSparkContext context)
        {
            _context = context;
        }

        public async Task<OneOf<PaginatedResponse<PollDto>, Error>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var hashtag = await _context.Hashtags.FirstOrDefaultAsync(
                h => h.Name.ToLower() == request.Hashtag.ToLower(),
                cancellationToken
            );

            if (hashtag == null)
            {
                return new Error("Hashtag not found");
            }

            var query = _context
                .Polls.Include(p => p.Options)
                .Include(p => p.Categories)
                .Include(p => p.Hashtags)
                .Include(p => p.CreatedBy)
                .Where(p => p.Hashtags.Any(h => h.Name.ToLower() == request.Hashtag.ToLower()));

            var totalItems = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

            var polls = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new PollDto(
                    p.Id,
                    p.Title,
                    p.Description,
                    p.CreatedAt,
                    p.ExpiresAt,
                    p.IsPublic,
                    p.CreatedBy.UserName!,
                    p.Options.Select(o => new PollOptionDto(o.Id, o.Text)).ToList(),
                    p.Categories.Select(c => new CategoryDto(c.Id, c.Name, c.Description)).ToList(),
                    p.Hashtags.Select(h => new HashtagDto(h.Id, h.Name)).ToList()
                ))
                .ToListAsync(cancellationToken);

            return new PaginatedResponse<PollDto>(
                polls,
                request.Page,
                request.PageSize,
                totalItems,
                totalPages
            );
        }
    }
}

public static class GetPopularHashtags
{
    public record Query : IRequest<OneOf<List<HashtagDto>, Error>>;

    public class Handler : IRequestHandler<Query, OneOf<List<HashtagDto>, Error>>
    {
        private readonly PollSparkContext _context;

        public Handler(PollSparkContext context)
        {
            _context = context;
        }

        public async Task<OneOf<List<HashtagDto>, Error>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var popularHashtags = await _context
                .Hashtags.Select(h => new
                {
                    h.Id,
                    h.Name,
                    PollCount = h.Polls.Count,
                })
                .OrderByDescending(h => h.PollCount)
                .Take(10)
                .Select(h => new HashtagDto(h.Id, h.Name))
                .ToListAsync(cancellationToken);

            return popularHashtags;
        }
    }
}

public static class ExtractHashtags
{
    public static List<string> FromText(string text)
    {
        var hashtags = new List<string>();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            if (word.StartsWith("#") && word.Length > 1)
            {
                var hashtag = word[1..].ToLower();
                if (!string.IsNullOrWhiteSpace(hashtag) && !hashtags.Contains(hashtag))
                {
                    hashtags.Add(hashtag);
                }
            }
        }

        return hashtags;
    }
}
