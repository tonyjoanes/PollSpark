using MediatR;
using Microsoft.AspNetCore.Authorization;
using PollSpark.Features.Auth.Commands;
using PollSpark.Features.Auth.Queries;

namespace PollSpark.Extensions;

public static class EndpointExtensions
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost(
            "/register",
            async (RegisterCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result.Match(
                    success => Results.Ok(success),
                    error => Results.BadRequest(error)
                );
            }
        );

        group.MapPost(
            "/login",
            async (LoginCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result.Match(
                    success => Results.Ok(success),
                    error => Results.BadRequest(error)
                );
            }
        );
    }

    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet(
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
        );
    }
}
