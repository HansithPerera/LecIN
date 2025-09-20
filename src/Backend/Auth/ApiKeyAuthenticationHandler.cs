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
    Repository service)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Constants.ApiKeyHeaderName, out var apiKey))
            return AuthenticateResult.Fail("Missing or invalid API Key.");

        var hash = Util.HashApiKey(apiKey!);
        var camera = await service.GetApiKeyByHashAsync(hash);
        if (camera == null) return AuthenticateResult.Fail("Invalid API Key.");

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, camera.Id.ToString()), new Claim(ClaimTypes.NameIdentifier, camera.Id.ToString())
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}