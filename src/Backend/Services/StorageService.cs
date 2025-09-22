namespace Backend.Services;

public class StorageService(Supabase.Client client)
{
    public async Task<Stream?> DownloadAsync(string bucketName, string path)
    {
        var data = await client.Storage.From(bucketName).Download(path, null);
        var memoryStream = new MemoryStream();
        await memoryStream.WriteAsync(data);
        memoryStream.Position = 0;
        return memoryStream;
    }
}