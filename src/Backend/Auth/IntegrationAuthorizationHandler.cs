using System.Security.Claims;
using Backend.Database;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Auth;

public class IntegrationAuthorizationHandler(Repository service) : AuthorizationHandler<IntegrationRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        IntegrationRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
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