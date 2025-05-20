using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.DTOs;
using PollSpark.Features.Auth.Services;
using PollSpark.Models;

namespace PollSpark.Features.Polls.Commands;

public class CreatePollCommandHandler
    : IRequestHandler<CreatePollCommand, OneOf<PollDto, ValidationError>>
{
    private readonly PollSparkContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreatePollCommandHandler(
        PollSparkContext context,
        ICurrentUserService currentUserService
    )
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<OneOf<PollDto, ValidationError>> Handle(
        CreatePollCommand request,
        CancellationToken cancellationToken
    )
    {
        var currentUser = await _currentUserService.GetCurrentUser();
        if (currentUser == null)
        {
            return new ValidationError("User not authenticated");
        }

        // Validate categories exist
        var categories = await _context
            .Categories.Where(c => request.CategoryIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (categories.Count != request.CategoryIds.Count)
        {
            return new ValidationError("One or more categories do not exist");
        }

        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            IsPublic = request.IsPublic,
            ExpiresAt = request.ExpiresAt,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
            Options = request
                .Options.Select(o => new PollOption { Id = Guid.NewGuid(), Text = o })
                .ToList(),
            Categories = categories,
        };

        _context.Polls.Add(poll);
        await _context.SaveChangesAsync(cancellationToken);

        // Load the created poll with its options, categories and created by user
        var createdPoll = await _context
            .Polls.Include(p => p.Options)
            .Include(p => p.Categories)
            .Include(p => p.Hashtags)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == poll.Id, cancellationToken);

        if (createdPoll == null)
        {
            return new ValidationError("Failed to create poll");
        }

        return new PollDto(
            createdPoll.Id,
            createdPoll.Title,
            createdPoll.Description,
            createdPoll.CreatedAt,
            createdPoll.ExpiresAt,
            createdPoll.IsPublic,
            createdPoll.CreatedBy.UserName,
            createdPoll.Options.Select(o => new PollOptionDto(o.Id, o.Text)).ToList(),
            [.. createdPoll.Categories.Select(c => new CategoryDto(c.Id, c.Name, c.Description))],
            createdPoll.Hashtags.Select(h => new HashtagDto(h.Id, h.Name)).ToList()
        );
    }
}
