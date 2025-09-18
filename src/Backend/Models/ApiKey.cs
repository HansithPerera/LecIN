using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("ApiKeys")]
public class ApiKey
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(128)] 
    public required string Hash { get; set; }

    public required bool IsActive { get; set; }

    public string? Name { get; set; }

    [MaxLength(100)] 
    public required string Prefix { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }

    public static ApiKey Create(out string unhashedKey, string? name = null)
    {
        unhashedKey = Util.GenerateApiKey();
        return new ApiKey
        {
            Hash = Util.HashApiKey(unhashedKey),
            IsActive = true,
            Name = name,
            Prefix = Util.GetApiKeyPrefix(unhashedKey),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}