using System.Reflection;
using Backend;
using Backend.Auth;
using Backend.Database;
using Backend.Face;
using Backend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var isMock = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
var testing = builder.Environment.IsEnvironment(Constants.TestingEnv);

// Configure the database context and authentication only if not in mock or testing mode.
if (!isMock && !testing)
{
    // Configure the database context to use PostgreSQL with the connection string from configuration.
    builder.Services.AddDbContextFactory<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

    builder.Services.AddAuthentication(o => { o.DefaultAuthenticateScheme = Constants.JwtAuthScheme; })
        .AddJwtBearer(Constants.JwtAuthScheme, o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = Util.GetSymmetricSecurityKey(builder.Configuration["SigningKey"] ?? string.Empty),
                ValidAudiences = [Constants.SupabaseAuthenticatedRole],
                ValidIssuer = Util.GetSupabaseIssuer(builder.Configuration["SupabaseProjectId"] ?? string.Empty)
            };
        })
        // Configure API Key authentication using apiKey header.
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            Constants.ApiKeyAuthScheme,
            _ => { }
        );
}

// Repository service for data access.
builder.Services.AddSingleton<AppService>();
builder.Services.AddSingleton<FaceService>();

// In-memory caching service.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<AppCache>();

// Define authorization policies based on user roles.
builder.Services.AddSingleton<IAuthorizationHandler, UserTypeAuthorization>();
builder.Services.AddSingleton<IAuthorizationHandler, AdminPermissionAuthorization>();
builder.Services.AddSingleton<IAuthorizationHandler, IntegrationAuthorizationHandler>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Constants.CameraAuthorizationPolicy,
        policy => policy.Requirements.Add(new IntegrationRequirement(IntegrationType.Camera)))
    .AddPolicy(Constants.AdminAuthorizationPolicy,
        policy => policy.Requirements.Add(new ScopeRequirement(UserType.Admin)))
    .AddPolicy(Constants.TeacherAuthorizationPolicy,
        policy => policy.Requirements.Add(new ScopeRequirement(UserType.Teacher)))
    .AddPolicy(Constants.StudentAuthorizationPolicy,
        policy => policy.Requirements.Add(new ScopeRequirement(UserType.Student)))
    .AddPolicy(Constants.AdminManageApiKeysPermission,
        policy => policy.Requirements.Add(new AdminPermRequirement(AdminPermissions.ManageApiKeys)))
    .AddPolicy(Constants.AdminReadApiKeysPermission,
        policy => policy.Requirements.Add(new AdminPermRequirement(AdminPermissions.ReadApiKeys)))
    .AddPolicy(Constants.AdminManageCamerasPermission,
        policy => policy.Requirements.Add(new AdminPermRequirement(AdminPermissions.ManageCameras)))
    .AddPolicy(Constants.AdminReadCamerasPermission,
        policy => policy.Requirements.Add(new AdminPermRequirement(AdminPermissions.ReadCameras)))
    .AddPolicy(Constants.AdminManageCoursesPermission,
        policy => policy.Requirements.Add(new AdminPermRequirement(AdminPermissions.ManageCourses)))
    .AddPolicy(Constants.AdminManageReportsPermission,
        policy => policy.Requirements.Add(new AdminPermRequirement(AdminPermissions.ManageReports)))
    .AddPolicy(Constants.AdminManageTeachersPermission,
        policy => policy.Requirements.Add(new AdminPermRequirement(AdminPermissions.ManageTeachers)))
    .AddPolicy(Constants.AdminManageStudentsPermission,
        policy => policy.Requirements.Add(new AdminPermRequirement(AdminPermissions.ManageStudents)))
    .AddPolicy(Constants.AdminExportDataPermission,
        policy => policy.Requirements.Add(new AdminPermRequirement(AdminPermissions.ExportData)))
    .AddPolicy(Constants.AdminReadTeachersPermission,
        policy => policy.Requirements.Add(new AdminPermRequirement(AdminPermissions.ReadTeachers)))
    .AddPolicy(Constants.AdminReadStudentsPermission,
        policy => policy.Requirements.Add(new AdminPermRequirement(AdminPermissions.ReadStudents)))
    .AddPolicy(Constants.AdminReadCoursesPermission,
        policy => policy.Requirements.Add(new AdminPermRequirement(AdminPermissions.ReadCourses)))
    .AddPolicy(Constants.AdminReadReportsPermission,
        policy => policy.Requirements.Add(new AdminPermRequirement(AdminPermissions.ReadReports)));

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