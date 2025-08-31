using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Tokens;

namespace Backend;

public static class Util
{
    public static string GetSupabaseIssuer(string projectRef)
    {
        return $"https://{projectRef}.supabase.co/auth/v1";
    }

    public static SymmetricSecurityKey GetSymmetricSecurityKey([Base64String] string base64Secret)
    {
        var bytes = Convert.FromBase64String(base64Secret);
        return new SymmetricSecurityKey(bytes);
    }
    
    public static string GetCacheKey(string prefix, params object[] parts)
    {
        return $"{prefix}:{string.Join(":", parts)}";
    }
}