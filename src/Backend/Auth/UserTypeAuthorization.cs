using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Backend;

public class UserTypeAuthorization(IDbContextFactory<AppDbContext> ctxFactory): AuthorizationHandler<ScopeRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ScopeRequirement requirement)
    {
        var userId = context.User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        await using var ctx = await ctxFactory.CreateDbContextAsync();

        var hasRole = requirement.Scope switch
        {
            UserType.Admin => throw new NotImplementedException("Admin role check not implemented"),
            UserType.Teacher => await ctx.Teachers.AnyAsync(t => t.Id == userId),
            UserType.Student => await ctx.Students.AnyAsync(s => s.Id == userId),
            _ => false
        };

        if (hasRole)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}

public enum UserType
{
    Admin,
    Teacher,
    Student
}

public class ScopeRequirement(UserType scope) : IAuthorizationRequirement
{
    public UserType Scope { get; } = scope;
}
