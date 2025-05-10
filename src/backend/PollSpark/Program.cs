using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PollSpark.Commands.Auth;
using PollSpark.Data;
using PollSpark.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using PollSpark.Queries.GetUserProfile;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddDbContext<PollSparkContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Auth Service
builder.Services.AddScoped<IAuthService, AuthService>();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Add auth endpoints
app.MapPost("/api/auth/register", async (RegisterCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return result.Match(success => Results.Ok(success), error => Results.BadRequest(error));
});

app.MapPost("/api/auth/login", async (LoginCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return result.Match(success => Results.Ok(success), error => Results.BadRequest(error));
});

// Add this with other endpoints
app.MapGet("/api/users/profile", [Authorize] async (IMediator mediator) =>
{
    var result = await mediator.Send(new GetUserProfileQuery());
    return result.Match(
        success => Results.Ok(success),
        error => Results.BadRequest(error)
    );
});

// Add your endpoints here
app.MapGet("/", () => "Welcome to PollSpark!");

app.Run();
