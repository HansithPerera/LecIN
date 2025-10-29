using Backend.Database;
using Backend.Face;

namespace Backend.Services;

public class FaceTrainingWorker(StorageService storage, Repository repo, FaceService service): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var faces = await repo.GetAllStudentFacesAsync();
        foreach (var face in faces)
        {
            try
            {
                var stream = await storage.DownloadAsync("studentfaces", face.FaceImagePath);
                if (stream == null) continue;
                await service.TrainOnFaceAsync(face.StudentId, stream);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error training on face {face.StudentId}: {e.Message}");
            }
        }
        
    }
}