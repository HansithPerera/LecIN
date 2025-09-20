using System.Security.Claims;
using Backend.Database;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Auth;

public class UserTypeAuthorization(Repository service) : AuthorizationHandler<ScopeRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ScopeRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var userGuid))
        {
            context.Fail();
            return;
        }

        var hasRole = requirement.Scope switch
        {
            UserType.Admin => await service.GetAdminByIdAsync(userGuid) != null,
            UserType.Teacher => await service.GetTeacherByIdAsync(userGuid) != null,
            UserType.Student => await service.GetStudentByIdAsync(userGuid) != null,
            _ => false
        };

        if (hasRole)
            context.Succeed(requirement);
        else
            context.Fail();
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