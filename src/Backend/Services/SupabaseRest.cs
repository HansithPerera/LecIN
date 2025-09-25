
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;

namespace Backend.Services;

public class SupabaseRest(IConfiguration cfg, IHttpClientFactory httpFactory, ILogger<SupabaseRest> logger)
{
    private readonly string _base = $"https://{cfg["Supabase:ProjectId"]}.supabase.co/rest/v1/";
    private readonly string _serviceKey = cfg["Supabase:ServiceKey"] ?? throw new InvalidOperationException("Missing Supabase:ServiceKey");
    private readonly IHttpClientFactory _httpFactory = httpFactory;

    // Keep server property names exactly as written (PascalCase) and be lenient when reading
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private HttpClient Client()
    {
        var c = _httpFactory.CreateClient(nameof(SupabaseRest));
        c.BaseAddress = new Uri(_base);
        c.DefaultRequestHeaders.Clear();
        c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        c.DefaultRequestHeaders.Add("apikey", _serviceKey);
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _serviceKey);
        // ask PostgREST to return the inserted row(s)
        c.DefaultRequestHeaders.Add("Prefer", "return=representation,count=exact");
        return c;
    }

    public async Task<T[]?> GetAsync<T>(string pathAndQuery)
    {
        using var c = Client();
        var res = await c.GetAsync(pathAndQuery);
        if (!res.IsSuccessStatusCode)
        {
            logger.LogError("GET {Url} failed: {Code} {Body}", c.BaseAddress + pathAndQuery, (int)res.StatusCode, await res.Content.ReadAsStringAsync());
            return null;
        }
        return await res.Content.ReadFromJsonAsync<T[]>(JsonOpts);
    }



    public async Task<T[]?> InsertAsync<T>(
    string table, object payload, string? onConflict = null, bool ignoreDuplicates = false)
    {
        using var c = Client();
        var url = table + (onConflict is not null ? $"?on_conflict={Uri.EscapeDataString(onConflict)}" : "");

        // Add Prefer for ignore-duplicates in addition to return=representation
        if (ignoreDuplicates)
            c.DefaultRequestHeaders.Add("Prefer", "resolution=ignore-duplicates");

        var json = JsonSerializer.Serialize(payload, JsonOpts);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        content.Headers.Add("Prefer", "return=representation");

        var res = await c.PostAsync(url, content);
        if (!res.IsSuccessStatusCode)
        {
            logger.LogError("INSERT {Table} failed: {Code} {Body}",
                table, (int)res.StatusCode, await res.Content.ReadAsStringAsync());
            return null;
        }
        return await res.Content.ReadFromJsonAsync<T[]>(JsonOpts);
    }
    /*
    public async Task<T[]?> InsertAsync<T>(string table, object payload)
    {
        using var c = Client();

        // Serialize with PascalCase keys preserved
        var json = JsonSerializer.Serialize(payload, JsonOpts);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        content.Headers.Add("Prefer", "return=representation");

        var res = await c.PostAsync(table, content);
        if (!res.IsSuccessStatusCode)
        {
            logger.LogError("INSERT {Table} failed: {Code} {Body}", table, (int)res.StatusCode, await res.Content.ReadAsStringAsync());
            return null;
        }
        return await res.Content.ReadFromJsonAsync<T[]>(JsonOpts);
    }
    */
}



/*
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Net.Http.Headers;

namespace Backend.Services;

public class SupabaseRest(IConfiguration cfg, IHttpClientFactory httpFactory, ILogger<SupabaseRest> logger)
{
    private readonly string _base = $"https://{cfg["Supabase:ProjectId"]}.supabase.co/rest/v1";
    private readonly string _serviceKey = cfg["Supabase:ServiceKey"] ?? throw new InvalidOperationException("Missing Supabase:ServiceKey");
    private readonly IHttpClientFactory _httpFactory = httpFactory;

    private HttpClient Client()
    {
        var c = _httpFactory.CreateClient(nameof(SupabaseRest));
        c.DefaultRequestHeaders.Clear();
        c.DefaultRequestHeaders.Add("apikey", _serviceKey);
        c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _serviceKey);
        // ask PostgREST to include row-count in Content-Range if we ever need it
        c.DefaultRequestHeaders.Add("Prefer", "return=representation,count=exact");
        return c;
    }

    public async Task<T[]?> GetAsync<T>(string pathAndQuery)
    {
        using var c = Client();
        var url = $"{_base}/{pathAndQuery}";
        var res = await c.GetAsync(url);
        if (!res.IsSuccessStatusCode)
        {
            logger.LogError("GET {Url} failed: {Code} {Body}", url, (int)res.StatusCode, await res.Content.ReadAsStringAsync());
            return null;
        }
        return await res.Content.ReadFromJsonAsync<T[]>();
    }

    public async Task<T[]?> InsertAsync<T>(string table, object payload)
    {
        using var c = Client();
        var url = $"{_base}/{table}";
        var res = await c.PostAsJsonAsync(url, payload);
        if (!res.IsSuccessStatusCode)
        {
            logger.LogError("INSERT {Table} failed: {Code} {Body}", table, (int)res.StatusCode, await res.Content.ReadAsStringAsync());
            return null;
        }
        return await res.Content.ReadFromJsonAsync<T[]>();
    }
}
*/