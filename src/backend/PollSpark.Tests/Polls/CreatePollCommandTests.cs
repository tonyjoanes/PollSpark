using Microsoft.EntityFrameworkCore;
using Moq;
using PollSpark.Data;
using PollSpark.Features.Auth.Services;
using PollSpark.Features.Polls.Commands;
using PollSpark.Models;
using Xunit;

namespace PollSpark.Tests.Polls;

public class CreatePollCommandTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly PollSparkContext _context;
    private readonly CreatePollCommandHandler _handler;

    public CreatePollCommandTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _context = new PollSparkContext(
            new DbContextOptionsBuilder<PollSparkContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options
        );
        _handler = new CreatePollCommandHandler(_context, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidPollData_ReturnsSuccess()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);

        var command = new CreatePollCommand(
            "Test Poll",
            "This is a test poll",
            true,
            DateTime.UtcNow.AddDays(7),
            new List<string> { "Option 1", "Option 2", "Option 3" }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT0); // Success case
        var pollDto = result.AsT0;
        Assert.Equal("Test Poll", pollDto.Title);
        Assert.Equal("This is a test poll", pollDto.Description);
        Assert.True(pollDto.IsPublic);
        Assert.Equal(3, pollDto.Options.Count);
        Assert.Equal("testuser", pollDto.CreatedByUsername);

        var loadedPoll = await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == pollDto.Id);
        Assert.NotNull(loadedPoll);

        var updatedPoll = await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == pollDto.Id);
        Assert.NotNull(updatedPoll);
        Assert.Equal("Test Poll", updatedPoll.Title);
        Assert.Equal("This is a test poll", updatedPoll.Description);
        Assert.True(updatedPoll.IsPublic);
        Assert.Equal(3, updatedPoll.Options.Count);
    }

    [Fact]
    public async Task Handle_UserNotAuthenticated_ReturnsError()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync((User?)null);

        var command = new CreatePollCommand(
            "Test Poll",
            "This is a test poll",
            true,
            DateTime.UtcNow.AddDays(7),
            new List<string> { "Option 1", "Option 2" }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT1); // Error case
        var error = result.AsT1;
        Assert.Equal("User not authenticated", error.Message);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync((User?)null);

        var command = new CreatePollCommand(
            "Test Poll",
            "This is a test poll",
            true,
            DateTime.UtcNow.AddDays(7),
            new List<string> { "Option 1", "Option 2" }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT1); // Error case
        var error = result.AsT1;
        Assert.Equal("User not authenticated", error.Message);
    }

    [Fact]
    public async Task Handle_PollWithExpiration_StoresExpirationDate()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);

        var expirationDate = DateTime.UtcNow.AddDays(7);
        var command = new CreatePollCommand(
            "Test Poll",
            "This is a test poll",
            true,
            expirationDate,
            new List<string> { "Option 1", "Option 2" }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT0); // Success case
        var pollDto = result.AsT0;
        Assert.Equal(expirationDate, pollDto.ExpiresAt);

        var loadedPoll = await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == pollDto.Id);
        Assert.NotNull(loadedPoll);

        var updatedPoll = await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == pollDto.Id);
        Assert.NotNull(updatedPoll);
        Assert.Equal("Test Poll", updatedPoll.Title);
        Assert.Equal("This is a test poll", updatedPoll.Description);
        Assert.True(updatedPoll.IsPublic);
        Assert.Equal(2, updatedPoll.Options.Count);
    }

    [Fact]
    public async Task Handle_PollWithoutExpiration_StoresNullExpirationDate()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);

        var command = new CreatePollCommand(
            "Test Poll",
            "This is a test poll",
            true,
            null,
            new List<string> { "Option 1", "Option 2" }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT0); // Success case
        var pollDto = result.AsT0;
        Assert.Null(pollDto.ExpiresAt);

        var loadedPoll = await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == pollDto.Id);
        Assert.NotNull(loadedPoll);

        var updatedPoll = await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == pollDto.Id);
        Assert.NotNull(updatedPoll);
        Assert.Equal("Test Poll", updatedPoll.Title);
        Assert.Equal("This is a test poll", updatedPoll.Description);
        Assert.True(updatedPoll.IsPublic);
        Assert.Equal(2, updatedPoll.Options.Count);
    }
}
