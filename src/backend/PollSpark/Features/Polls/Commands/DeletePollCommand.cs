using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.Features.Auth.Services;
using PollSpark.Models;

namespace PollSpark.Features.Polls.Commands;

public record DeletePollCommand(Guid Id) : IRequest<OneOf<Success, ValidationError>>;

public class DeletePollCommandHandler
    : IRequestHandler<DeletePollCommand, OneOf<Success, ValidationError>>
{
    private readonly PollSparkContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeletePollCommandHandler(
        PollSparkContext context,
        ICurrentUserService currentUserService
    )
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<OneOf<Success, ValidationError>> Handle(
        DeletePollCommand request,
        CancellationToken cancellationToken
    )
    {
        var currentUser = await _currentUserService.GetCurrentUser();
        if (currentUser == null)
        {
            return new ValidationError("User not authenticated");
        }

        var poll = await _context.Polls.FirstOrDefaultAsync(
            p => p.Id == request.Id,
            cancellationToken
        );

        if (poll == null)
        {
            return new ValidationError("Poll not found");
        }

        if (poll.CreatedById != currentUser.Id)
        {
            return new ValidationError("You don't have permission to delete this poll");
        }

        _context.Polls.Remove(poll);
        await _context.SaveChangesAsync(cancellationToken);

        return new Success();
    }
}
