namespace PollSpark.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string Description
);

public record HashtagDto(
    Guid Id,
    string Name
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
    List<CategoryDto> Categories,
    List<HashtagDto> Hashtags
);

public record PollOptionDto(
    Guid Id,
    string Text
);
