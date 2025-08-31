using Backend.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Backend.Auth;

public class UserTypeAuthorization(AppService service) : AuthorizationHandler<ScopeRequirement>
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
        
        var hasRole = requirement.Scope switch
        {
            UserType.Admin => await service.GetAdminByIdAsync(userId) != null,
            UserType.Teacher => await service.GetTeacherByIdAsync(userId) != null,
            UserType.Student => await service.GetStudentByIdAsync(userId) != null,
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