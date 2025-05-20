using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.Features.Auth.Services;
using PollSpark.Models;
using AuthResponse = PollSpark.DTOs.AuthResponse;

namespace PollSpark.Features.Auth.Commands;

public record RegisterCommand(string Username, string Email, string Password)
    : IRequest<OneOf<AuthResponse, AuthError>>;

public class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, OneOf<AuthResponse, AuthError>>
{
    private readonly PollSparkContext _context;
    private readonly IAuthService _authService;

    public RegisterCommandHandler(PollSparkContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<OneOf<AuthResponse, AuthError>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken
    )
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return new AuthError("Email already registered");
        }

        if (await _context.Users.AnyAsync(u => u.UserName == request.Username))
        {
            return new AuthError("Username already taken");
        }

        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            PasswordHash = _authService.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var token = _authService.GenerateJwtToken(user);
        return new AuthResponse(token, user.UserName);
    }
}
