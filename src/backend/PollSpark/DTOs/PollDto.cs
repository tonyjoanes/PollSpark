namespace PollSpark.DTOs;

public record PollDto(
    Guid Id,
    string Title,
    string Description,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    bool IsPublic,
    string CreatedByUsername,
    List<PollOptionDto> Options,
    int TotalVotes
);

public record PollOptionDto(Guid Id, string Text, int VoteCount);
