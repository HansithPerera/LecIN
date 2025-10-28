using System.Reflection;
using Backend;
using Backend.Auth;
using Backend.Database;
using Backend.Face;
using Backend.Services;
using Emgu.CV;
using Emgu.CV.Face;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var isMock = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";

// Configure the database context and authentication only if not in mock or testing mode.
if (!isMock)
{
    builder.Services.AddAuthentication(Constants.ApiKeyAuthScheme)
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            Constants.ApiKeyAuthScheme,
            _ => { }
        );

    builder.Services.AddSingleton<Supabase.Client>(_ =>
        new Supabase.Client(
            builder.Configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase URL is not configured."),
            builder.Configuration["Supabase:Key"] ?? throw new InvalidOperationException("Supabase Key is not configured.")
        )
    );
    
    builder.Services.AddHostedService<FaceTrainingWorker>();
}

// Repository service for data access.
builder.Services.AddSingleton<Repository>();

// Face recognition services.
builder.Services.AddSingleton<CascadeClassifier>(
    _ => new CascadeClassifier("haarcascade_frontalface_default.xml")
);

builder.Services.AddSingleton<LBPHFaceRecognizer>(
    _ => new LBPHFaceRecognizer(1, 8, 8, 8, 100)
);
builder.Services.AddSingleton<FaceService>();
builder.Services.AddSingleton<StorageService>();

// Application services.
builder.Services.AddSingleton<CameraService>();

builder.Services.AddSingleton<IAuthorizationHandler, IntegrationAuthorizationHandler>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Constants.CameraAuthorizationPolicy,
        policy => policy.Requirements.Add(new IntegrationRequirement(IntegrationType.Camera)));

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

builder.Services.AddControllers();

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