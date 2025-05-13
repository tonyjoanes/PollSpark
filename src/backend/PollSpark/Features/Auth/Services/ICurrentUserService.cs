using PollSpark.Models;

namespace PollSpark.Features.Auth.Services;

public interface ICurrentUserService
{
    Task<User?> GetCurrentUser();
} 