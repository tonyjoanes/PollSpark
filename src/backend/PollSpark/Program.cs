using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PollSpark.Data;
using PollSpark.Extensions;
using PollSpark.Features.Auth.Services;
using PollSpark.Handlers;
using PollSpark.Models;
using PollSpark.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "PollSpark API",
            Version = "v1",
            Description = "API for PollSpark application",
        }
    );

    // Add JWT Authentication support in Swagger
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Configure JSON serialization
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    options.SerializerOptions.WriteIndented = true;
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddDbContext<PollSparkContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<PollSparkContext>()
    .AddDefaultTokenProviders();

// Add Auth Service
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure JWT Authentication
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
        };
    });

builder.Services.AddAuthorization();

// Add Rate Limiting
builder.Services.AddSingleton<IRateLimiter, MemoryRateLimiter>();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add CORS middleware
app.UseCors();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapPollEndpoints();
app.MapCategoryEndpoints();

// Hashtag endpoints
app.MapGet(
    "/api/polls/hashtag/{hashtag}",
    async (
        string hashtag,
        IMediator mediator,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 10
    ) =>
    {
        var result = await mediator.Send(new GetPollsByHashtag.Query(hashtag, page, pageSize));
        return result.Match(polls => Results.Ok(polls), error => Results.NotFound(error.Message));
    }
);

app.MapGet(
    "/api/hashtags/popular",
    async (IMediator mediator) =>
    {
        var result = await mediator.Send(new GetPopularHashtags.Query());
        return result.Match(
            hashtags => Results.Ok(hashtags),
            error => Results.NotFound(error.Message)
        );
    }
);

// Add your endpoints here
app.MapGet("/", () => "Welcome to PollSpark!");

// Add this after the app.Build() call
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PollSparkContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    await context.Database.MigrateAsync();
    await DbSeeder.SeedCategories(context);
    await DbSeeder.SeedPolls(context, userManager);
}

app.Run();
