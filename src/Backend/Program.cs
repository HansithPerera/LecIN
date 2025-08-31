using System.Reflection;
using System.Text;
using Backend;
using Backend.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var isMock = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
var testing = builder.Environment.IsEnvironment("Testing");

if (!isMock && !testing)
{
    builder.Services.AddDbContextFactory<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

    // Configure JWT Bearer authentication using the signing key from supabase.
    var signingKey = Encoding.UTF8.GetBytes(builder.Configuration["SigningKey"] ?? string.Empty);
    var supabaseProjectId = builder.Configuration["SupabaseProjectId"] ?? string.Empty;

    var validIssuers = $"https://{supabaseProjectId}.supabase.co/auth/v1";
    var validAudiences = new List<string> { "authenticated" };

    builder.Services.AddAuthentication().AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(signingKey),
            ValidAudiences = validAudiences,
            ValidIssuer = validIssuers
        };
    });
}

builder.Services.AddSingleton<IAuthorizationHandler, UserTypeAuthorization>();

// Define authorization policies based on user roles.
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