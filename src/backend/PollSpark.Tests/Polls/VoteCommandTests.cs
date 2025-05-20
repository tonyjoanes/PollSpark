using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using PollSpark.Data;
using PollSpark.Features.Polls.Commands;
using PollSpark.Models;
using Xunit;

namespace PollSpark.Tests.Polls;

public class VoteCommandTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly PollSparkContext _context;
    private readonly VoteCommandHandler _handler;

    public VoteCommandTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _context = new PollSparkContext(
            new DbContextOptionsBuilder<PollSparkContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options
        );
        _handler = new VoteCommandHandler(_context, _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task Handle_ValidVote_ReturnsSuccess()
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
                new() { Id = Guid.NewGuid(), Text = "Option 2" },
            },
        };
        await _context.Polls.AddAsync(poll);
        await _context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) })
        );
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var command = new VoteCommand(poll.Id, poll.Options.First().Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT0); // Success case
        var vote = await _context.Votes.FirstOrDefaultAsync(v =>
            v.PollId == poll.Id && v.UserId == user.Id
        );
        Assert.NotNull(vote);
        Assert.Equal(poll.Options.First().Id, vote.OptionId);
    }

    [Fact]
    public async Task Handle_PollNotFound_ReturnsError()
    {
        // Arrange
        var command = new VoteCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT1); // Error case
        var error = result.AsT1;
        Assert.Equal("Poll not found", error.Message);
    }

    [Fact]
    public async Task Handle_PollExpired_ReturnsError()
    {
        // Arrange
        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            Title = "Test Poll",
            Description = "Test Description",
            IsPublic = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            Options = new List<PollOption>
            {
                new() { Id = Guid.NewGuid(), Text = "Option 1" },
                new() { Id = Guid.NewGuid(), Text = "Option 2" },
            },
        };
        await _context.Polls.AddAsync(poll);
        await _context.SaveChangesAsync();

        var command = new VoteCommand(poll.Id, poll.Options.First().Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT1); // Error case
        var error = result.AsT1;
        Assert.Equal("Poll has expired", error.Message);
    }

    [Fact]
    public async Task Handle_InvalidOption_ReturnsError()
    {
        // Arrange
        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            Title = "Test Poll",
            Description = "Test Description",
            IsPublic = true,
            CreatedAt = DateTime.UtcNow,
            Options = new List<PollOption>
            {
                new() { Id = Guid.NewGuid(), Text = "Option 1" },
                new() { Id = Guid.NewGuid(), Text = "Option 2" },
            },
        };
        await _context.Polls.AddAsync(poll);
        await _context.SaveChangesAsync();

        var command = new VoteCommand(poll.Id, Guid.NewGuid()); // Invalid option ID

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT1); // Error case
        var error = result.AsT1;
        Assert.Equal("Invalid option for this poll", error.Message);
    }

    [Fact]
    public async Task Handle_UpdateExistingVote_ReturnsSuccess()
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
                new() { Id = Guid.NewGuid(), Text = "Option 2" },
            },
        };
        await _context.Polls.AddAsync(poll);

        var existingVote = new Vote
        {
            PollId = poll.Id,
            OptionId = poll.Options.First().Id,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
        };
        await _context.Votes.AddAsync(existingVote);
        await _context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) })
        );
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var command = new VoteCommand(poll.Id, poll.Options.Last().Id); // Vote for second option

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsT0); // Success case
        var vote = await _context.Votes.FirstOrDefaultAsync(v =>
            v.PollId == poll.Id && v.UserId == user.Id
        );
        Assert.NotNull(vote);
        Assert.Equal(poll.Options.Last().Id, vote.OptionId); // Should be updated to second option
    }
}
