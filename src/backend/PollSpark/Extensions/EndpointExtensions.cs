using MediatR;
using Microsoft.AspNetCore.Authorization;
using PollSpark.Features.Auth.Commands;
using PollSpark.Features.Auth.Queries;
using PollSpark.Features.Polls.Commands;
using PollSpark.Models;

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
            .WithDescription("Retrieves the profile information of the currently authenticated user")
            .Produces<UserProfileResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    public static void MapPollEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/polls").WithTags("Polls");

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
            .Produces<Poll>(StatusCodes.Status200OK)
            .Produces<ValidationError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
