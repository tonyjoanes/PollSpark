using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.DTOs;
using PollSpark.Models;
using PollSpark.Services.Auth;

namespace PollSpark.Commands.Auth;

public record LoginCommand(string Email, string Password) : IRequest<OneOf<AuthResponse, AuthError>>;

public record AuthError(string Message);

public class LoginCommandHandler : IRequestHandler<LoginCommand, OneOf<AuthResponse, AuthError>>
{
    private readonly PollSparkContext _context;
    private readonly IAuthService _authService;

    public LoginCommandHandler(PollSparkContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<OneOf<AuthResponse, AuthError>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if (user == null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return new AuthError("Invalid email or password");
        }

        var token = _authService.GenerateJwtToken(user);
        return new AuthResponse(token, user.Username);
    }
} 