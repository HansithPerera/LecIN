using System.Reflection;
using System.Text;
using Backend;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// If the application is running as a mock during OPENAPI generation, skip collecting these values.
var isMock = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";

builder.Services.AddDbContextFactory<AppDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."))
);

var signingKey = isMock 
    ? null
    : Encoding.UTF8.GetBytes(builder.Configuration["SigningKey"] ?? throw new InvalidOperationException("SigningKey not found in config"));
var supabaseProjectId = isMock
    ? null
    : builder.Configuration["SupabaseProjectId"] ?? throw new InvalidOperationException("SupabaseProjectId not found in config");

var validIssuers = $"https://{supabaseProjectId}.supabase.co/auth/v1";
var validAudiences = new List<string> { "authenticated" };

// Configure JWT Bearer authentication using the signing key from supabase.
builder.Services.AddAuthentication().AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(signingKey),
        ValidAudiences = validAudiences,
        ValidIssuer = validIssuers
    };
});

builder.Services.AddSingleton<IAuthorizationHandler, UserTypeAuthorization>();

// Define authorization policies based on user roles.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Constants.AdminAuthorizationPolicy, policy => policy.Requirements.Add(new ScopeRequirement(UserType.Admin)));
    options.AddPolicy(Constants.TeacherAuthorizationPolicy, policy => policy.Requirements.Add(new ScopeRequirement(UserType.Teacher)));
    options.AddPolicy(Constants.StudentAuthorizationPolicy, policy => policy.Requirements.Add(new ScopeRequirement(UserType.Student)));
});

// In development, use a test authentication handler that always authenticates the user as an admin.
if (builder.Environment.IsEnvironment("Dev"))
{
    builder.Services.AddAuthentication("Test")
        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", null);
}

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsEnvironment("Dev"))
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();