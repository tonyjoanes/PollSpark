using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PollSpark.DTOs;
using PollSpark.Extensions;
using PollSpark.Features.Auth.Commands;
using PollSpark.Features.Auth.Queries;
using PollSpark.Features.Categories.Queries;
using PollSpark.Features.Polls.Commands;
using PollSpark.Features.Polls.Queries;
using PollSpark.Models;
using PollSpark.Services;

namespace PollSpark.Extensions;

public static class EndpointExtensions
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group
            .MapPost(
                "/register",
                async (RegisterCommand command, IMediator mediator) =>
                {
                    var result = await mediator.Send(command);
                    return result.Match(
                        success => Results.Ok(success),
                        error => Results.BadRequest(error)
                    );
                }
            )
            .WithName("Register")
            .WithDescription("Creates a new user account with the provided credentials")
            .Produces<DTOs.AuthResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group
            .MapPost(
                "/login",
                async (LoginCommand command, IMediator mediator) =>
                {
                    var result = await mediator.Send(command);
                    return result.Match(
                        success => Results.Ok(success),
                        error => Results.BadRequest(error)
                    );
                }
            )
            .WithName("Login")
            .WithDescription("Authenticates a user and returns a JWT token for subsequent requests")
            .Produces<DTOs.AuthResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);
    }

    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group
            .MapGet(
                "/profile",
                [Authorize]
                async (IMediator mediator) =>
                {
                    var result = await mediator.Send(new GetUserProfileQuery());
                    return result.Match(
                        success => Results.Ok(success),
                        error => Results.BadRequest(error)
                    );
                }
            )
            .WithName("GetUserProfile")
            .WithDescription(
                "Retrieves the profile information of the currently authenticated user"
            )
            .Produces<UserProfileResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    public static void MapPollEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/polls")
            .WithTags("Polls")
            .AddEndpointFilter(
                async (context, next) =>
                {
                    // Add rate limiting
                    var rateLimiter =
                        context.HttpContext.RequestServices.GetRequiredService<IRateLimiter>();
                    if (!await rateLimiter.TryAcquireAsync())
                    {
                        return HttpResultsExtensions.TooManyRequests();
                    }
                    return await next(context);
                }
            );

        group
            .MapPost(
                "/",
                [Authorize]
                async (CreatePollCommand command, IMediator mediator) =>
                {
                    var result = await mediator.Send(command);
                    return result.Match(
                        success => Results.Ok(success),
                        error => Results.BadRequest(error)
                    );
                }
            )
            .WithName("CreatePoll")
            .WithDescription("Creates a new poll with the provided details")
            .Produces<PollDto>(StatusCodes.Status200OK)
            .Produces<ValidationError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/",
                async (
                    IMediator mediator,
                    [FromQuery] int page = 1,
                    [FromQuery] int pageSize = 10
                ) =>
                {
                    var query = new GetPollsQuery(page, pageSize);
                    var result = await mediator.Send(query);
                    return result.Match(
                        success => Results.Ok(success),
                        error => Results.BadRequest(error)
                    );
                }
            )
            .WithName("GetPolls")
            .WithDescription("Retrieves a paginated list of polls")
            .Produces<PaginatedResponse<PollDto>>(StatusCodes.Status200OK)
            .CacheOutput(x => x.Tag("polls"));

        group
            .MapGet(
                "/{id}",
                async (Guid id, IMediator mediator) =>
                {
                    var query = new GetPollByIdQuery(id);
                    var result = await mediator.Send(query);
                    return result.Match(
                        success => Results.Ok(success),
                        error => Results.NotFound(error)
                    );
                }
            )
            .WithName("GetPollById")
            .WithDescription("Retrieves a specific poll by its ID")
            .Produces<PollDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .CacheOutput(x => x.Tag("polls"));

        group
            .MapPut(
                "/{id}",
                [Authorize]
                async (Guid id, UpdatePollCommand command, IMediator mediator) =>
                {
                    command = command with { Id = id };
                    var result = await mediator.Send(command);
                    return result.Match(
                        success => Results.Ok(success),
                        error => Results.BadRequest(error)
                    );
                }
            )
            .WithName("UpdatePoll")
            .WithDescription("Updates an existing poll")
            .Produces<PollDto>(StatusCodes.Status200OK)
            .Produces<ValidationError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group
            .MapDelete(
                "/{id}",
                [Authorize]
                async (Guid id, IMediator mediator) =>
                {
                    var command = new DeletePollCommand(id);
                    var result = await mediator.Send(command);
                    return result.Match(
                        success => Results.NoContent(),
                        error => Results.BadRequest(error)
                    );
                }
            )
            .WithName("DeletePoll")
            .WithDescription("Deletes a poll")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ValidationError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group
            .MapPost(
                "/{id}/vote",
                async (Guid id, HttpContext httpContext, IMediator mediator) =>
                {
                    // Read the request body
                    httpContext.Request.EnableBuffering();
                    var requestBody = await new StreamReader(
                        httpContext.Request.Body
                    ).ReadToEndAsync();
                    httpContext.Request.Body.Position = 0;
                    Console.WriteLine($"Raw request body: {requestBody}");

                    // Parse the request body
                    var voteRequest = System.Text.Json.JsonSerializer.Deserialize<
                        Dictionary<string, string>
                    >(requestBody);
                    Console.WriteLine(
                        $"Parsed request: {System.Text.Json.JsonSerializer.Serialize(voteRequest)}"
                    );

                    if (!Guid.TryParse(voteRequest["optionId"], out var optionId))
                    {
                        return Results.BadRequest(new ValidationError("Invalid option ID format"));
                    }

                    var command = new VoteCommand(id, optionId);
                    var result = await mediator.Send(command);
                    return result.Match(
                        success => Results.Ok(success),
                        error => Results.BadRequest(error)
                    );
                }
            )
            .WithName("VoteOnPoll")
            .WithDescription("Votes on a poll option")
            .Produces<Success>(StatusCodes.Status200OK)
            .Produces<ValidationError>(StatusCodes.Status400BadRequest)
            .AddEndpointFilter(
                async (context, next) =>
                {
                    // Add rate limiting for voting
                    var rateLimiter =
                        context.HttpContext.RequestServices.GetRequiredService<IRateLimiter>();
                    if (!await rateLimiter.TryAcquireAsync("vote", TimeSpan.FromMinutes(1)))
                    {
                        return HttpResultsExtensions.TooManyRequests(
                            "Too many votes. Please try again later."
                        );
                    }
                    return await next(context);
                }
            );

        group
            .MapGet(
                "/{id}/my-vote",
                async (Guid id, IMediator mediator) =>
                {
                    var query = new GetUserVoteQuery(id);
                    var result = await mediator.Send(query);
                    return result.Match(
                        success => Results.Ok(success),
                        error => Results.NotFound(error)
                    );
                }
            )
            .WithName("GetUserVote")
            .WithDescription("Gets the current user's vote for a poll")
            .Produces<Guid?>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group
            .MapGet(
                "/{id}/results",
                async (Guid id, IMediator mediator) =>
                {
                    var query = new GetPollResultsQuery(id);
                    var result = await mediator.Send(query);
                    return result.Match(
                        success => Results.Ok(success),
                        error => Results.NotFound(error)
                    );
                }
            )
            .WithName("GetPollResults")
            .WithDescription("Gets the results of a poll")
            .Produces<PollResultsDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group
            .MapGet(
                "/my-votes",
                async (
                    IMediator mediator,
                    [FromQuery] int page = 1,
                    [FromQuery] int pageSize = 10
                ) =>
                {
                    var query = new GetUserVotedPollsQuery(page, pageSize);
                    var result = await mediator.Send(query);
                    return result.Match(
                        success => Results.Ok(success),
                        error => Results.BadRequest(error)
                    );
                }
            )
            .WithName("GetUserVotedPolls")
            .WithDescription("Gets polls that the current user has voted on")
            .Produces<PaginatedResponse<PollDto>>(StatusCodes.Status200OK)
            .Produces<ValidationError>(StatusCodes.Status400BadRequest);
    }

    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group
            .MapGet(
                "/",
                async (IMediator mediator) =>
                {
                    var query = new GetCategoriesQuery();
                    var result = await mediator.Send(query);
                    return result.Match(
                        success => Results.Ok(success),
                        error => Results.BadRequest(error)
                    );
                }
            )
            .WithName("GetCategories")
            .WithDescription("Retrieves a list of all categories")
            .Produces<List<CategoryDto>>(StatusCodes.Status200OK)
            .CacheOutput(x => x.Tag("categories"));
    }
}
