using Microsoft.EntityFrameworkCore;
using Moq;
using PollSpark.Data;
using PollSpark.Features.Auth.Commands;
using PollSpark.Features.Auth.Services;
using Xunit;

namespace PollSpark.Tests.Auth;

public class RegisterCommandTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly PollSparkContext _context;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _context = new PollSparkContext(
            new DbContextOptionsBuilder<PollSparkContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options
        );
        _handler = new RegisterCommandHandler(_context, _authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var command = new RegisterCommand("testuser", "test@example.com", "password123");
        _authServiceMock.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hashed_password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT0); // Success case
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.NotNull(user);
        Assert.Equal("testuser", user.Username);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsError()
    {
        // Arrange
        var command = new RegisterCommand("testuser", "test@example.com", "password123");
        _authServiceMock.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hashed_password");

        // Add existing user
        await _context.Users.AddAsync(
            new Models.User
            {
                Username = "existing",
                Email = "test@example.com",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT1); // Error case
        var error = result.AsT1;
        Assert.Equal("Email already registered", error.Message);
    }
}
