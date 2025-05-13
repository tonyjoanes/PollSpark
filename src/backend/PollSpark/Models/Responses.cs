namespace PollSpark.Models;

public record UserProfileResponse(string Username, string Email);

public record ErrorResponse(string Message);
