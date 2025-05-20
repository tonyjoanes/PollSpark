using MediatR;
using OneOf;
using PollSpark.DTOs;
using PollSpark.Models;

namespace PollSpark.Features.Polls.Commands;

public record CreatePollCommand(
    string Title,
    string Description,
    bool IsPublic,
    DateTime? ExpiresAt,
    List<string> Options,
    List<Guid> CategoryIds
) : IRequest<OneOf<PollDto, ValidationError>>;
