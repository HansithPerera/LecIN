using System.Reflection;
using Backend;
using Backend.Auth;
using Backend.Database;
using Backend.Face;
using Backend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddUserSecrets<Backend.Program>();

var isMock = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
var testing = builder.Environment.IsEnvironment(Constants.TestingEnv);

// Configure the database context and authentication only if not in mock or testing mode.
if (!isMock && !testing)
{
    // Configure the database context to use PostgreSQL with the connection string from configuration.
    builder.Services.AddDbContextFactory<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))

    );

    builder.Services.AddAuthentication(Constants.ApiKeyAuthScheme)
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            Constants.ApiKeyAuthScheme,
            _ => { }
        );
}

// Repository service for data access.
builder.Services.AddSingleton<Repository>();
builder.Services.AddSingleton<FaceService>();

// Application services.
builder.Services.AddSingleton<CameraService>();

builder.Services.AddSingleton<IAuthorizationHandler, IntegrationAuthorizationHandler>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Constants.CameraAuthorizationPolicy,
        policy => policy.Requirements.Add(new IntegrationRequirement(IntegrationType.Camera)));

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

builder.Services.AddControllers();

Console.WriteLine("[DefaultConnection] " + builder.Configuration.GetConnectionString("DefaultConnection"));


var app = builder.Build();

if (app.Environment.IsEnvironment(Constants.DevelopmentEnv)) app.MapOpenApi();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make the implicit Program class public so test projects can access it
namespace Backend
{
    public class Program
    {
    }
}