using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using OneOf;
using PollSpark.Data;
using PollSpark.DTOs;
using PollSpark.Features.Auth.Services;
using PollSpark.Features.Polls.Commands;
using PollSpark.Models;
using TechTalk.SpecFlow;
using Xunit;

namespace PollSpark.Tests.Features.Polls
{
    [Binding]
    public class UpdatePollSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly PollSparkContext _context;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly UpdatePollCommandHandler _handler;
        private Poll _poll;
        private OneOf<PollDto, ValidationError> _result;

        public UpdatePollSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            var options = new DbContextOptionsBuilder<PollSparkContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new PollSparkContext(options);
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _handler = new UpdatePollCommandHandler(_context, _currentUserServiceMock.Object);
        }

        [Given(@"I am an authenticated user")]
        public void GivenIAmAnAuthenticatedUser()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };
            _currentUserServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
        }

        [Given(@"I have created a poll with title ""(.*)""")]
        public async Task GivenIHaveCreatedAPollWithTitle(string title)
        {
            _poll = new Poll
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = "Test description",
                CreatedById = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
            };
            await _context.Polls.AddAsync(_poll);
            await _context.SaveChangesAsync();
        }

        [Given(@"the poll has options ""(.*)"" and ""(.*)""")]
        public async Task GivenThePollHasOptions(string option1, string option2)
        {
            var options = new[]
            {
                new PollOption
                {
                    Id = Guid.NewGuid(),
                    PollId = _poll.Id,
                    Text = option1,
                },
                new PollOption
                {
                    Id = Guid.NewGuid(),
                    PollId = _poll.Id,
                    Text = option2,
                },
            };
            await _context.PollOptions.AddRangeAsync(options);
            await _context.SaveChangesAsync();
        }

        [Given(@"I am not authenticated")]
        public void GivenIAmNotAuthenticated()
        {
            _currentUserServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync((User)null);
        }

        [Given(@"there is a poll created by another user")]
        public async Task GivenThereIsAPollCreatedByAnotherUser()
        {
            _poll = new Poll
            {
                Id = Guid.NewGuid(),
                Title = "Other User's Poll",
                Description = "Test description",
                CreatedById = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
            };
            await _context.Polls.AddAsync(_poll);
            await _context.SaveChangesAsync();
        }

        [When(@"I update the poll with new title ""(.*)""")]
        public async Task WhenIUpdateThePollWithNewTitle(string newTitle)
        {
            var command = new UpdatePollCommand(
                _poll.Id,
                newTitle,
                _poll.Description,
                _poll.IsPublic,
                _poll.ExpiresAt,
                _poll.Options.Select(o => o.Text).ToList()
            );
            _result = await _handler.Handle(command, default);
        }

        [When(@"I update the description to ""(.*)""")]
        public async Task WhenIUpdateTheDescriptionTo(string newDescription)
        {
            var command = new UpdatePollCommand(
                _poll.Id,
                _poll.Title,
                newDescription,
                _poll.IsPublic,
                _poll.ExpiresAt,
                _poll.Options.Select(o => o.Text).ToList()
            );
            _result = await _handler.Handle(command, default);
        }

        [When(@"I update the options to ""(.*)"" and ""(.*)""")]
        public async Task WhenIUpdateTheOptionsTo(string newOption1, string newOption2)
        {
            var command = new UpdatePollCommand(
                _poll.Id,
                _poll.Title,
                _poll.Description,
                _poll.IsPublic,
                _poll.ExpiresAt,
                new[] { newOption1, newOption2 }.ToList()
            );
            _result = await _handler.Handle(command, default);
        }

        [When(@"I try to update the poll")]
        public async Task WhenITryToUpdateThePoll()
        {
            var command = new UpdatePollCommand(
                _poll.Id,
                "Updated Title",
                "Updated Description",
                _poll.IsPublic,
                _poll.ExpiresAt,
                new[] { "New Option" }.ToList()
            );
            _result = await _handler.Handle(command, default);
        }

        [When(@"I try to update a poll with ID ""(.*)""")]
        public async Task WhenITryToUpdateAPollWithID(string pollId)
        {
            var command = new UpdatePollCommand(
                Guid.Parse(pollId),
                "Updated Title",
                "Updated Description",
                true,
                null,
                new[] { "New Option" }.ToList()
            );
            _result = await _handler.Handle(command, default);
        }

        [Then(@"the poll should be updated successfully")]
        public void ThenThePollShouldBeUpdatedSuccessfully()
        {
            Assert.True(_result.IsT0);
        }

        [Then(@"the poll should have the new title ""(.*)""")]
        public async Task ThenThePollShouldHaveTheNewTitle(string expectedTitle)
        {
            var updatedPoll = await _context.Polls.FindAsync(_poll.Id);
            Assert.Equal(expectedTitle, updatedPoll.Title);
        }

        [Then(@"the poll should have the new description ""(.*)""")]
        public async Task ThenThePollShouldHaveTheNewDescription(string expectedDescription)
        {
            var updatedPoll = await _context.Polls.FindAsync(_poll.Id);
            Assert.Equal(expectedDescription, updatedPoll.Description);
        }

        [Then(@"the poll should have the new options ""(.*)"" and ""(.*)""")]
        public async Task ThenThePollShouldHaveTheNewOptions(
            string expectedOption1,
            string expectedOption2
        )
        {
            var updatedPoll = await _context
                .Polls.Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == _poll.Id);
            Assert.Equal(2, updatedPoll.Options.Count);
            Assert.Contains(updatedPoll.Options, o => o.Text == expectedOption1);
            Assert.Contains(updatedPoll.Options, o => o.Text == expectedOption2);
        }

        [Then(@"I should receive an error message ""(.*)""")]
        public void ThenIShouldReceiveAnErrorMessage(string expectedMessage)
        {
            Assert.True(_result.IsT1);
            var error = _result.AsT1;
            Assert.Equal(expectedMessage, error.Message);
        }
    }
}
