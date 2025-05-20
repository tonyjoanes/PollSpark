using Microsoft.EntityFrameworkCore;
using Moq;
using PollSpark.Data;
using PollSpark.Features.Auth.Services;
using PollSpark.Features.Polls.Commands;
using PollSpark.Models;
using Xunit;

namespace PollSpark.Tests.Polls;

public class DeletePollCommandTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly PollSparkContext _context;
    private readonly DeletePollCommandHandler _handler;

    public DeletePollCommandTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _context = new PollSparkContext(
            new DbContextOptionsBuilder<PollSparkContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options
        );
        _handler = new DeletePollCommandHandler(_context, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidDelete_ReturnsSuccess()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);

        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            Title = "Test Poll",
            Description = "Test Description",
            IsPublic = true,
            CreatedById = user.Id,
            CreatedAt = DateTime.UtcNow,
            Options = new List<PollOption>
            {
                new() { Id = Guid.NewGuid(), Text = "Option 1" },
                new() { Id = Guid.NewGuid(), Text = "Option 2" }
            }
        };
        await _context.Polls.AddAsync(poll);
        await _context.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.GetCurrentUser())
            .ReturnsAsync(user);

        var command = new DeletePollCommand(poll.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT0); // Success case
        var deletedPoll = await _context.Polls.FindAsync(poll.Id);
        Assert.Null(deletedPoll);
    }

    [Fact]
    public async Task Handle_UserNotAuthenticated_ReturnsError()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.GetCurrentUser())
            .ReturnsAsync((User?)null);

        var command = new DeletePollCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT1); // Error case
        var error = result.AsT1;
        Assert.Equal("User not authenticated", error.Message);
    }

    [Fact]
    public async Task Handle_PollNotFound_ReturnsError()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.GetCurrentUser())
            .ReturnsAsync(user);

        var command = new DeletePollCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT1); // Error case
        var error = result.AsT1;
        Assert.Equal("Poll not found", error.Message);
    }

    [Fact]
    public async Task Handle_NotPollOwner_ReturnsError()
    {
        // Arrange
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Username = "owner",
            Email = "owner@example.com",
            CreatedAt = DateTime.UtcNow
        };
        var otherUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "otheruser",
            Email = "other@example.com",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddRangeAsync(owner, otherUser);

        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            Title = "Test Poll",
            Description = "Test Description",
            IsPublic = true,
            CreatedById = owner.Id,
            CreatedAt = DateTime.UtcNow,
            Options = new List<PollOption>
            {
                new() { Id = Guid.NewGuid(), Text = "Option 1" },
                new() { Id = Guid.NewGuid(), Text = "Option 2" }
            }
        };
        await _context.Polls.AddAsync(poll);
        await _context.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.GetCurrentUser())
            .ReturnsAsync(otherUser);

        var command = new DeletePollCommand(poll.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT1); // Error case
        var error = result.AsT1;
        Assert.Equal("You don't have permission to delete this poll", error.Message);
    }
} 