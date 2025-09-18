using Backend.Database;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Auth;

public class AdminPermissionAuthorization(Repository service) : AuthorizationHandler<AdminPermRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        AdminPermRequirement requirement)
    {
        var userId = context.User.Identity?.Name;
        if (!Guid.TryParse(userId, out var userGuid))
        {
            context.Fail();
            return;
        }

        var admin = await service.GetAdminByIdAsync(userGuid);
        if (admin != null && (admin.Permissions & requirement.Perms) == requirement.Perms)
            context.Succeed(requirement);
        else
            context.Fail();
    }
}

public class AdminPermRequirement(AdminPermissions perms) : IAuthorizationRequirement
{
    public AdminPermissions Perms { get; } = perms;
}