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
    List<string> Options
) : IRequest<OneOf<PollDto, ValidationError>>;
