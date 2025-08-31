using System.Reflection;
using Backend;
using Backend.Auth;
using Backend.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var isMock = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
var testing = builder.Environment.IsEnvironment("Testing");

// Configure the database context and authentication only if not in mock or testing mode.
if (!isMock && !testing)
{
    // Configure the database context to use PostgreSQL with the connection string from configuration.
    builder.Services.AddDbContextFactory<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
    
    // Configure JWT Bearer authentication using the signing key from supabase.
    builder.Services.AddAuthentication().AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = Util.GetSymmetricSecurityKey(builder.Configuration["SigningKey"] ?? string.Empty),
            ValidAudiences = ["authenticated"],
            ValidIssuer = Util.GetSupabaseIssuer(builder.Configuration["SupabaseProjectId"] ?? string.Empty)
        };
    });
}

// Repository service for data access.
builder.Services.AddSingleton<AppService>();

// In-memory caching service.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<AppCache>();

// Define authorization policies based on user roles.
builder.Services.AddSingleton<IAuthorizationHandler, UserTypeAuthorization>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Constants.AdminAuthorizationPolicy,
        policy => policy.Requirements.Add(new ScopeRequirement(UserType.Admin)))
    .AddPolicy(Constants.TeacherAuthorizationPolicy,
        policy => policy.Requirements.Add(new ScopeRequirement(UserType.Teacher)))
    .AddPolicy(Constants.StudentAuthorizationPolicy,
        policy => policy.Requirements.Add(new ScopeRequirement(UserType.Student)));

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsEnvironment("Dev")) app.MapOpenApi();

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