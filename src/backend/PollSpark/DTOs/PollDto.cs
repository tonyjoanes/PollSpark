namespace PollSpark.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string Description
);

public record PollDto(
    Guid Id,
    string Title,
    string Description,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    bool IsPublic,
    string CreatedByUsername,
    List<PollOptionDto> Options,
    List<CategoryDto> Categories
);

public record PollOptionDto(
    Guid Id,
    string Text
);
