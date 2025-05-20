using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.DTOs;
using PollSpark.Models;

namespace PollSpark.Handlers;

public static class CreatePoll
{
    public record Command(
        string Title,
        string Description,
        bool IsPublic,
        DateTime? ExpiresAt,
        List<string> Options,
        List<string> CategoryIds,
        string UserId
    ) : IRequest<OneOf<PollDto, Error>>;

    public class Handler : IRequestHandler<Command, OneOf<PollDto, Error>>
    {
        private readonly PollSparkContext _context;
        private readonly UserManager<User> _userManager;

        public Handler(PollSparkContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<OneOf<PollDto, Error>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return new Error("User not found");
            }

            if (request.Options.Count < 2 || request.Options.Count > 4)
            {
                return new Error("Poll must have between 2 and 4 options");
            }

            var categories = await _context.Categories
                .Where(c => request.CategoryIds.Contains(c.Id.ToString()))
                .ToListAsync(cancellationToken);

            // Extract hashtags from title and description
            var hashtagNames = ExtractHashtags.FromText(request.Title + " " + request.Description);
            
            // Get or create hashtags
            var hashtags = new List<Hashtag>();
            foreach (var hashtagName in hashtagNames)
            {
                var hashtag = await _context.Hashtags
                    .FirstOrDefaultAsync(h => h.Name == hashtagName, cancellationToken);

                if (hashtag == null)
                {
                    hashtag = new Hashtag
                    {
                        Id = Guid.NewGuid(),
                        Name = hashtagName
                    };
                    _context.Hashtags.Add(hashtag);
                }

                hashtags.Add(hashtag);
            }

            var poll = new Poll
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt,
                IsPublic = request.IsPublic,
                CreatedById = user.Id,
                Options = request.Options.Select(o => new PollOption
                {
                    Id = Guid.NewGuid(),
                    Text = o
                }).ToList(),
                Categories = categories,
                Hashtags = hashtags
            };

            _context.Polls.Add(poll);
            await _context.SaveChangesAsync(cancellationToken);

            return new PollDto(
                poll.Id,
                poll.Title,
                poll.Description,
                poll.CreatedAt,
                poll.ExpiresAt,
                poll.IsPublic,
                user.UserName,
                poll.Options.Select(o => new PollOptionDto(o.Id, o.Text)).ToList(),
                poll.Categories.Select(c => new CategoryDto(c.Id, c.Name, c.Description)).ToList(),
                poll.Hashtags.Select(h => new HashtagDto(h.Id, h.Name)).ToList()
            );
        }
    }
} 