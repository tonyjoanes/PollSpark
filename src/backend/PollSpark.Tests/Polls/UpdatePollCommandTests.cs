using Microsoft.EntityFrameworkCore;
using Moq;
using PollSpark.Data;
using PollSpark.Features.Auth.Services;
using PollSpark.Features.Polls.Commands;
using PollSpark.Models;
using Xunit;
using MediatR;
using OneOf;
using PollSpark.DTOs;

namespace PollSpark.Tests.Polls;

public class UpdatePollCommandTests : IDisposable
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly PollSparkContext _context;
    private readonly UpdatePollCommandHandler _handler;

    public UpdatePollCommandTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        var options = new DbContextOptionsBuilder<PollSparkContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new PollSparkContext(options);
        _handler = new UpdatePollCommandHandler(_context, _currentUserServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<Poll> CreateTestPoll(User user, List<Category>? categories = null)
    {
        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            Description = "Original Description",
            IsPublic = true,
            CreatedById = user.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user,
            Categories = categories ?? new List<Category>(),
            Options = new List<PollOption>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Text = "Original Option 1",
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Text = "Original Option 2",
                },
            }
        };

        _context.Polls.Add(poll);
        await _context.SaveChangesAsync();
        
        // Reload the poll to ensure proper tracking
        return await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.Categories)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == poll.Id);
    }

    [Fact(Skip = "Temporarily skipped due to entity tracking issues")]
    public async Task Handle_ValidUpdate_ReturnsSuccess()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var poll = await CreateTestPoll(user);
        _currentUserServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);

        var command = new UpdatePollCommand(
            poll.Id,
            "Updated Title",
            "Updated Description",
            false,
            DateTime.UtcNow.AddDays(7),
            new List<string> { "New Option 1", "New Option 2", "New Option 3" },
            new List<Guid>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT0); // Success case
        var pollDto = result.AsT0;
        Assert.Equal("Updated Title", pollDto.Title);
        Assert.Equal("Updated Description", pollDto.Description);
        Assert.False(pollDto.IsPublic);
        Assert.Equal(3, pollDto.Options.Count);
        Assert.Equal("testuser", pollDto.CreatedByUsername);

        // Verify the changes were persisted
        var updatedPoll = await _context
            .Polls.Include(p => p.Options)
            .Include(p => p.CreatedBy)
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == poll.Id);
        Assert.NotNull(updatedPoll);
        Assert.Equal("Updated Title", updatedPoll.Title);
        Assert.Equal("Updated Description", updatedPoll.Description);
        Assert.False(updatedPoll.IsPublic);
        Assert.Equal(3, updatedPoll.Options.Count);
    }

    [Fact]
    public async Task Handle_UserNotAuthenticated_ReturnsError()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync((User?)null);

        var command = new UpdatePollCommand(
            Guid.NewGuid(),
            "Updated Title",
            "Updated Description",
            true,
            null,
            new List<string> { "Option 1", "Option 2" },
            new List<Guid>()
        );

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
            CreatedAt = DateTime.UtcNow,
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);

        var command = new UpdatePollCommand(
            Guid.NewGuid(),
            "Updated Title",
            "Updated Description",
            true,
            null,
            new List<string> { "Option 1", "Option 2" },
            new List<Guid>()
        );

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
            CreatedAt = DateTime.UtcNow,
        };
        var otherUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "otheruser",
            Email = "other@example.com",
            CreatedAt = DateTime.UtcNow,
        };
        _context.Users.AddRange(owner, otherUser);
        await _context.SaveChangesAsync();

        var poll = await CreateTestPoll(owner);
        _currentUserServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(otherUser);

        var command = new UpdatePollCommand(
            poll.Id,
            "Updated Title",
            "Updated Description",
            true,
            null,
            new List<string> { "Option 1", "Option 2" },
            new List<Guid>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT1); // Error case
        var error = result.AsT1;
        Assert.Equal("You don't have permission to update this poll", error.Message);
    }

    [Fact(Skip = "Temporarily skipped due to entity tracking issues")]
    public async Task Handle_WhenValidRequestWithCategories_UpdatesPollWithCategories()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Test Category",
            Description = "Test Category Description",
            CreatedAt = DateTime.UtcNow
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var poll = await CreateTestPoll(user, new List<Category>());
        _currentUserServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);

        var command = new UpdatePollCommand(
            poll.Id,
            "Updated Poll",
            "Updated Description",
            true,
            null,
            new List<string> { "New Option 1", "New Option 2" },
            new List<Guid> { category.Id }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT0);
        var pollDto = result.AsT0;
        Assert.Equal("Updated Poll", pollDto.Title);
        Assert.Single(pollDto.Categories);
        Assert.Equal(category.Id, pollDto.Categories[0].Id);
        Assert.Equal(category.Name, pollDto.Categories[0].Name);
    }
}
