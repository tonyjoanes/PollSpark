using Microsoft.EntityFrameworkCore;
using Moq;
using PollSpark.Data;
using PollSpark.Features.Auth.Commands;
using PollSpark.Features.Auth.Services;
using Xunit;

namespace PollSpark.Tests.Auth;

public class LoginCommandTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly PollSparkContext _context;
    private readonly LoginCommandHandler _handler;

    public LoginCommandTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _context = new PollSparkContext(
            new DbContextOptionsBuilder<PollSparkContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options
        );
        _handler = new LoginCommandHandler(_context, _authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "password123");
        _authServiceMock
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _authServiceMock
            .Setup(x => x.GenerateJwtToken(It.IsAny<Models.User>()))
            .Returns("jwt_token");

        // Add test user
        await _context.Users.AddAsync(
            new Models.User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                CreatedAt = DateTime.UtcNow,
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT0); // Success case
        var response = result.AsT0;
        Assert.Equal("jwt_token", response.Token);
        Assert.Equal("testuser", response.Username);
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ReturnsError()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "wrong_password");
        _authServiceMock
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        // Add test user
        await _context.Users.AddAsync(
            new Models.User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                CreatedAt = DateTime.UtcNow,
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT1); // Error case
        var error = result.AsT1;
        Assert.Equal("Invalid email or password", error.Message);
    }
}
