using System.Security.Cryptography;
using System.Text;

namespace Backend;

public static class Util
{
    public static string GenerateApiKey()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    public static string GetApiKeyPrefix(string apiKey)
    {
        return apiKey[..8];
    }

    public static string HashApiKey(string apiKey)
    {
        var bytes = Encoding.UTF8.GetBytes(apiKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}