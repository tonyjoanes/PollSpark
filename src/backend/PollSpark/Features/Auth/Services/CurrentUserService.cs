using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PollSpark.Data;
using PollSpark.Models;
using System.Security.Claims;

namespace PollSpark.Features.Auth.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly PollSparkContext _context;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, PollSparkContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public async Task<User?> GetCurrentUser()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }

        return await _context.Users.FindAsync(userId);
    }
}
