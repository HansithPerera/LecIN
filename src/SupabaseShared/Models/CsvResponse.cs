using Newtonsoft.Json;

namespace SupabaseShared.Models;

public class CsvResponse
{
    [JsonProperty("csv")] public string Csv { get; set; } = string.Empty;
}