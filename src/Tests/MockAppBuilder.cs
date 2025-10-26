using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Program = Backend.Program;

namespace Tests;

public class MockAppBuilder : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var kvps = new List<KeyValuePair<string, string?>>
        {
            new("Supabase:Url", "http://127.0.0.1:54321"),
            new("Supabase:Key",
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImV4cCI6MTk4MzgxMjk5Nn0.EGIM96RAZx35lJzdJsyH-qQwv8Hdp7fsn3W0YpN81IU")
        };

        builder.UseConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(kvps)
            .Build());
    }
}