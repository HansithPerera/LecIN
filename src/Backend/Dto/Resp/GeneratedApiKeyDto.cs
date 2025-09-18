namespace Backend.Dto.Resp;

public class GeneratedApiKeyDto
{
    public required Guid ApiKeyId { get; set; }
    
    public required string ApiKey { get; set; }
    public required string Prefix { get; set; }
}