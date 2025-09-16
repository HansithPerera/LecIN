using System.Security.Claims;
using System.Text.Encodings.Web;
using Backend.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Backend.Auth;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    AppService service)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
        {
            var hash = Util.HashApiKey(apiKey!);
            var camera = await service.GetCameraByApiKeyHashAsync(hash);
            if (camera != null)
            {
                var claims = new[] { new Claim(ClaimTypes.Name, camera.Id) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            else
            {
                return AuthenticateResult.Fail("Invalid API Key.");
            }
        }
        else
        {
            return AuthenticateResult.Fail("Missing or invalid API Key.");
        }
    }
}