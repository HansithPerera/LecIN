using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text.Json;
using Lecin.Models;

namespace Lecin.Services;

public static class BackendClient
{
    // If you already have an AppConstants with an API base, use it; otherwise leave localhost.
    private static readonly HttpClient http = new() { BaseAddress = new Uri("http://localhost:5105/") };
    private static readonly JsonSerializerOptions json = new() { PropertyNameCaseInsensitive = true };

    public static async Task<AttendancePercentageDto> GetStudentAttendancePercentAsync(Guid studentId)
    {
        // If your endpoint is anonymous for the demo, token can be empty.
        var token = await GetAccessTokenAsync();
        http.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(token))
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var res = await http.GetAsync($"api/teacher/attendance/percentage/{studentId}");
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AttendancePercentageDto>(body, json)!;
    }

    // TODO: wire to your real Supabase token if you keep [Authorize] on the endpoint.
    private static Task<string> GetAccessTokenAsync() => Task.FromResult(string.Empty);
}
