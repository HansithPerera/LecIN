using System.Net.Http.Headers;
using Supabase;

namespace Lecin;

public class AuthHandler(Client client) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (client.Auth.CurrentSession?.AccessToken != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer" , client.Auth.CurrentSession.AccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}