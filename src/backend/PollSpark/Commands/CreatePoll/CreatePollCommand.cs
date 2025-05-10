using MediatR;
using OneOf;
using PollSpark.Models;

namespace PollSpark.Commands.CreatePoll;

public record CreatePollCommand(
    string Title,
    string Description,
    bool IsPublic,
    DateTime? ExpiresAt,
    List<string> Options
) : IRequest<OneOf<Poll, ValidationError>>;

public record ValidationError(string Message);
