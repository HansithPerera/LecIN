using System.Drawing.Imaging;
using Backend.Api;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Camera.Services;

public class Worker(FaceService service, OpenApiClient client, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            
            var capture = new VideoCapture(1);
            var frame = capture.QueryFrame();
            if (frame == null)
            {
                continue;
            }
            
            var image = frame.ToImage<Bgr, byte>();
            var processedFaces = await service.ProcessFrameAsync(image);
            if (processedFaces.Count > 0)
            {
                var files = new List<FileParameter>();
                foreach (var face in processedFaces)
                {
                    
                    var ms = new MemoryStream();
                    face.ToBitmap()?.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    files.Add(new FileParameter(ms, $"{Guid.NewGuid()}.png", "image/png"));
                }
                try
                {
                    await client.UploadFacesAsync(files, stoppingToken);
                }
                catch (Exception e)
                {
                    logger.LogError("Error uploading faces: {message}", e.Message);
                }
            }
            logger.LogInformation("Detected {count} faces", processedFaces.Count);
            await Task.Delay(1000, stoppingToken);
        }
    }
}