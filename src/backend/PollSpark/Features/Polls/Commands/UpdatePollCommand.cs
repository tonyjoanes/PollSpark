using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.DTOs;
using PollSpark.Features.Auth.Services;
using PollSpark.Models;

namespace PollSpark.Features.Polls.Commands;

public record UpdatePollCommand(
    Guid Id,
    string Title,
    string Description,
    bool IsPublic,
    DateTime? ExpiresAt,
    List<string> Options
) : IRequest<OneOf<PollDto, ValidationError>>;

public class UpdatePollCommandHandler : IRequestHandler<UpdatePollCommand, OneOf<PollDto, ValidationError>>
{
    private readonly PollSparkContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePollCommandHandler(
        PollSparkContext context,
        ICurrentUserService currentUserService
    )
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<OneOf<PollDto, ValidationError>> Handle(
        UpdatePollCommand request,
        CancellationToken cancellationToken
    )
    {
        var currentUser = await _currentUserService.GetCurrentUser();
        if (currentUser == null)
        {
            return new ValidationError("User not authenticated");
        }

        var poll = await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (poll == null)
        {
            return new ValidationError("Poll not found");
        }

        if (poll.CreatedById != currentUser.Id)
        {
            return new ValidationError("You don't have permission to update this poll");
        }

        poll.Title = request.Title;
        poll.Description = request.Description;
        poll.IsPublic = request.IsPublic;
        poll.ExpiresAt = request.ExpiresAt;

        // Remove existing options
        _context.PollOptions.RemoveRange(poll.Options);

        // Add new options
        poll.Options = request.Options.Select(o => new PollOption
        {
            Id = Guid.NewGuid(),
            Text = o,
            PollId = poll.Id
        }).ToList();

        await _context.SaveChangesAsync(cancellationToken);

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