using Backend.Database;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Auth;

public class IntegrationAuthorizationHandler(AppService service) : AuthorizationHandler<IntegrationRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        IntegrationRequirement requirement)
    {
        var userId = context.User.Identity?.Name;
        if (!Guid.TryParse(userId, out var apiKeyId)) context.Fail();
        var authorized = requirement.Type switch
        {
            IntegrationType.Camera => await service.IsCameraApiKey(apiKeyId),
            _ => false
        };
        if (authorized)
            context.Succeed(requirement);
        else
            context.Fail();
    }
}

public enum IntegrationType
{
    Camera
}

public class IntegrationRequirement(IntegrationType type) : IAuthorizationRequirement
{
    public IntegrationType Type { get; } = type;
}