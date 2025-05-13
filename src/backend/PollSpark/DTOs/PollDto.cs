namespace PollSpark.DTOs;

public record PollDto(
    Guid Id,
    string Title,
    string Description,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    bool IsPublic,
    string CreatedByUsername,
    List<PollOptionDto> Options
);

public record PollOptionDto(
    Guid Id,
    string Text
);
