using Camera.Services;
using FaceONNX;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);


builder.Services.AddSingleton<Client>(_ =>
    new Client(
        builder.Configuration["Supabase:Url"] ??
        throw new InvalidOperationException("Supabase URL is not configured."),
        builder.Configuration["Supabase:Key"] ??
        throw new InvalidOperationException("Supabase Key is not configured.")
    )
);

// Face recognition services.
builder.Services.AddSingleton<FaceDetector>();

builder.Services.AddSingleton<FaceEmbedder>();

builder.Services.AddSingleton<FrameBuffer>();

builder.Services.AddHostedService<Worker>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();

app.MapControllers();

app.Run();

// Make the implicit Program class public so test projects can access it
namespace Camera
{
    public class Program
    {
    }
}